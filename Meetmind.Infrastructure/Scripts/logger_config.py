import os
from pathlib import Path
import logging
from logging.handlers import TimedRotatingFileHandler

# 1. Récupérer la variable d'environnement MEETMIND_HOME
home_dir = os.environ.get("MEETMIND_HOME")
if not home_dir:
    raise EnvironmentError("La variable d environnement MEETMIND_HOME n est pas definie.")

# 2. Définir le dossier et le fichier de log
logs_dir = Path(home_dir) / "log" / "MeetMind"
logs_dir.mkdir(parents=True, exist_ok=True)  # Crée le dossier si besoin

log_file = logs_dir / "MeetMindPython_log.txt"

# 3. Configuration du logger
logger = logging.getLogger("MeetMind")
logger.setLevel(logging.INFO)

handler = TimedRotatingFileHandler(
    filename=log_file,
    when="midnight",
    interval=1,
    backupCount=7,
    encoding="utf-8"
)
formatter = logging.Formatter("%(asctime)s - %(levelname)s - %(message)s")
handler.setFormatter(formatter)
logger.addHandler(handler)

# Affiche aussi dans la console
console = logging.StreamHandler()
console.setFormatter(formatter)
logger.addHandler(console)
