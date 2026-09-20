#!/usr/bin/env bash
# Cria o ambiente Python da extracao (.venv na raiz do projeto).
set -euo pipefail
cd "$(dirname "$0")/.."

if command -v uv >/dev/null 2>&1; then
  uv venv .venv
  uv pip install --python .venv/bin/python -r requirements.txt
else
  python3 -m venv .venv
  ./.venv/bin/pip install -q -r requirements.txt
fi

echo
echo "Pronto. Use:  source .venv/bin/activate"
./.venv/bin/python -c "import UnityPy; print('UnityPy', UnityPy.__version__)"
