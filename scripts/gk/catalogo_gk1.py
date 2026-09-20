"""Catálogo do Graveyard Keeper 1."""
from __future__ import annotations

from collections import defaultdict

from .catalogo_comum import (PONTOS, Names, carregar, escrever, lista_txt,
                             pontos_txt)
from .games import Game

#: ItemDefinition.ItemType
ITEM_TYPES = {
    -1: "PseudoitemFirst", 0: "None", 1: "Axe", 2: "Pickaxe", 3: "Shovel", 4: "Sword",
    5: "Hammer", 6: "FishingRod", 9: "Torch", 10: "Hand", 11: "Item", 12: "HeadArmor",
    13: "BodyArmor", 20: "Preach", 31: "Bait", 50: "Crate", 60: "Rat", 61: "RatBuff",
    101: "GraveStone", 102: "GraveFence", 103: "GraveCover", 200: "Body",
    201: "BodyHead", 202: "BodyBody", 203: "BodyArmR", 204: "BodyArmL",
    205: "BodyLegR", 206: "BodyLegL", 210: "BodyHeadPart", 220: "BodyBodyPart",
    230: "BodyArmPart", 250: "BodyLegPart", 270: "BodyUniversalPart",
    271: "SoulBodyPart", 280: "Soul", 300: "ZombieWorker", 400: "Bag",
    10101: "GraveStoneReq", 10102: "GraveFenceReq", 10103: "GraveCoverReq",
}

#: CraftDefinition.CraftType
CRAFT_TYPES = {
    0: "None", 1: "ResourcesBasedCraft", 2: "Survey", 3: "MixedCraft", 4: "Fixing",
    5: "AlchemyDecompose", 6: "PrayCraft", 7: "RatBuff", 8: "RefugeeCampCraft",
}

#: ObjectCraftDefinition.BuildType
BUILD_TYPES = {0: "Put", 1: "Remove", 2: "None"}

#: TechDefinition.TECH_POINTS — saídas que são ponto de tecnologia, não item.
TECH_POINTS = ["r", "g", "b", "v", "gratitude_points"]


def expr(valor):
    """SmartExpression -> número quando é constante, senão a expressão crua."""
    if not isinstance(valor, dict):
        return valor
    if valor.get("_simplified"):
        return round(valor["_simpified_float"], 4)
    return valor.get("_expression") or None


def game_res(res: dict) -> dict:
    """GameRes -> {tipo: valor}, só o que é diferente de zero."""
    return {t: v for t, v in zip(res.get("_res_type", []), res.get("_res_v", [])) if v}


def item_ref(names: Names, item: dict) -> dict:
    """Entrada/saída de receita, já com a quantidade REAL.

    `Item.value` é um campo morto no balanceamento: quem manda é o par
    `min_value`/`max_value` (SmartExpression), que é o que
    `WorldGameObject.GetCraftAmountCounter` avalia. Ex.: `flitch_2` tem
    value=1 mas min_value='7+Ppar("p_woodworker")*2' — são 7 tábuas, não 1.
    """
    out = names.ref(item["id"])
    lo, hi = expr(item.get("min_value")), expr(item.get("max_value"))
    if lo is None:
        out["qtd"] = item["value"]
    elif isinstance(lo, str):
        out["qtd"] = None
        out["qtd_expr"] = lo
    else:
        out["qtd"] = lo
    if hi is not None and hi != lo:
        out["qtd_max"] = hi
    return out


def itens(game: Game, names: Names) -> list[dict]:
    out = []
    for it in carregar(game, "items_data"):
        out.append({
            "id": it["id"],
            "pt": names.name(it["id"], "pt"),
            "en": names.name(it["id"], "en"),
            "descricao_pt": names.desc(it["id"], "pt"),
            "descricao_en": names.desc(it["id"], "en"),
            "tipo": ITEM_TYPES.get(it["type"], str(it["type"])),
            "preco_base": it["base_price"],
            "qualidade": it["quality"],
            "pilha": it["stack_count"],
            "eficiencia": it["efficiency"],
            "tem_durabilidade": bool(it["has_durability"]),
            "nao_usado": bool(it["not_used"]),
            "tipos_de_produto": it["product_types"],
        })
    return sorted(out, key=lambda i: i["id"])


def receitas(game: Game, names: Names) -> list[dict]:
    liberada_por: dict[str, list[str]] = defaultdict(list)
    for tech in carregar(game, "techs_data"):
        for craft_id in tech["crafts"]:
            liberada_por[craft_id.lstrip("@")].append(tech["id"])

    out = []
    for origem, crafts in (("craft", carregar(game, "craft_data")),
                           ("construcao", carregar(game, "craft_obj_data"))):
        for c in crafts:
            saidas, pontos = [], {}
            for item in c["output"]:
                ref = item_ref(names, item)
                if item["id"] in TECH_POINTS:
                    pontos[item["id"]] = ref.get("qtd", ref.get("qtd_expr"))
                else:
                    saidas.append(ref)
            rec = {
                "id": c["id"],
                "origem": origem,
                "tipo": CRAFT_TYPES.get(c["craft_type"], str(c["craft_type"])),
                # Receita de construção não usa `craft_in`: a "estação" é quem
                # constrói (`builder_ids`, p.ex. o canteiro de obras).
                "estacoes": [names.ref(o) for o in (c["craft_in"] or c.get("builder_ids", []))],
                "entradas": [item_ref(names, i) for i in c["needs"]],
                "entradas_da_estacao": [item_ref(names, i) for i in c["needs_from_wgo"]],
                "saidas": saidas,
                "pontos_tecnologia": pontos,
                "tempo_s": expr(c["craft_time"]),
                "energia": expr(c["energy"]),
                "sanidade": expr(c["sanity"]),
                "dificuldade": c["difficulty"],
                "oculta": bool(c["hidden"]),
                "precisa_desbloquear": bool(c["needs_unlock"]),
                "perks": c["linked_perks"],
                "liberada_por": liberada_por.get(c["id"], []),
            }
            if origem == "construcao":
                rec["objeto_construido"] = names.ref(c["out_obj"]) if c["out_obj"] else None
                rec["acao"] = BUILD_TYPES.get(c["build_type"], str(c["build_type"]))
            out.append(rec)
    return sorted(out, key=lambda r: r["id"])


