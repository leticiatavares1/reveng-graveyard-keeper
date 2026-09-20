"""Catálogo do Graveyard Keeper 2 (demo).

O GK2 é o mesmo modelo do GK1 redesenhado: `GameBalance` continua sendo um
ScriptableObject único, mas as classes viraram `*Def`, os campos passaram para
camelCase e o `SmartExpression` virou `LazyExpression`. As saídas de receita
agora ficam dentro de um objeto (`outputItems.chanceOutputItems`) em vez de uma
lista solta.
"""
from __future__ import annotations

from collections import defaultdict

from .catalogo_comum import (PONTOS, Names, carregar, escrever, lista_txt,
                             pontos_txt)
from .games import Game

#: ItemType (Assembly-CSharp.dll)
ITEM_TYPES = {
    0: "None", 1: "Axe", 2: "Shovel", 3: "Pickaxe", 4: "Hammer", 5: "FishingRod",
    10: "Hand", 11: "Sword", 12: "BodyArmor", 13: "Brain", 14: "Heart", 15: "Flesh",
    16: "Bones", 18: "Bait", 20: "Preach", 22: "Bow", 23: "SurgicalKit",
    24: "Reagents", 25: "SmallTools", 26: "Book", 27: "Talisman", 28: "Spit",
    30: "Embalm", 35: "Skull", 36: "Guts", 37: "Skin", 45: "Collar", 50: "Pike",
    400: "Bag", 666: "Demon",
}

#: TechTreeTab — a chave do nome no locale é `tech_tab_<Nome>`.
TECH_TABS = {0: "Building", 1: "Metallurgy", 2: "Farming", 3: "Theology",
             4: "Anatomy", 5: "Cooking"}

#: Os três pontos de tecnologia, como campos próprios do CraftDef.
PONTOS_CRAFT = {"techRed": "tech_red", "techGreen": "tech_green", "techBlue": "tech_blue"}

#: PureValueType
PURE_NONE, PURE_FLOAT, PURE_BOOL, PURE_STRING = 0, 1, 2, 3


def expr(valor):
    """LazyExpression -> número quando é valor puro, senão a expressão crua."""
    if not isinstance(valor, dict):
        return valor
    tipo = valor.get("pureValueType", PURE_NONE)
    if tipo == PURE_FLOAT:
        return round(valor["pureValueFloat"], 4)
    if tipo == PURE_BOOL:
        return bool(valor["pureValueBool"])
    return valor.get("expressionString") or None


def item_ref(names: Names, item: dict) -> dict:
    """Entrada/saída de receita, com a quantidade e a faixa quando houver."""
    out = names.ref(item["id"])
    qtd = expr(item.get("count"))
    lo, hi = expr(item.get("minValue")), expr(item.get("maxValue"))
    if isinstance(qtd, str):
        out["qtd"] = None
        out["qtd_expr"] = qtd
    else:
        out["qtd"] = qtd
    if lo is not None and hi is not None:
        out["qtd"], out["qtd_max"] = lo, hi
    chance = expr(item.get("chance"))
    if chance is not None:
        out["chance"] = chance
    if item.get("outputGroupId"):
        out["grupo"] = item["outputGroupId"]
    return out


def saidas_de(names: Names, craft: dict) -> list[dict]:
    """`outputItems` é um objeto com duas listas, não uma lista."""
    saida = craft.get("outputItems") or {}
    refs = [item_ref(names, i) for i in saida.get("chanceOutputItems", [])]
    # Saida em grupo: uma lista de alternativas, cada uma com a sua chance.
    for grupo in saida.get("groupChanceOutputItems", []):
        for i in grupo.get("chanceItems", []):
            ref = item_ref(names, i)
            ref["alternativa"] = True
            refs.append(ref)
    return refs


def itens(game: Game, names: Names) -> list[dict]:
    out = []
    for it in carregar(game, "itemDefs"):
        out.append({
            "id": it["id"],
            "pt": names.name(it["id"], "pt"),
            "en": names.name(it["id"], "en"),
            "descricao_pt": names.desc(it["id"], "pt"),
            "descricao_en": names.desc(it["id"], "en"),
            "tipo": ITEM_TYPES.get(it["type"], str(it["type"])),
            "preco_base": it["basePrice"],
            "qualidade": it["quality"],
            "pilha": it["stackCount"],
            "tem_durabilidade": bool(it["hasDurability"]),
            "e_ferramenta": bool(it["isTool"]),
            "e_arma": bool(it["isWeapon"]),
            "e_semente": bool(it["isSeed"]),
            "e_combustivel": bool(it["isFuel"]),
            "e_produto": bool(it["isProduct"]),
            "e_ponto_de_tecnologia": bool(it["isTechPoint"]),
            "grupos": it["itemGroupIds"],
        })
    return sorted(out, key=lambda i: i["id"])


def receitas(game: Game, names: Names) -> list[dict]:
    liberada_por: dict[str, list[str]] = defaultdict(list)
    for tech in carregar(game, "techDefs"):
        for craft_id in tech["craftsAfterUnlock"]:
            liberada_por[craft_id].append(tech["id"])

    out = []
    for c in carregar(game, "craftDefs"):
        pontos = {}
        for campo, chave in PONTOS_CRAFT.items():
            v = expr(c.get(campo))
            if v:
                pontos[chave] = v
        out.append({
            "id": c["id"],
            "origem": "craft",
            "estacoes": [names.ref(o) for o in c["craftsIn"]],
            "entradas": [item_ref(names, i) for i in c["needItems"]],
            "entradas_da_estacao": [item_ref(names, i) for i in c["needItemsFromWgo"]],
            "saidas": saidas_de(names, c),
            "pontos_tecnologia": pontos,
            "tempo_s": expr(c["duration"]),
            "energia_por_tick": expr(c["energyPerTick"]),
            "insanidade_por_tick": expr(c["insanityPerTick"]),
            "oculta": bool(c["isHidden"]),
            "precisa_desbloquear": bool(c["isNeedsUnlock"]),
            "e_automatica": bool(c["isAuto"]),
            "e_de_estrela": bool(c["isStarCraft"]),
            "combustivel": c["fuelItemDefId"] or None,
            "perks": c["linkedPerks"],
            "aba": c["tabId"] or None,
            "liberada_por": liberada_por.get(c["id"], []),
        })
    return sorted(out, key=lambda r: r["id"])


