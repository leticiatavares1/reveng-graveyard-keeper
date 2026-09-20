#!/usr/bin/env python3
"""Cruza balanceamento + localizacao e gera os catalogos navegaveis.

Le out/data/balance e out/data/locales (saida dos dois `extrai-*.py`) e escreve:

  out/data/wiki/itens.json      itens normalizados, com nome oficial pt-BR e en
  out/data/wiki/receitas.json   receitas normalizadas (inclui as de construcao)
  out/catalogo/itens.md         tabela de itens
  out/catalogo/receitas.md      receitas por estacao de trabalho
  out/catalogo/tecnologias.md   arvore de tecnologia, custo e o que libera

Os `.json` sao a materia-prima para o conteudo da keeper-wiki-fnd; os `.md` sao
para ler. Ver docs/04-ponte-para-a-wiki.md.

    ./scripts/catalogo.py
"""
from __future__ import annotations

import json
import os
import sys
from collections import defaultdict

BALANCE = "out/data/balance"
LOCALES = "out/data/locales"
OUT_JSON = "out/data/wiki"
OUT_MD = "out/catalogo"

#: ItemDefinition.ItemType (Assembly-CSharp.dll)
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

#: TechDefinition.TECH_POINTS -- saidas que sao pontos de tecnologia, nao itens.
TECH_POINTS = ["r", "g", "b", "v", "gratitude_points"]
TECH_POINT_LABEL = {
    "r": "vermelho", "g": "verde", "b": "azul", "v": "roxo",
    "gratitude_points": "gratidao",
}


def load_balance(name: str):
    with open(os.path.join(BALANCE, f"{name}.json"), encoding="utf8") as fh:
        return json.load(fh)


def load_locale(lng: str) -> dict[str, str]:
    with open(os.path.join(LOCALES, f"{lng}.json"), encoding="utf8") as fh:
        return json.load(fh)["strings"]


def expr(value):
    """SmartExpression -> numero quando e constante, senao a expressao crua."""
    if not isinstance(value, dict):
        return value
    if value.get("_simplified"):
        return round(value["_simpified_float"], 4)
    return value.get("_expression") or None


def game_res(res: dict) -> dict:
    """GameRes -> {tipo: valor}, so o que e diferente de zero."""
    pairs = zip(res.get("_res_type", []), res.get("_res_v", []))
    return {t: v for t, v in pairs if v}


class Names:
    """Nome e descricao de um id, no idioma oficial do jogo."""

    def __init__(self, pt: dict[str, str], en: dict[str, str]):
        self.pt, self.en = pt, en

    def name(self, item_id: str, lng: str) -> str | None:
        table = self.pt if lng == "pt" else self.en
        # ItemDefinition.GetItemName: item com estrelas (`id:2`) cai no id base.
        for key in (item_id, item_id.rsplit(":", 1)[0]):
            if key in table:
                return table[key]
        return None

    def desc(self, item_id: str, lng: str) -> str | None:
        table = self.pt if lng == "pt" else self.en
        for key in (item_id + "_d", item_id.rsplit(":", 1)[0] + "_d"):
            if key in table:
                return table[key]
        return None

    def ref(self, item_id: str) -> dict:
        return {"id": item_id, "pt": self.name(item_id, "pt"), "en": self.name(item_id, "en")}

    def item_ref(self, item: dict) -> dict:
        """Entrada/saida de receita, ja com a quantidade REAL.

        `Item.value` e um campo morto no balanceamento: quem manda e o par
        `min_value`/`max_value` (SmartExpression), que e o que
        `WorldGameObject.GetCraftAmountCounter` avalia. Ex.: `flitch_2` tem
        value=1 mas min_value='7+Ppar("p_woodworker")*2' -- sao 7 taboas, nao 1.
        """
        out = self.ref(item["id"])
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

    def label(self, item_id: str) -> str:
        return self.name(item_id, "pt") or self.name(item_id, "en") or item_id


def build_items(names: Names) -> list[dict]:
    out = []
    for it in load_balance("items_data"):
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
    out.sort(key=lambda i: i["id"])
    return out


