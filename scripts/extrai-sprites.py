#!/usr/bin/env python3
"""Exporta os icones de item, bancada, objeto de construcao e ramo de
tecnologia (e as estrelas de qualidade) como PNG.

Le `out/<jogo>/data/wiki/{itens,receitas,tecnologias}.json` -- ou seja, roda
DEPOIS do `catalogo.py`, que e quem sabe derivar o nome de cada sprite (item
por `ItemDefinition.GetIcon()`; bancada e objeto de construcao pelo
`custom_icon`/`icon` do proprio balanceamento; ramo de tecnologia por
`i_tbranch_<n>`, fixo). Aqui so se abre o `resources.assets` e se escreve o
que o catalogo pediu:

  out/<jogo>/icones/<nome>.png

A estrela de qualidade e sprite separado (`item_star_1..3`), desenhado por cima
do icone na celula do inventario: um item com `"estrela": 2` aparece no jogo com
o mesmo `i_*` dos outros niveis mais a estrela de prata.

Os PNG ficam fora do git (ver `.gitignore`) -- sao asset do jogo, nao dado
extraido. Regenere quando o build do jogo mudar.

    ./scripts/extrai-sprites.py gk1
"""
from __future__ import annotations

import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gk import assets, games  # noqa: E402

#: O glifo de cada dia da semana no HUD do jogo (HUDSinIcon.spr_back, um por
#: pecado), usado pelo artigo escrito a mao "Dias da semana" da keeper-wiki-fnd.
#: Nao vem de nenhum JSON do catalogo -- e balanceamento de UI (NGUI), nao
#: item/receita/tecnologia -- por isso o nome fica fixo aqui, achado uma vez
#: so por leitura direta do HUD na cena do jogo (level2 + sharedassets2.assets,
#: nao resources.assets). So a variante "_off" (o icone base, sempre visivel;
#: "_on" e o brilho de um estado de jogo que nao interessa aqui).
HUD_SEMANA = {f"i_hud_sin0{n}_off" for n in range(1, 7)}
HUD_SEMANA_ARQUIVO = "sharedassets2.assets"

#: As caveiras branca (boas acoes) e vermelha (pecados) do painel do corpo, no
#: necroterio e na mesa de embalsamar (BodyPanelSkullBarGUI.skull_white /
#: skull_red), usadas pelas tabelas de efeito do artigo "Corpos e autopsia".
#: Mesma situacao dos dias da semana: e sprite de UI, nao de item, entao o
#: nome fica fixo aqui. Estao no resources.assets, lado a lado (11x10 px).
HUD_CAVEIRAS = {"icon_hud_skull", "icon_skull_red"}


def pedidos(wiki_dir: str) -> tuple[set[str], set[str], set[str]]:
    """(icones, estrelas, icones em uso) que o catalogo referencia.

    Bancada, objeto de construcao e ramo de tecnologia nao tem `nao_usado`
    -- ao contrario de item, tudo que aparece em receita ou arvore de
    pesquisa esta, por definicao, em uso.
    """
    with open(os.path.join(wiki_dir, "itens.json"), encoding="utf8") as fh:
        itens = json.load(fh)
    with open(os.path.join(wiki_dir, "receitas.json"), encoding="utf8") as fh:
        receitas = json.load(fh)
    with open(os.path.join(wiki_dir, "tecnologias.json"), encoding="utf8") as fh:
        tecnologias = json.load(fh)

    icones = {i["icone"] for i in itens if i.get("icone")}
    estrelas = {f"item_star_{i['estrela']}" for i in itens if i.get("estrela")}
    em_uso = {i["icone"] for i in itens if i.get("icone") and not i["nao_usado"]}

    for r in receitas:
        for estacao in r.get("estacoes") or []:
            if estacao.get("icone"):
                icones.add(estacao["icone"])
                em_uso.add(estacao["icone"])
        objeto = r.get("objeto_construido")
        if objeto and objeto.get("icone"):
            icones.add(objeto["icone"])
            em_uso.add(objeto["icone"])
    for t in tecnologias:
        ramo_icone = (t.get("ramo") or {}).get("icone")
        if ramo_icone:
            icones.add(ramo_icone)
            em_uso.add(ramo_icone)

    return icones, estrelas, em_uso


def main() -> None:
    if len(sys.argv) < 2:
        raise SystemExit(f"uso: {sys.argv[0]} <{'|'.join(games.GAMES)}> [destino]")
    game = games.get(sys.argv[1])
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(game.out, "icones")

    wiki_dir = os.path.join(game.out, "data", "wiki")
    if not os.path.isfile(os.path.join(wiki_dir, "itens.json")):
        raise SystemExit(f"Rode antes: ./scripts/catalogo.py {game.id}")

    icones, estrelas, em_uso = pedidos(wiki_dir)
    if not icones:
        raise SystemExit(f"{wiki_dir} nao tem nenhum campo `icone` -- rode o catalogo de novo")

    os.makedirs(out, exist_ok=True)
    salvos = set()
    for nome, imagem in assets.read_sprites(game, icones | estrelas):
        imagem.save(os.path.join(out, f"{nome}.png"))
        salvos.add(nome)

    faltando = sorted((icones | estrelas) - salvos)
    print(f"  icones   {len(salvos & icones):5}/{len(icones)}")
    print(f"  estrelas {len(salvos & estrelas):5}/{len(estrelas)}")
    if faltando:
        # Sprite que falta e quase sempre de item fora de uso, que saiu da arte
        # e ficou no balanceamento. O punhado que sobra falta no jogo tambem:
        # `GetIcon` monta `i_<id>` e `i_hop_honey:1` nao existe em colecao
        # nenhuma. Nao se inventa substituto -- quem consome mostra o vazio.
        vivos = sorted(x for x in faltando if x in em_uso)
        print(f"\n{len(faltando)} sem sprite em {assets.RESOURCES}, "
              f"{len(vivos)} de item em uso" + (f": {', '.join(vivos)}" if vivos else ""))

    if game.id == "gk1":
        salvos_hud = set()
        for nome, imagem in assets.read_sprites(game, HUD_SEMANA, filename=HUD_SEMANA_ARQUIVO):
            imagem.save(os.path.join(out, f"{nome}.png"))
            salvos_hud.add(nome)
        print(f"  hud      {len(salvos_hud):5}/{len(HUD_SEMANA)} (dias da semana, {HUD_SEMANA_ARQUIVO})")
        salvos |= salvos_hud

        salvos_caveiras = set()
        for nome, imagem in assets.read_sprites(game, HUD_CAVEIRAS):
            imagem.save(os.path.join(out, f"{nome}.png"))
            salvos_caveiras.add(nome)
        print(f"  caveiras {len(salvos_caveiras):5}/{len(HUD_CAVEIRAS)} (painel do corpo, {assets.RESOURCES})")
        salvos |= salvos_caveiras

    print(f"-> {out} ({len(salvos)} PNG)")


if __name__ == "__main__":
    main()
