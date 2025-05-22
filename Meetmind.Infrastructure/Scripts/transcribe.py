import sys
import json

# Forcer la sortie UTF-8 sur Windows
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

try:
    import logger_config
    logger = logger_config.logger
except Exception as e:
    print(json.dumps({"error": f"Logger config failed: {e}"}))
    
    sys.exit(1)

try:
    from faster_whisper import WhisperModel
except ImportError as e:
    logger.error(json.dumps({"error": f"faster-whisper not installed: {e}"}))
    sys.exit(1)

try:
    from pyannote.audio import Pipeline
except ImportError as e:
    logger = None
    logger.error(json.dumps({"error": f"pyannote.audio not installed: {e}"}))
    sys.exit(1)

def parse_args():
    import argparse
    parser = argparse.ArgumentParser(description="Transcribe AND diarize an audio file (faster-whisper + pyannote)")
    parser.add_argument("audio", help="Audio file to process")
    parser.add_argument("--language", help="Force language code (ex: fr, en, es, ...)", default=None)
    parser.add_argument("--model", help="Whisper model name or path (default: base)", default=None)
    parser.add_argument("--device", help="Device to use: cpu, cuda, auto (default: auto)", default="auto")
    parser.add_argument("--compute_type", help="Compute type: default, int8, int8_float16, int16, float16, float32 (default: default)", default="default")
    parser.add_argument("--diarization_model", help="HuggingFace pipeline/model for pyannote (default: pyannote/speaker-diarization)", default="pyannote/speaker-diarization")
    parser.add_argument("--hf_token", help="HuggingFace access token for pyannote", default=None)
    return parser.parse_args()

def format_timestamp(secs):
    hrs = int(secs // 3600)
    mins = int((secs % 3600) // 60)
    secs_f = secs % 60
    return f"{hrs:02d}:{mins:02d}:{secs_f:06.3f}"

def diarize(audio_path, model_name, hf_token):
    # Chargement du pipeline pyannote
    logger.info(f"diarize audio_path: {audio_path}, model_name: {model_name}, hf_token :{hf_token}")
    if hf_token:
        pipeline = Pipeline.from_pretrained(model_name, use_auth_token=hf_token)
    else:
        pipeline = Pipeline.from_pretrained(model_name)
    diarization = pipeline(audio_path)
    diar_segments = []
    speakers = set()
    for turn, _, speaker in diarization.itertracks(yield_label=True):
        diar_segments.append({
            "start": float(turn.start),
            "end": float(turn.end),
            "speaker": speaker
        })
        speakers.add(speaker)
    return diar_segments, list(speakers)

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

def main():
    args = parse_args()
    logger.info(f"Argument parse : {args}")
    audio_path = args.audio
    logger.info(f"Audio path : {audio_path}")

    try:
        # --- 1. Diarization ---
        logger.info("Starting speaker diarization...")
        diar_segments, speakers = diarize(audio_path, args.diarization_model, args.hf_token)
        logger.info(f"Speaker diarization found speakers: {speakers}")

        # --- 2. Transcription ---
        model_name_or_path = args.model or "base"
        logger.info(f"Loading faster-whisper model: {model_name_or_path}, device: {args.device}, compute_type: {args.compute_type}")
        model = WhisperModel(model_name_or_path, device=args.device, compute_type=args.compute_type)
        logger.info(f"Starting transcription of {audio_path}... (lang: {args.language})")
        segments, info = model.transcribe(audio_path, language=args.language, beam_size=5, vad_filter=True)

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

        # --- 3. Assign speakers ---
        logger.info("Assigning speakers to segments...")
        assigned_segments = assign_speakers_to_segments(segments_list, diar_segments)

        # --- 4. Format output (with string timestamps) ---
        formatted_segments = [
            {
                "start": format_timestamp(s["start"]),
                "end": format_timestamp(s["end"]),
                "speaker": s["speaker"],
                "text": s["text"]
            }
            for s in assigned_segments
        ]

        # --- 5. Output JSON ---
        output = {
            "segments": formatted_segments,
            "text": " ".join([s["text"] for s in formatted_segments]).strip(),
            "language": getattr(info, "language", None),
            "language_probability": getattr(info, "language_probability", None),
            "duration": getattr(info, "duration", None),
            "speakers": speakers,
            "segments_count": len(formatted_segments),
            "success": True
        }

        logger.info(f"Transcription+diarization successful for {audio_path}, language: {output['language']}, segments: {output['segments_count']}")
        print(json.dumps(output, ensure_ascii=False, indent=2))

    except Exception as e:
        if logger:
            logger.error(f"Transcription or diarization failed: {e}", exc_info=True)
        print(json.dumps({"error": str(e), "success": False}))
        sys.exit(1)

if __name__ == "__main__":
    main()
