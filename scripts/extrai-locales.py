#!/usr/bin/env python3
"""Extrai as 11 localizacoes oficiais (`lng_*`) para JSON.

Cada `lng_<id>` e um ScriptableObject `GJL` com duas listas paralelas
(`txt_ids` x `txts`). O `lng_pt-br` e a traducao OFICIAL do jogo -- e a fonte
canonica dos nomes em portugues da wiki.

    ./scripts/extrai-locales.py [destino]
"""
from __future__ import annotations

import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gk import assets  # noqa: E402

OUT = sys.argv[1] if len(sys.argv) > 1 else "out/data/locales"


def main() -> None:
    os.makedirs(OUT, exist_ok=True)
    env = assets.load()
    found = 0
    for name, obj in assets.monobehaviours(env, lambda n: n.startswith("lng_")):
        data = assets.read(obj, "Assembly-CSharp-firstpass.dll", "GJL")
        lng = data["id"]
        strings = dict(zip(data["txt_ids"], data["txts"]))
        payload = {
            "id": lng,
            "count": len(strings),
            "strings": strings,
            "aliases": dict(zip(data["aliases_1"], data["aliases_2"])),
        }
        with open(os.path.join(OUT, f"{lng}.json"), "w", encoding="utf8") as fh:
            json.dump(payload, fh, ensure_ascii=False, indent=1, sort_keys=True)
        print(f"  {lng:8} {len(strings):6} textos, {len(payload['aliases'])} aliases")
        found += 1
    if not found:
        raise SystemExit("Nenhum MonoBehaviour `lng_*` encontrado")
    print(f"-> {OUT} ({found} idiomas)")


if __name__ == "__main__":
    main()
