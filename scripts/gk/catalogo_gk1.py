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

#: ObjectDefinition.InteractionType — só os que importam para o ícone da
#: bancada (`estacao_icone`); Chest e Grave não têm regra de fallback.
INTERACTION_CRAFT = 1
INTERACTION_RUNSCRIPT = 2
INTERACTION_BUILDER = 4

#: Três ids com ícone escrito direto no C# (`WorldGameObject.
#: GetUniversalObjectInfo()`), fora de qualquer regra: a cova comum usa o
#: ícone da campa comprada, não o do buraco, e as duas mesas de embalsamamento
#: são case especial no switch por id.
ICONE_FIXO = {
    "grave_ground": "i_b_grave_place",
    "mf_balsamation_1": "i_b_mf_balsamation_1",
    "mf_balsamation_2": "i_b_mf_balsamation_2",
}

#: ItemDefinition.QualityType.Stars — o item mostra estrela de qualidade.
QUALIDADE_ESTRELAS = 1

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


def icone(it: dict) -> str:
    """Nome do sprite do item, pela regra de `ItemDefinition.GetIcon()`.

    O campo `icon` e opcional: 483 itens vem com ele vazio e o jogo cai em
    `"i_" + id`. Quem exporta o PNG e o `extrai-sprites.py`, a partir daqui.
    (`custom_ovr_icon` NAO entra: aquele e o icone que o personagem levanta
    acima da cabeca, e nao o da celula do inventario.)
    """
    return it["icon"] or f"i_{it['id']}"


def estrela(it: dict) -> int | None:
    """Nivel da estrela de qualidade (1, 2 ou 3), ou None quando nao tem.

    Mesma condicao de `ItemDefinition.GetQualityIconName()`, que monta o sprite
    como `"item_star_" + quality`. `hamp_crop:1..3` e a excecao que mostra por
    que a condicao importa: tem sufixo de qualidade, mas `quality_type` Default
    e `quality` 0 -- no jogo nao aparece estrela nenhuma.
    """
    n = int(it["quality"])
    return n if it["quality_type"] == QUALIDADE_ESTRELAS and n > 0 else None


def ao_usar(it: dict) -> dict:
    """O que o item devolve quando o jogador o usa: energia, saude, pontos.

    E a parte FIXA do efeito -- `params_on_use`, que o jogo soma sempre. A
    saude vem num campo proprio, fora do par `_res_type`/`_res_v`, e pode ser
    negativa: `infusion` tira 20 de saude junto com os 80 de energia, e
    `shr_agaric` so tira 5.

    Vale ler junto com `pode_usar`: em ferramenta (`can_be_used` falso) o
    numero aqui e o custo de energia por golpe, nao o que ela devolve.
    """
    res = game_res(it["params_on_use"])
    if it["params_on_use"]["_hp"]:
        res["hp"] = it["params_on_use"]["_hp"]
    return res


def ao_usar_expr(it: dict) -> list[str]:
    """A parte do efeito que e formula, e nao numero.

    `AddPpar("energy", 20*Ppar("food_multiplier"))` e o ganho que depende de
    perk do jogador; `AddBuff("buff_longtimer")` e o buff que a comida da. Sai
    cru, pela mesma regra do `qtd_expr`: nao se arredonda o que nao e numero.
    Nenhuma das 229 expressoes do GK1 vem simplificada em constante.
    """
    return [e["_expression"].strip() for e in it["on_use_expressions"]
            if e.get("_expression", "").strip()]


def grupos_de_qualidade(itens_data: list[dict]) -> dict[str, str]:
    """`id do item -> id do grupo`, para os itens que so mudam de qualidade.

    `pumpkin_crop:1/2/3` sao tres ItemDefinition, um por nivel, e o jogo os
    junta em `pumpkin_crop`: e a parte da regra de `Item.InitMultiqualityItems`
    que interessa a wiki -- um id sem definicao propria vira o grupo dos ids
    que comecam com `<id>:`. Por isso 80 receitas pedem `pumpkin_crop`, que nao
    existe em `items_data`.

    Duas coisas de proposito ficam de fora:
    - `test_paper` e `test_scroll`, que tem definicao propria E variantes com
      sufixo: para o jogo o grupo nem chega a existir;
    - os grupos de prefixo (`meal`, `chisel`, `fillet_fish`...), que juntam
      itens DIFERENTES ("qualquer refeicao"), e nao niveis do mesmo item.
    """
    ids = {it["id"] for it in itens_data}
    grupo = {}
    for it in itens_data:
        base, _, sufixo = it["id"].rpartition(":")
        if base and sufixo.isdigit() and base not in ids:
            grupo[it["id"]] = base
    return grupo


