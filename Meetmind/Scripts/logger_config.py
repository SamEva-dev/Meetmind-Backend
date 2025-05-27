# -*- coding: utf-8 -*-

import os
import sys
from pathlib import Path
import logging
from logging.handlers import TimedRotatingFileHandler

# 1. Recuperer la variable d environnement MEETMIND_HOME
home_dir = os.environ.get("MEETMIND_HOME")
if not home_dir:
    raise EnvironmentError("La variable d environnement MEETMIND_HOME n est pas definie.")

# 2. Definir le dossier et le fichier de log
logs_dir = Path(home_dir) / "log" / "MeetMind"
logs_dir.mkdir(parents=True, exist_ok=True)  # Crée le dossier si besoin

log_file = logs_dir / "MeetMindPython_log.txt"

# 3. Configuration du logger
logger = logging.getLogger("MeetMind")
logger.setLevel(logging.INFO)
logger.propagate = False  # <--- Évite la duplication si root logger configuré ailleurs

# File handler (rotation quotidienne)
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

# Console handler (stdout, pour remontee dans les logs .NET)
console = logging.StreamHandler(sys.stdout)
console.setFormatter(formatter)
logger.addHandler(console)

# (Optionnel) Forcer le flush immediat sur console (utile pour ProcessStartInfo)
for h in logger.handlers:
    if isinstance(h, logging.StreamHandler):
        h.flush = sys.stdout.flush

# Exemple de test
logger.info("Logger MeetMind demarre et pret a remonter dans la console .NET.")
