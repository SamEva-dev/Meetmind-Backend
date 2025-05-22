import os
import sys
import json
from fastapi import FastAPI, File, UploadFile, Form, Header
from fastapi.responses import JSONResponse
import tempfile

# Forcer UTF-8 sur Windows
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

# ---- Logger
try:
    import logger_config
    logger = logger_config.logger
    logger.info("Logger loaded (from logger_config.py)")
except Exception as e:
    print(json.dumps({"error": f"Logger config failed: {e}"}))
    sys.exit(1)

# ---- Import IA
try:
    from faster_whisper import WhisperModel
    logger.info("faster-whisper importé avec succès")
except ImportError as e:
    logger.error(f"faster-whisper not installed: {e}")
    sys.exit(1)

try:
    from pyannote.audio import Pipeline
    logger.info("pyannote.audio importé avec succès")
except ImportError as e:
    logger.error(f"pyannote.audio not installed: {e}")
    sys.exit(1)

app = FastAPI(title="Transcription+Diarization Worker")

# ---- Caches pour les modèles chargés à la volée
whisper_models_cache = {}
pyannote_pipelines_cache = {}

def get_whisper_model(model_name, device, compute_type):
    key = (model_name, device, compute_type)
    if key not in whisper_models_cache:
        logger.info(f"Loading WhisperModel: {key}")
        whisper_models_cache[key] = WhisperModel(model_name, device=device, compute_type=compute_type)
    return whisper_models_cache[key]

def get_pyannote_pipeline(model_name, hf_token):
    key = (model_name, hf_token)
    if key not in pyannote_pipelines_cache:
        logger.info(f"Loading pyannote pipeline: {key}")
        pyannote_pipelines_cache[key] = Pipeline.from_pretrained(model_name, use_auth_token=hf_token)
    return pyannote_pipelines_cache[key]

def format_timestamp(secs):
    hrs = int(secs // 3600)
    mins = int((secs % 3600) // 60)
    secs_f = secs % 60
    return f"{hrs:02d}:{mins:02d}:{secs_f:06.3f}"

def assign_speakers_to_segments(segments, diar_segments):
    assigned = []
    for seg in segments:
        candidates = [
            d for d in diar_segments
            if not (seg["end"] <= d["start"] or seg["start"] >= d["end"])
        ]
        if candidates:
            overlaps = [(c, min(seg["end"], c["end"]) - max(seg["start"], c["start"])) for c in candidates]
            best = max(overlaps, key=lambda x: x[1])[0]
            speaker = best["speaker"]
        else:
            speaker = "Unknown"
        seg_with_speaker = dict(seg)
        seg_with_speaker["speaker"] = speaker
        assigned.append(seg_with_speaker)
    return assigned

@app.post("/transcribe")
async def transcribe(
    audio: UploadFile = File(...),
    language: str = Form(None),
    whisper_model: str = Form("base"),
    whisper_device: str = Form("cpu"),
    whisper_compute_type: str = Form("int8"),
    diarization_model: str = Form("pyannote/speaker-diarization"),
    hf_token: str = Form(None)
):
    logger.info(f"Received new transcription request: {audio.filename}")

    try:
        # 1. Save temp audio
        with tempfile.NamedTemporaryFile(delete=False, suffix=".wav") as tmpfile:
            tmp_path = tmpfile.name
            tmpfile.write(await audio.read())
        logger.info(f"Audio saved to temp file: {tmp_path}")

        # 2. Get/load models
        whisper = get_whisper_model(whisper_model, whisper_device, whisper_compute_type)
        pipeline = get_pyannote_pipeline(diarization_model, hf_token)

        # 3. Diarization
        logger.info("Starting speaker diarization...")
        diarization = pipeline(tmp_path)
        diar_segments = []
        speakers = set()
        for turn, _, speaker in diarization.itertracks(yield_label=True):
            diar_segments.append({
                "start": float(turn.start),
                "end": float(turn.end),
                "speaker": speaker
            })
            speakers.add(speaker)
        logger.info(f"Speaker diarization done. Speakers: {speakers}")

        # 4. Transcription
        logger.info("Starting Whisper transcription...")
        segments, info = whisper.transcribe(tmp_path, language=language, beam_size=5, vad_filter=True)
        segments_list = []
        for segment in segments:
            seg_start = float(segment.start)
            seg_end = float(segment.end)
            seg_txt = segment.text.strip()
            segments_list.append({
                "start": seg_start,
                "end": seg_end,
                "text": seg_txt
            })
        logger.info(f"Transcription: {len(segments_list)} segments extracted")

        # 5. Assign speakers
        logger.info("Assigning speakers to segments...")
        assigned_segments = assign_speakers_to_segments(segments_list, diar_segments)
        formatted_segments = [
            {
                "start": format_timestamp(s["start"]),
                "end": format_timestamp(s["end"]),
                "speaker": s["speaker"],
                "text": s["text"]
            }
            for s in assigned_segments
        ]

        output = {
            "segments": formatted_segments,
            "text": " ".join([s["text"] for s in formatted_segments]).strip(),
            "language": getattr(info, "language", None),
            "language_probability": getattr(info, "language_probability", None),
            "duration": getattr(info, "duration", None),
            "speakers": list(speakers),
            "segments_count": len(formatted_segments),
            "success": True
        }
        logger.info(f"Transcription+diarization finished for {audio.filename}, language: {output['language']}, segments: {output['segments_count']}")
        os.unlink(tmp_path)
        return JSONResponse(output)
    except Exception as e:
        logger.error(f"Transcription or diarization failed: {e}", exc_info=True)
        return JSONResponse({"error": str(e), "success": False}, status_code=500)

@app.get("/")
def root():
    return {"message": "Transcription+Diarization Worker is running. See /docs for API documentation."}

