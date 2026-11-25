"""Stage tracked application source files for coverage rendering in Jenkins."""

import shutil
import subprocess
import os
from pathlib import Path

root = Path.cwd()
destination = Path(os.environ.get("COVERAGE_SOURCES_DIR", root / "reports" / "source-files"))
paths = subprocess.check_output(["git", "ls-files", "-z"]).decode("utf-8").split("\0")

for item in paths:
    if not item:
        continue
    path = Path(item)
    backend = path.parts[0] == "CatalysticBackend" and path.suffix == ".cs"
    frontend = path.parts[:2] == ("catalysticfrontend", "src") and path.suffix in {".js", ".jsx", ".ts", ".tsx"}
    if backend or frontend:
        target = destination / path
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(root / path, target)

