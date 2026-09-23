"""Peças compartilhadas pelos catálogos dos dois jogos."""
from __future__ import annotations

import json
import os

from .games import Game

#: Rótulo em pt-BR dos pontos de tecnologia (os ids mudam entre os jogos).
PONTOS = {
    "r": "vermelho", "g": "verde", "b": "azul", "v": "roxo",
    "gratitude_points": "gratidao",
    "tech_red": "vermelho", "tech_green": "verde", "tech_blue": "azul",
    "happiness": "felicidade",
}


def carregar(game: Game, lista: str) -> list:
    caminho = os.path.join(game.out, "data", "balance", f"{lista}.json")
    with open(caminho, encoding="utf8") as fh:
        return json.load(fh)


def carregar_locale(game: Game, lng: str) -> dict[str, str]:
    """Os textos do idioma, com os aliases já resolvidos.

    O `GJL.L()` do jogo consulta os aliases antes do dicionário, e em cadeia:
    `1h_ore_metal` -> `t_iron_ore_2` -> "Minério de ferro". Sem isso, 51 itens,
    20 bancadas e 7 tecnologias ficavam sem nome. Um alias cujo alvo não está no
    dicionário faz o jogo mostrar o próprio alvo, cru ("Advanced gravestones"):
    isso não é tradução, e aqui é ignorado.
    """
    caminho = os.path.join(game.out, "data", "locales", f"{lng}.json")
    with open(caminho, encoding="utf8") as fh:
        loc = json.load(fh)
    strings, aliases = dict(loc["strings"]), loc["aliases"]
    for chave in aliases:
        alvo, vistos = chave, set()
        while alvo in aliases and alvo not in vistos:
            vistos.add(alvo)
            alvo = aliases[alvo]
        if alvo in loc["strings"]:
            strings[chave] = loc["strings"][alvo]
    return strings


class Names:
    """Nome e descrição de um id, no idioma oficial do jogo.

    Os dois jogos usam a mesma convenção: a chave do locale é o próprio `id`, a
    descrição é `<id>_d`, e um id com sufixo (`item:2`) cai no id base quando não
    tem entrada própria.
    """

    def __init__(self, game: Game, pt: dict[str, str], en: dict[str, str]):
        self.game, self.pt, self.en = game, pt, en

    def _tabela(self, lng: str) -> dict[str, str]:
        return self.pt if lng == "pt" else self.en

    def name(self, item_id: str, lng: str) -> str | None:
        tabela = self._tabela(lng)
        for chave in (item_id, item_id.rsplit(":", 1)[0]):
            if chave in tabela:
                return tabela[chave]
        return None

    def desc(self, item_id: str, lng: str) -> str | None:
        tabela = self._tabela(lng)
        sufixo = self.game.desc_suffix
        for chave in (item_id + sufixo, item_id.rsplit(":", 1)[0] + sufixo):
            if chave in tabela:
                return tabela[chave]
        return None

    def ref(self, item_id: str) -> dict:
        return {"id": item_id, "pt": self.name(item_id, "pt"), "en": self.name(item_id, "en")}

    def label(self, item_id: str) -> str:
        return self.name(item_id, "pt") or self.name(item_id, "en") or item_id


def qtd_txt(entrada: dict) -> str:
    """'3x Tábua' — ou a expressão crua, quando a quantidade depende de perk."""
    q = entrada.get("qtd_expr")
    if q is None:
        q = f"{entrada['qtd']:g}" if entrada.get("qtd") is not None else "?"
    teto = entrada.get("qtd_max")
    if teto is not None:
        q += f"–{teto:g}" if isinstance(teto, (int, float)) else f"–{teto}"
    return f"{q}x {entrada['pt'] or entrada['en'] or entrada['id']}"


def lista_txt(entradas: list[dict]) -> str:
    return "<br>".join(qtd_txt(e) for e in entradas) or "—"


def pontos_txt(pontos: dict) -> str:
    return " ".join(f"{v} {PONTOS.get(k, k)}" for k, v in pontos.items()) or "—"


def escrever(game: Game, jsons: dict[str, list], mds: dict[str, str]) -> None:
    out_json = os.path.join(game.out, "data", "wiki")
    out_md = os.path.join(game.out, "catalogo")
    os.makedirs(out_json, exist_ok=True)
    os.makedirs(out_md, exist_ok=True)

    for nome, payload in jsons.items():
        caminho = os.path.join(out_json, f"{nome}.json")
        with open(caminho, "w", encoding="utf8") as fh:
            json.dump(payload, fh, ensure_ascii=False, indent=1)
        print(f"  {caminho:38} {len(payload)} registros")

    for nome, texto in mds.items():
        caminho = os.path.join(out_md, f"{nome}.md")
        if game.aviso:
            texto = texto.replace("\n\n", f"\n\n> **{game.aviso}**\n\n", 1)
        with open(caminho, "w", encoding="utf8") as fh:
            fh.write(texto)
        print(f"  {caminho:38} {len(texto.splitlines())} linhas")
