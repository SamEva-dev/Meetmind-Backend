import os
import sys
import json
from fastapi import FastAPI, File, UploadFile, Form
from fastapi.responses import JSONResponse
import tempfile
from datetime import datetime
from enum import Enum

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

try:
    from transformers import pipeline, AutoModelForSeq2SeqLM, AutoTokenizer
    logger.info("transformers importé avec succès")
except ImportError as e:
    logger.error(f"transformers not installed: {e}")
    pipeline = None

try:
    from langdetect import detect, DetectorFactory
    DetectorFactory.seed = 0
    logger.info("langdetect importé avec succès")
except ImportError as e:
    logger.error(f"langdetect not installed: {e}")
    detect = None

app = FastAPI(title="Transcription+Diarization+Summary Worker")

# ---- Modèles recommandés par langue (tu peux affiner ici !)
WHISPER_MODEL_FOR_LANG = {
    "fr": "large-v2",
    "en": "large-v2",
    "es": "large-v2",
    "de": "large-v2",
    "it": "large-v2",
    "pt": "large-v2",
}
WHISPER_MODEL_FALLBACK = "large-v2"  # Pour toutes les autres langues

SUMMARY_MODEL_FOR_LANG = {
    "fr": "plguillou/t5-base-fr-sum-cnndm",  # HuggingFace (FR)
    "en": "facebook/bart-large-cnn",              # HuggingFace (EN)
    "es": "mrm8488/bert2bert_shared-spanish-finetuned-summarization",  # HuggingFace (ES)
    "de": "ml6team/mt5-small-german-finetune-mlsum",                  # HuggingFace (DE)
    "it": "mrm8488/bert2bert_shared-italian-finetuned-summarization", # HuggingFace (IT)
    "pt": "cmarkea/bart-base-portuguese-summarizer",                  # HuggingFace (PT)
}
SUMMARY_MODEL_FALLBACK = "facebook/bart-large-cnn"  # Fallback toutes langues

# ---- Caches pour les modèles chargés à la volée
whisper_models_cache = {}
pyannote_pipelines_cache = {}
summarizer_cache = {}

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

def get_summarizer(model_name):
    if model_name not in summarizer_cache:
        logger.info(f"Loading summarization model: {model_name}")
        tokenizer = AutoTokenizer.from_pretrained(model_name)
        model = AutoModelForSeq2SeqLM.from_pretrained(model_name)
        summarizer_cache[model_name] = pipeline("summarization", model=model, tokenizer=tokenizer)
    return summarizer_cache[model_name]

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
    whisper_model: str = Form(None),
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

        # 2. Détection automatique de langue si non spécifiée/auto
        lang = language
        if not lang or lang.lower() in ("", "auto", "none"):
            quick_model = get_whisper_model(WHISPER_MODEL_FALLBACK, whisper_device, whisper_compute_type)
            logger.info("Langue non spécifiée, transcription rapide pour détection de langue.")
            segments, info = quick_model.transcribe(tmp_path, beam_size=1, vad_filter=True)
            text_concat = " ".join([s.text.strip() for s in segments])
            if detect and text_concat:
                try:
                    lang = detect(text_concat)
                    logger.info(f"Langue détectée automatiquement : {lang}")
                except Exception as e:
                    lang = "fr"
                    logger.warning(f"Langue non détectée. Défaut: 'fr'. Cause: {e}")
            else:
                lang = "fr"
                logger.warning("Langue non détectée (langdetect manquant ou texte vide), défaut: 'fr'")

        # 3. Modèle Whisper optimal selon langue (sauf si explicitement demandé)
        w_model = whisper_model
        if not w_model or w_model.lower() in ("auto", "default", ""):
            w_model = WHISPER_MODEL_FOR_LANG.get(lang, WHISPER_MODEL_FALLBACK)
            logger.info(f"Modèle Whisper choisi dynamiquement : {w_model} (langue : {lang})")
        else:
            logger.info(f"Modèle Whisper explicitement demandé : {w_model}")
        whisper = get_whisper_model(w_model, whisper_device, whisper_compute_type)
        pipeline = get_pyannote_pipeline(diarization_model, hf_token)

        # 4. Diarization
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

        # 5. Transcription finale (avec langue fixée)
        logger.info("Starting Whisper transcription (final)...")
        segments, info = whisper.transcribe(tmp_path, language=lang, beam_size=5, vad_filter=True)
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

        # 6. Assign speakers
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
            "language": lang,
            "language_probability": getattr(info, "language_probability", None),
            "duration": getattr(info, "duration", None),
            "speakers": list(speakers),
            "segments_count": len(formatted_segments),
            "success": True
        }
        logger.info(f"Transcription+diarization finished for {audio.filename}, language: {lang}, segments: {output['segments_count']}")
        os.unlink(tmp_path)
        return JSONResponse(output)
    except Exception as e:
        logger.error(f"Transcription or diarization failed: {e}", exc_info=True)
        return JSONResponse({"error": str(e), "success": False}, status_code=500)

