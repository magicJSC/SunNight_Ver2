"""Read-only telemetry sampler while Unity's script execution queue is occupied."""
import json
import time
from pathlib import Path

root = Path(__file__).resolve().parents[1]
source = root / "Logs/FirstDayV03/search-night-resume-final-progress.json"
result = root / "Logs/FirstDayV03/search-night-resume-final.json"
samples = []
deadline = time.monotonic() + 420
while time.monotonic() < deadline:
    try:
        row = json.loads(source.read_text(encoding="utf-8-sig"))
        if not samples or row["seconds"] != samples[-1]["seconds"]:
            samples.append(row)
            (root / "Logs/FirstNightV03/clock-and-combat-samples.json").write_text(
                json.dumps(samples, ensure_ascii=False, indent=2), encoding="utf-8")
    except (OSError, ValueError):
        pass
    if result.exists():
        break
    time.sleep(1)
print(f"Recorded {len(samples)} gameplay snapshots")