def build_recipes(names: Names) -> list[dict]:
    # Qual tecnologia libera qual receita (TechDefinition.crafts).
    unlocked_by: dict[str, list[str]] = defaultdict(list)
    for tech in load_balance("techs_data"):
        for craft_id in tech["crafts"]:
            unlocked_by[craft_id.lstrip("@")].append(tech["id"])

    out = []
    for source, crafts in (("craft", load_balance("craft_data")),
                           ("construcao", load_balance("craft_obj_data"))):
        for c in crafts:
            saidas, pontos = [], {}
            for item in c["output"]:
                ref = names.item_ref(item)
                if item["id"] in TECH_POINTS:
                    pontos[item["id"]] = ref.get("qtd", ref.get("qtd_expr"))
                else:
                    saidas.append(ref)
            rec = {
                "id": c["id"],
                "origem": source,
                "tipo": CRAFT_TYPES.get(c["craft_type"], str(c["craft_type"])),
                # Receita de construcao nao usa `craft_in`: a "estacao" e quem
                # constroi (`builder_ids`, p.ex. o canteiro de obras).
                "estacoes": [names.ref(o) for o in (c["craft_in"] or c.get("builder_ids", []))],
                "entradas": [names.item_ref(i) for i in c["needs"]],
                "entradas_da_estacao": [names.item_ref(i) for i in c["needs_from_wgo"]],
                "saidas": saidas,
                "pontos_tecnologia": pontos,
                "tempo_s": expr(c["craft_time"]),
                "energia": expr(c["energy"]),
                "sanidade": expr(c["sanity"]),
                "dificuldade": c["difficulty"],
                "oculta": bool(c["hidden"]),
                "precisa_desbloquear": bool(c["needs_unlock"]),
                "perks": c["linked_perks"],
                "liberada_por": unlocked_by.get(c["id"], []),
            }
            if source == "construcao":
                rec["objeto_construido"] = names.ref(c["out_obj"]) if c["out_obj"] else None
                rec["acao"] = BUILD_TYPES.get(c["build_type"], str(c["build_type"]))
            out.append(rec)
    out.sort(key=lambda r: r["id"])
    return out


def md_items(items: list[dict]) -> str:
    rows = ["# Itens", "",
            f"{len(items)} itens em `items_data`. Nome pt-BR e o da traducao oficial do jogo.",
            "", "| id | pt-BR | en | tipo | preco | qualidade | pilha |",
            "| -- | ----- | -- | ---- | ----: | --------: | ----: |"]
    for i in items:
        if i["nao_usado"]:
            continue
        rows.append("| `{id}` | {pt} | {en} | {tipo} | {preco_base:g} | {qualidade:g} | {pilha} |".format(
            **{**i, "pt": i["pt"] or "—", "en": i["en"] or "—"}))
    return "\n".join(rows) + "\n"


def md_recipes(recipes: list[dict], names: Names) -> str:
    by_station: dict[str, list[dict]] = defaultdict(list)
    for r in recipes:
        for st in r["estacoes"] or [{"id": "(sem estacao)", "pt": None, "en": None}]:
            by_station[st["id"]].append(r)

    rows = ["# Receitas por estacao", "",
            f"{len(recipes)} receitas (`craft_data` + `craft_obj_data`), "
            f"{len(by_station)} estacoes.", ""]
    for station in sorted(by_station):
        label = names.label(station)
        rows += [f"## {label} — `{station}`", "",
                 "| receita | precisa | produz | tempo | energia | pontos |",
                 "| ------- | ------- | ------ | ----: | ------: | ------ |"]
        for r in sorted(by_station[station], key=lambda x: x["id"]):
            def fmt(lst):
                out = []
                for e in lst:
                    qtd = e.get("qtd_expr") or (f"{e['qtd']:g}" if e.get("qtd") is not None else "?")
                    if e.get("qtd_max") is not None:
                        hi = e["qtd_max"]
                        qtd += f"–{hi:g}" if isinstance(hi, (int, float)) else f"–{hi}"
                    out.append(f"{qtd}x {e['pt'] or e['en'] or e['id']}")
                return "<br>".join(out) or "—"
            pontos = " ".join(f"{v} {TECH_POINT_LABEL.get(k, k)}"
                              for k, v in r["pontos_tecnologia"].items()) or "—"
            rows.append("| `{id}` | {n} | {o} | {t} | {e} | {p} |".format(
                id=r["id"], n=fmt(r["entradas"]), o=fmt(r["saidas"]),
                t=r["tempo_s"] if r["tempo_s"] is not None else "—",
                e=r["energia"] if r["energia"] is not None else "—", p=pontos))
        rows.append("")
    return "\n".join(rows) + "\n"


