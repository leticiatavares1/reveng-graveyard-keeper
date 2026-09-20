#!/usr/bin/env bash
# Decompila os assemblies do jogo para C# em out/src-csharp/.
# Mesma tecnica do metrics-reveng: ilspycmd, um projeto .csproj por assembly.
set -euo pipefail
cd "$(dirname "$0")/.."

GK_DATA="${GK_DATA:-$HOME/.local/share/Steam/steamapps/common/Graveyard Keeper/Graveyard Keeper_Data}"
DEST="${1:-out/src-csharp}"

# ilspycmd 8.x tem alvo .NET 6 e a maquina so tem 8/10 -- mesma pegadinha do metrics-reveng.
export DOTNET_ROLL_FORWARD=LatestMajor

command -v ilspycmd >/dev/null 2>&1 || {
  echo "ilspycmd nao encontrado. Instale com: dotnet tool install -g ilspycmd --version '8.*'" >&2
  exit 1
}

for dll in Assembly-CSharp Assembly-CSharp-firstpass; do
  echo "== $dll"
  rm -rf "${DEST:?}/$dll"
  mkdir -p "$DEST/$dll"
  ilspycmd -p -o "$DEST/$dll" "$GK_DATA/Managed/$dll.dll"
done

echo
echo "-> $DEST ($(find "$DEST" -name '*.cs' | wc -l) arquivos .cs)"