def tecnologias(game: Game, names: Names) -> list[dict]:
    out = []
    for t in carregar(game, "techDefs"):
        aba = TECH_TABS.get(t["tab"], str(t["tab"]))
        custo = {k: v for k, v in (("tech_red", t["redSpheresPrice"]),
                                   ("tech_green", t["greenSpheresPrice"]),
                                   ("tech_blue", t["blueSpheresPrice"])) if v}
        out.append({
            "id": t["id"],
            "pt": names.name(t["id"], "pt"),
            "en": names.name(t["id"], "en"),
            "aba": {"id": aba, "pt": names.name(f"tech_tab_{aba}", "pt")},
            "custo": custo,
            "requer": t["parents"],
            "libera_receitas": t["craftsAfterUnlock"],
            "libera_construcoes": t["buildingsAfterUnlock"],
            "libera_perks": t["perksAfterUnlock"],
            "libera_formulas": t["alchemyFormulasAfterUnlock"],
            "disponivel_na_demo": bool(t["isAvailableInDemo"]),
            "oculta_no_inicio": bool(t["hiddenAtStart"]),
        })
    return sorted(out, key=lambda x: (x["aba"]["id"], x["id"]))


def md_itens(itens: list[dict]) -> str:
    linhas = ["# Itens", "",
              f"{len(itens)} itens em `itemDefs`. Nome pt-BR e o da traducao oficial do jogo.",
              "", "| id | pt-BR | en | tipo | preco | qualidade | pilha |",
              "| -- | ----- | -- | ---- | ----: | --------: | ----: |"]
    for i in itens:
        linhas.append("| `{id}` | {pt} | {en} | {tipo} | {preco_base:g} | {qualidade:g} | {pilha} |".format(
            **{**i, "pt": i["pt"] or "—", "en": i["en"] or "—"}))
    return "\n".join(linhas) + "\n"


def md_receitas(receitas: list[dict], names: Names) -> str:
    por_estacao: dict[str, list[dict]] = defaultdict(list)
    for r in receitas:
        for st in r["estacoes"] or [{"id": "(sem estacao)"}]:
            por_estacao[st["id"]].append(r)

    linhas = ["# Receitas por estacao", "",
              f"{len(receitas)} receitas (`craftDefs`), {len(por_estacao)} estacoes.", ""]
    for estacao in sorted(por_estacao):
        linhas += [f"## {names.label(estacao)} — `{estacao}`", "",
                   "| receita | precisa | produz | tempo | energia/tick | pontos |",
                   "| ------- | ------- | ------ | ----: | -----------: | ------ |"]
        for r in sorted(por_estacao[estacao], key=lambda x: x["id"]):
            linhas.append("| `{id}` | {n} | {o} | {t} | {e} | {p} |".format(
                id=r["id"], n=lista_txt(r["entradas"]), o=lista_txt(r["saidas"]),
                t=r["tempo_s"] if r["tempo_s"] is not None else "—",
                e=r["energia_por_tick"] if r["energia_por_tick"] is not None else "—",
                p=pontos_txt(r["pontos_tecnologia"])))
        linhas.append("")
    return "\n".join(linhas) + "\n"


def md_tecnologias(techs: list[dict], names: Names) -> str:
    linhas = ["# Tecnologias", "",
              "Custo em esferas e o que cada no libera (`techDefs`). A coluna demo diz se "
              "o no esta disponivel nesta build.",
              "", "| tecnologia | aba | custo | demo | libera |",
              "| ---------- | --- | ----- | :--: | ------ |"]
    for t in techs:
        custo = " ".join(f"{v:g} {PONTOS.get(k, k)}" for k, v in t["custo"].items())
        libera = t["libera_receitas"] + t["libera_construcoes"]
        txt = ", ".join(f"`{c}`" for c in libera[:6]) or "—"
        if len(libera) > 6:
            txt += f" (+{len(libera) - 6})"
        linhas.append(f"| {t['pt'] or t['id']} (`{t['id']}`) | {t['aba']['pt'] or t['aba']['id']} "
                      f"| {custo or '—'} | {'sim' if t['disponivel_na_demo'] else 'nao'} | {txt} |")
    return "\n".join(linhas) + "\n"


def run(game: Game, names: Names) -> None:
    i, r, t = itens(game, names), receitas(game, names), tecnologias(game, names)
    escrever(game,
             {"itens": i, "receitas": r, "tecnologias": t},
             {"itens": md_itens(i), "receitas": md_receitas(r, names),
              "tecnologias": md_tecnologias(t, names)})
    sem_nome = [x["id"] for x in i if not x["pt"]]
    fora_da_demo = [x for x in t if not x["disponivel_na_demo"]]
    print(f"\n{len(sem_nome)} itens sem nome na localizacao (ex.: {sem_nome[:5]})")
    print(f"{len(fora_da_demo)} de {len(t)} tecnologias marcadas como fora da demo")
