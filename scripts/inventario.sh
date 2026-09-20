#!/usr/bin/env bash
# Inventario do build instalado: versao, assemblies proprios x terceiros,
# arquivos serializados. Equivale ao "classificar binarios" do metrics-reveng.
set -uo pipefail
cd "$(dirname "$0")/.."

GK_DATA="${GK_DATA:-$HOME/.local/share/Steam/steamapps/common/Graveyard Keeper/Graveyard Keeper_Data}"
[ -d "$GK_DATA" ] || { echo "Pasta do jogo nao encontrada: $GK_DATA" >&2; exit 1; }

echo "# Inventario do build"
echo
echo "Pasta: $GK_DATA"
echo -n "Jogo:  "; tr '\n' ' ' < "$GK_DATA/app.info"; echo
echo -n "Unity: "; strings -n 5 "$GK_DATA/globalgamemanagers" | grep -m1 -E '^[0-9]{4}\.[0-9]+\.[0-9]+[a-z][0-9]+$'
BUILDID=$(grep -m1 '"buildid"' "$(dirname "$(dirname "$GK_DATA")")/../appmanifest_599140.acf" 2>/dev/null | tr -d '\t"' | sed 's/buildid//')
[ -n "${BUILDID:-}" ] && echo "Steam buildid:$BUILDID"

echo
echo "## Assemblies do jogo (decompilaveis)"
for dll in Assembly-CSharp Assembly-CSharp-firstpass; do
  f="$GK_DATA/Managed/$dll.dll"
  [ -f "$f" ] && printf '%-32s %8s KiB  %s\n' "$dll.dll" "$(( $(stat -c%s "$f") / 1024 ))" "$(file -b "$f" | cut -c1-40)"
done

echo
echo "## Arquivos serializados (maiores)"
find "$GK_DATA" -maxdepth 1 -type f \( -name 'resources.assets' -o -name 'globalgamemanagers*' -o -name 'level*' \) \
  -printf '%10s  %p\n' | sort -rn | head -12 | sed "s|$GK_DATA/||"
