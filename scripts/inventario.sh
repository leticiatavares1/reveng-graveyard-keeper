#!/usr/bin/env bash
# Inventario do build instalado: versao, assemblies proprios, arquivos serializados.
# Equivale ao "classificar binarios" do metrics-reveng.
#
#   ./scripts/inventario.sh gk1
#   ./scripts/inventario.sh gk2
set -uo pipefail
cd "$(dirname "$0")/.."

JOGO="${1:-gk1}"
eval "$(./.venv/bin/python - "$JOGO" <<'PY'
import sys, os
sys.path.insert(0, "scripts")
from gk import games
g = games.get(sys.argv[1])
print(f'DATA={g.env_data_dir()!r}')
print(f'NOME={g.nome!r}')
print(f'APPID={g.steam_appid}')
print('ASMS=(' + ' '.join(f'"{a}"' for a in g.assemblies) + ')')
PY
)"
[ -d "$DATA" ] || { echo "Pasta do jogo nao encontrada: $DATA" >&2; exit 1; }

echo "# Inventario — $NOME"
echo
echo "Pasta: $DATA"
echo -n "Unity: "; strings -n 5 "$DATA/globalgamemanagers" | grep -m1 -E '^[0-9]{4}\.[0-9]+\.[0-9]+[a-z][0-9]+$'
BUILDID=$(grep -m1 '"buildid"' "$HOME/.local/share/Steam/steamapps/appmanifest_$APPID.acf" 2>/dev/null | tr -d '\t"' | sed 's/buildid//')
[ -n "${BUILDID:-}" ] && echo "Steam buildid:$BUILDID (appid $APPID)"

echo
echo "## Assemblies do estudio (decompilaveis)"
for dll in "${ASMS[@]}"; do
  f="$DATA/Managed/$dll.dll"
  [ -f "$f" ] && printf '%-34s %8s KiB\n' "$dll.dll" "$(( $(stat -c%s "$f") / 1024 ))"
done

echo
echo "## Arquivos serializados (maiores)"
find "$DATA" -maxdepth 1 -type f \( -name 'resources.assets' -o -name 'globalgamemanagers*' \
  -o -name 'level*' -o -name 'sharedassets*' \) -printf '%10s  %p\n' | sort -rn | head -10 | sed "s|$DATA/||"

if [ -d "$DATA/StreamingAssets/aa" ]; then
  echo
  echo "## Addressables"
  echo "$(find "$DATA/StreamingAssets/aa" -name '*.bundle' | wc -l) bundles, $(du -sh "$DATA/StreamingAssets/aa" | cut -f1)"
  echo "(arte e cenas; o balanceamento NAO esta ai — vem de resources.assets)"
fi
