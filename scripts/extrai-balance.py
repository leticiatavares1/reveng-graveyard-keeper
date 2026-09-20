#!/usr/bin/env python3
"""Extrai o balanceamento inteiro do jogo para JSON, uma lista por arquivo.

O balanceamento de cada Graveyard Keeper e um ScriptableObject unico
(`Resources.Load<GameBalance>(...)`) com TODAS as listas de definicao: itens,
receitas, objetos, tecnologias, perks, quests... A saida e fiel ao binario:
nada e filtrado ou renomeado, para que o `git diff` entre duas versoes do jogo
mostre exatamente o que a Lazy Bear mudou.

    ./scripts/extrai-balance.py gk1
    ./scripts/extrai-balance.py gk2
"""
from __future__ import annotations

import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gk import assets, games  # noqa: E402


def main() -> None:
    if len(sys.argv) < 2:
        raise SystemExit(f"uso: {sys.argv[0]} <{'|'.join(games.GAMES)}> [destino]")
    game = games.get(sys.argv[1])
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(game.out, "data", "balance")

    os.makedirs(out, exist_ok=True)
    data = assets.read_balance(game)

    index = {}
    for key, value in data.items():
        if not isinstance(value, list):
            continue
        index[key] = len(value)
        with open(os.path.join(out, f"{key}.json"), "w", encoding="utf8") as fh:
            json.dump(value, fh, ensure_ascii=False, indent=1, sort_keys=True)
        if value:
            print(f"  {key:24} {len(value):5}")

    with open(os.path.join(out, "_index.json"), "w", encoding="utf8") as fh:
        json.dump(index, fh, ensure_ascii=False, indent=1, sort_keys=True)
    print(f"-> {out} ({sum(index.values())} definicoes em {len(index)} listas)")


if __name__ == "__main__":
    main()