def tecnologias(game: Game, names: Names) -> list[dict]:
    out = []
    for t in carregar(game, "techs_data"):
        out.append({
            "id": t["id"],
            "pt": names.name(t["id"], "pt"),
            "en": names.name(t["id"], "en"),
            # TechBranchDefinition só guarda o número do ramo; o nome vem do
            # locale, na chave `tbranch_<n>`.
            "ramo": {"n": t["branch_type"], "pt": names.name(f"tbranch_{t['branch_type']}", "pt")},
            "custo": game_res(t["price"]),
            "requer": t["_parents"],
            "libera_receitas": t["crafts"],
            "libera_perks": t["perks"],
            "oculta": bool(t["hidden"]) or bool(t["invisible"]),
            "requer_dlc": t["requires_dlc"],
        })
    return sorted(out, key=lambda x: (x["ramo"]["n"], x["id"]))


def md_itens(itens: list[dict]) -> str:
    linhas = ["# Itens", "",
              f"{len(itens)} itens em `items_data`. Nome pt-BR e o da traducao oficial do jogo.",
              "", "| id | pt-BR | en | tipo | preco | qualidade | pilha |",
              "| -- | ----- | -- | ---- | ----: | --------: | ----: |"]
    for i in itens:
        if i["nao_usado"]:
            continue
        linhas.append("| `{id}` | {pt} | {en} | {tipo} | {preco_base:g} | {qualidade:g} | {pilha} |".format(
            **{**i, "pt": i["pt"] or "—", "en": i["en"] or "—"}))
    return "\n".join(linhas) + "\n"


def md_receitas(receitas: list[dict], names: Names) -> str:
    por_estacao: dict[str, list[dict]] = defaultdict(list)
    for r in receitas:
        for st in r["estacoes"] or [{"id": "(sem estacao)"}]:
            por_estacao[st["id"]].append(r)

    linhas = ["# Receitas por estacao", "",
              f"{len(receitas)} receitas (`craft_data` + `craft_obj_data`), "
              f"{len(por_estacao)} estacoes.", ""]
    for estacao in sorted(por_estacao):
        linhas += [f"## {names.label(estacao)} — `{estacao}`", "",
                   "| receita | precisa | produz | tempo | energia | pontos |",
                   "| ------- | ------- | ------ | ----: | ------: | ------ |"]
        for r in sorted(por_estacao[estacao], key=lambda x: x["id"]):
            linhas.append("| `{id}` | {n} | {o} | {t} | {e} | {p} |".format(
                id=r["id"], n=lista_txt(r["entradas"]), o=lista_txt(r["saidas"]),
                t=r["tempo_s"] if r["tempo_s"] is not None else "—",
                e=r["energia"] if r["energia"] is not None else "—",
                p=pontos_txt(r["pontos_tecnologia"])))
        linhas.append("")
    return "\n".join(linhas) + "\n"


def md_tecnologias(game: Game, techs: list[dict], names: Names) -> str:
    # A tabela segue a ordem do desenho da arvore (ramo, depois y e x), que diz
    # mais do que a alfabetica usada no JSON.
    por_id = {t["id"]: t for t in techs}
    ordenadas = sorted(carregar(game, "techs_data"),
                       key=lambda x: (x["branch_type"], x["y"], x["x"]))
    linhas = ["# Tecnologias", "",
              "Custo em pontos de tecnologia e o que cada no libera (`techs_data`).",
              "", "| tecnologia | ramo | custo | libera |",
              "| ---------- | ---- | ----- | ------ |"]
    for bruto in ordenadas:
        t = por_id[bruto["id"]]
        custo = " ".join(f"{v:g} {PONTOS.get(k, k)}" for k, v in t["custo"].items())
        libera = ", ".join(f"`{c}`" for c in t["libera_receitas"][:8]) or "—"
        if len(t["libera_receitas"]) > 8:
            libera += f" (+{len(t['libera_receitas']) - 8})"
        linhas.append(f"| {t['pt'] or t['id']} (`{t['id']}`) | {t['ramo']['pt'] or t['ramo']['n']} "
                      f"| {custo or '—'} | {libera} |")
    return "\n".join(linhas) + "\n"


def run(game: Game, names: Names) -> None:
    i, r, t = itens(game, names), receitas(game, names), tecnologias(game, names)
    escrever(game,
             {"itens": i, "receitas": r, "tecnologias": t},
             {"itens": md_itens(i), "receitas": md_receitas(r, names),
              "tecnologias": md_tecnologias(game, t, names)})
    sem_nome = [x["id"] for x in i if not x["pt"] and not x["nao_usado"]]
    print(f"\n{len(sem_nome)} itens em uso sem nome na localizacao (ex.: {sem_nome[:5]})")