from pydantic import BaseModel

class DetailLevel(str, Enum):
    short = "short"
    standard = "standard"
    Detailed = "detailed"

class SummarizeRequest(BaseModel):
    text: str
    language: str = None
    summary_model: str = None
    detail_level: DetailLevel = DetailLevel.standard

class SummarizeResponse(BaseModel):
    summary: str

@app.post("/summarize", response_model=SummarizeResponse)
async def summarize(req: SummarizeRequest):
    text = req.text
    max_input_length = 2048
    if len(text) > max_input_length:
        logger.warning(f"Texte résumé tronqué à {max_input_length} caractères.")
        text = text[:max_input_length]

    # Paramétrage longueur du résumé selon le niveau de détail demandé
    if req.detail_level == "short":
        max_length, min_length = 80, 20
    elif req.detail_level == "detailed":
        max_length, min_length = 500, 80
    else:  # "standard" par défaut
        max_length, min_length = 250, 40

    # Détection automatique de la langue si non spécifiée/auto
    lang = req.language
    if not lang or lang.lower() in ("", "auto", "none"):
        if detect:
            try:
                lang = detect(text)
                logger.info(f"Langue détectée automatiquement: {lang}")
            except Exception as e:
                logger.warning(f"Impossible de détecter la langue: {e}. Défaut à 'fr'.")
                lang = "fr"
        else:
            lang = "fr"
            logger.warning("langdetect non installé, langue par défaut: fr")

    # Modèle de résumé optimal selon langue (sauf si explicitement demandé)
    model_name = req.summary_model
    if not model_name or model_name.lower() in ("auto", "default", ""):
        model_name = SUMMARY_MODEL_FOR_LANG.get(lang, SUMMARY_MODEL_FALLBACK)
        logger.info(f"Modèle résumé choisi dynamiquement : {model_name} (langue : {lang})")
    else:
        logger.info(f"Modèle résumé explicitement demandé : {model_name}")

    try:
        summarizer = get_summarizer(model_name)
    except Exception as e:
        logger.error(f"Erreur chargement modèle résumé: {model_name} | {e}")
        return JSONResponse({"summary": f"Erreur : impossible de charger le modèle {model_name} : {e}"}, status_code=500)

    try:
        logger.info(f"Résumé demandé ({len(text)} chars, lang={lang}, modèle={model_name})")
        result = summarizer(text, max_length=250, min_length=40, do_sample=False)
        summary = result[0]['summary_text']
        logger.info(f"Résumé généré (longueur {len(summary)} caractères)")
        return SummarizeResponse(summary=summary)
    except Exception as e:
        logger.error(f"Erreur lors du résumé: {e}")
        return JSONResponse({"summary": f"Erreur: {e}"}, status_code=500)


@app.get("/health")
def health_check():
    return {
        "status": "ok",
        "timestamp": datetime.now().isoformat() + "Z",
        "version": "1.0.0"
    }
