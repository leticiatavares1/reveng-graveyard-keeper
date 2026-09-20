#!/usr/bin/env python3
"""Cruza balanceamento + localização e gera os catálogos navegáveis.

Lê `out/<jogo>/data/balance` e `.../locales` (saída dos dois `extrai-*.py`) e escreve:

  out/<jogo>/data/wiki/{itens,receitas,tecnologias}.json   matéria-prima da wiki
  out/<jogo>/catalogo/{itens,receitas,tecnologias}.md      para ler

Cada jogo tem o seu adaptador em `gk/catalogo_<jogo>.py`, porque o esquema do
balanceamento mudou bastante entre os dois. Ver docs/04-ponte-para-a-wiki.md.

    ./scripts/catalogo.py gk1
    ./scripts/catalogo.py gk2
"""
from __future__ import annotations

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gk import catalogo_gk1, catalogo_gk2, games  # noqa: E402
from gk.catalogo_comum import Names, carregar_locale  # noqa: E402

ADAPTADORES = {"gk1": catalogo_gk1, "gk2": catalogo_gk2}


def main() -> None:
    if len(sys.argv) < 2:
        raise SystemExit(f"uso: {sys.argv[0]} <{'|'.join(games.GAMES)}>")
    game = games.get(sys.argv[1])

    if not os.path.isdir(os.path.join(game.out, "data", "balance")):
        raise SystemExit(f"Rode antes: ./scripts/extrai-balance.py {game.id} "
                         f"e ./scripts/extrai-locales.py {game.id}")

    names = Names(game, carregar_locale(game, "pt-br"), carregar_locale(game, "en"))
    ADAPTADORES[game.id].run(game, names)


if __name__ == "__main__":
    main()