def build_techs(names: Names) -> list[dict]:
    out = []
    for t in load_balance("techs_data"):
        out.append({
            "id": t["id"],
            "pt": names.name(t["id"], "pt"),
            "en": names.name(t["id"], "en"),
            # TechBranchDefinition so guarda o numero do ramo; o nome vem do
            # locale, na chave `tbranch_<n>`.
            "ramo": {"n": t["branch_type"], "pt": names.name(f"tbranch_{t['branch_type']}", "pt")},
            "custo": game_res(t["price"]),
            "requer": t["_parents"],
            "libera_receitas": t["crafts"],
            "libera_perks": t["perks"],
            "oculta": bool(t["hidden"]) or bool(t["invisible"]),
            "requer_dlc": t["requires_dlc"],
        })
    out.sort(key=lambda x: (x["ramo"]["n"], x["id"]))
    return out


def md_techs(names: Names) -> str:
    rows = ["# Tecnologias", "",
            "Custo em pontos de tecnologia e o que cada no libera (`techs_data`).",
            "", "| tecnologia | ramo | custo | libera |",
            "| ---------- | ---- | ----- | ------ |"]
    for t in sorted(load_balance("techs_data"), key=lambda x: (x["branch_type"], x["y"], x["x"])):
        price = " ".join(f"{v:g} {TECH_POINT_LABEL.get(k, k)}" for k, v in game_res(t["price"]).items())
        libera = ", ".join(f"`{c}`" for c in t["crafts"][:8]) or "—"
        if len(t["crafts"]) > 8:
            libera += f" (+{len(t['crafts']) - 8})"
        branch = names.label(f"tbranch_{t['branch_type']}")
        rows.append(f"| {names.label(t['id'])} (`{t['id']}`) | {branch} | {price or '—'} | {libera} |")
    return "\n".join(rows) + "\n"


def main() -> None:
    if not os.path.isdir(BALANCE):
        raise SystemExit("Rode antes: ./scripts/extrai-balance.py e ./scripts/extrai-locales.py")
    os.makedirs(OUT_JSON, exist_ok=True)
    os.makedirs(OUT_MD, exist_ok=True)
    names = Names(load_locale("pt-br"), load_locale("en"))

    items = build_items(names)
    recipes = build_recipes(names)
    techs = build_techs(names)

    for path, payload in ((f"{OUT_JSON}/itens.json", items),
                          (f"{OUT_JSON}/receitas.json", recipes),
                          (f"{OUT_JSON}/tecnologias.json", techs)):
        with open(path, "w", encoding="utf8") as fh:
            json.dump(payload, fh, ensure_ascii=False, indent=1)
        print(f"  {path:32} {len(payload)} registros")

    for path, text in ((f"{OUT_MD}/itens.md", md_items(items)),
                       (f"{OUT_MD}/receitas.md", md_recipes(recipes, names)),
                       (f"{OUT_MD}/tecnologias.md", md_techs(names))):
        with open(path, "w", encoding="utf8") as fh:
            fh.write(text)
        print(f"  {path:32} {len(text.splitlines())} linhas")

    sem_nome = [i["id"] for i in items if not i["pt"] and not i["nao_usado"]]
    print(f"\n{len(sem_nome)} itens em uso sem nome na localizacao (ex.: {sem_nome[:5]})")


if __name__ == "__main__":
    main()