def itens(game: Game, names: Names) -> list[dict]:
    itens_data = carregar(game, "items_data")
    grupo = grupos_de_qualidade(itens_data)
    out = []
    for it in itens_data:
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
            "icone": icone(it),
            "estrela": estrela(it),
            "grupo": grupo.get(it["id"]),
            "pode_usar": bool(it["can_be_used"]),
            "ao_usar": ao_usar(it),
            "ao_usar_expr": ao_usar_expr(it),
        })
    return sorted(out, key=lambda i: i["id"])


def estacao_icone(obj: dict | None, put_por_objeto: dict[str, str], estacao_id: str) -> str | None:
    """Ícone da bancada, pela MESMA regra de `WorldGameObject.
    GetUniversalObjectInfo()` que decide o ícone no painel de interação do
    jogo — não é só `custom_icon`: ele tem fallback por convenção de nome, que
    muda com o tipo de interação, e um objeto de craft troca o ícone pelo da
    própria receita que o constrói.

    - Craft (1): o ícone da receita "Put" que ergue este objeto, se existir;
      senão `custom_icon`; senão `"i_b_" + id`.
    - RunScript (2) / Builder (4): `custom_icon`, senão `"i_z_" + id`.
    - Mesa de autópsia: `custom_icon`, senão `"i_b_" + id` (mesmo fallback do
      Craft, mas sem a troca pela receita).
    - Chest, Grave, None: só `custom_icon` — o jogo não tem fallback para eles
      (`UniversalObjectInfo.icon` fica `null`), então a maioria fica sem ícone
      mesmo, e isso não é bug: é o jogo também não tendo ícone ali.

    Sem essa regra, a bancada de carpintaria — a mais básica do jogo — aparecia
    sem ícone (`custom_icon` vazio), quando na verdade o jogo mostra
    `i_b_mf_workbench_1` no fallback.
    """
    if estacao_id in ICONE_FIXO:
        return ICONE_FIXO[estacao_id]
    if obj is None:
        return None
    custom = obj["custom_icon"] or None
    if obj.get("is_autopsy_table"):
        return custom or f"i_b_{estacao_id}"
    interacao = obj.get("interaction_type")
    if interacao == INTERACTION_CRAFT:
        return put_por_objeto.get(estacao_id) or custom or f"i_b_{estacao_id}"
    if interacao in (INTERACTION_RUNSCRIPT, INTERACTION_BUILDER):
        return custom or f"i_z_{estacao_id}"
    return custom


def estacao_ref(names: Names, obj_por_id: dict, put_por_objeto: dict, estacao_id: str) -> dict:
    """Referência de bancada, com o ícone do objeto de mundo que ela é.

    Não existe classe própria de "bancada": no jogo é o mesmo `ObjectDefinition`
    de qualquer objeto colocável.
    """
    ref = names.ref(estacao_id)
    ref["icone"] = estacao_icone(obj_por_id.get(estacao_id), put_por_objeto, estacao_id)
    return ref


def receitas(game: Game, names: Names) -> list[dict]:
    liberada_por: dict[str, list[str]] = defaultdict(list)
    for tech in carregar(game, "techs_data"):
        for craft_id in tech["crafts"]:
            liberada_por[craft_id.lstrip("@")].append(tech["id"])

    obj_por_id = {o["id"]: o for o in carregar(game, "objs_data")}
    # "Put" é quem ergue o objeto; ao_out_obj pode ter mais de uma receita
    # (builder trancado por perk, por exemplo) — fica a primeira encontrada,
    # como `BuildModeLogics.GetObjectPutCraftDefinition` também faria.
    put_por_objeto: dict[str, str] = {}
    for c in carregar(game, "craft_obj_data"):
        if c.get("out_obj") and c.get("build_type") == 0 and c.get("icon"):
            put_por_objeto.setdefault(c["out_obj"], c["icon"])

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
                "estacoes": [estacao_ref(names, obj_por_id, put_por_objeto, o)
                             for o in (c["craft_in"] or c.get("builder_ids", []))],
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
                # `icon` é o sprite que o próprio jogo mostra no menu de
                # construção para este resultado — mais confiável que adivinhar
                # pelo `custom_icon` do objeto erguido, que pode nem existir
                # (a demolição usa o mesmo ícone da construção original).
                rec["objeto_construido"] = (
                    {**names.ref(c["out_obj"]), "icone": c.get("icon") or None}
                    if c["out_obj"] else None
                )
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
            # locale, na chave `tbranch_<n>`. Não existe ícone por tecnologia
            # — só por ramo (`i_tbranch_<n>`, 8 sprites, um por cor/especialidade).
            "ramo": {
                "n": t["branch_type"],
                "pt": names.name(f"tbranch_{t['branch_type']}", "pt"),
                "icone": f"i_tbranch_{t['branch_type']}",
            },
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
