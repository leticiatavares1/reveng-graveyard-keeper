#!/usr/bin/env bash
# Decompila os assemblies do estudio para C# em out/<jogo>/src-csharp/.
# Mesma tecnica do metrics-reveng: ilspycmd, um projeto .csproj por assembly.
#
#   ./scripts/decompila.sh gk1
#   ./scripts/decompila.sh gk2
set -euo pipefail
cd "$(dirname "$0")/.."

JOGO="${1:-gk1}"
eval "$(./.venv/bin/python - "$JOGO" <<'PY'
import sys
sys.path.insert(0, "scripts")
from gk import games
g = games.get(sys.argv[1])
print(f'DATA={g.env_data_dir()!r}')
print(f'DEST={g.out!r}/src-csharp')
print('ASMS=(' + ' '.join(f'"{a}"' for a in g.assemblies) + ')')
PY
)"
DEST="${2:-$DEST}"

# ilspycmd 8.x tem alvo .NET 6 e a maquina so tem 8/10 -- mesma pegadinha do metrics-reveng.
export DOTNET_ROLL_FORWARD=LatestMajor

command -v ilspycmd >/dev/null 2>&1 || {
  echo "ilspycmd nao encontrado. Instale com: dotnet tool install -g ilspycmd --version '8.*'" >&2
  exit 1
}

for dll in "${ASMS[@]}"; do
  echo "== $dll"
  rm -rf "${DEST:?}/$dll"
  mkdir -p "$DEST/$dll"
  ilspycmd -p -o "$DEST/$dll" "$DATA/Managed/$dll.dll"
done

echo
echo "-> $DEST ($(find "$DEST" -name '*.cs' | wc -l) arquivos .cs)"
