#!/usr/bin/env python3
"""Extrai `game_data` -- o balanceamento inteiro do jogo -- para JSON.

`GameBalance` e um ScriptableObject unico (`Resources.Load<GameBalance>("game_data")`,
4,3 MB) com TODAS as listas de definicao: itens, receitas, objetos, tecnologias,
perks, peixes, almas, conquistas... Cada lista vira um arquivo em
out/data/balance/, fiel ao binario: nada e filtrado ou renomeado, para que o
`git diff` entre duas versoes do jogo mostre exatamente o que a Lazy Bear mudou.

    ./scripts/extrai-balance.py [destino]
"""
from __future__ import annotations

import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gk import assets  # noqa: E402

OUT = sys.argv[1] if len(sys.argv) > 1 else "out/data/balance"


def main() -> None:
    os.makedirs(OUT, exist_ok=True)
    env = assets.load()
    for name, obj in assets.monobehaviours(env, lambda n: n == "game_data"):
        data = assets.read(obj, "Assembly-CSharp.dll", "GameBalance")
        index = {}
        for key, value in data.items():
            if not isinstance(value, list):
                continue
            index[key] = len(value)
            with open(os.path.join(OUT, f"{key}.json"), "w", encoding="utf8") as fh:
                json.dump(value, fh, ensure_ascii=False, indent=1, sort_keys=True)
            print(f"  {key:24} {len(value):5}")
        with open(os.path.join(OUT, "_index.json"), "w", encoding="utf8") as fh:
            json.dump(index, fh, ensure_ascii=False, indent=1, sort_keys=True)
        print(f"-> {OUT} ({sum(index.values())} definicoes em {len(index)} listas)")
        return
    raise SystemExit("MonoBehaviour `game_data` nao encontrado em resources.assets")


if __name__ == "__main__":
    main()
