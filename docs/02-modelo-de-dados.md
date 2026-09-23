# 2. O modelo de dados do jogo

Este documento descreve o **GK1** em detalhe e, em [§GK2](#o-gk2-o-mesmo-desenho-reescrito),
o que muda no GK2. A ideia é a mesma nos dois; os nomes é que mudaram quase todos.

## Uma frase

**Todo o balanceamento do Graveyard Keeper é um único ScriptableObject.**
`GameBalance.LoadGameBalance()` faz `Resources.Load<GameBalance>("game_data")`, e esse
objeto de 4,3 MB carrega 34 listas de definição — itens, receitas, objetos, tecnologias,
peixes, almas, conquistas. Não há banco, não há JSON externo, não há AssetBundle: quem lê
`game_data` lê o jogo inteiro.

Os nomes visíveis ao jogador **não** estão nele. Ficam nos 11 `lng_*` (classe `GJL`),
ligados por id. A junção dos dois é o que o `scripts/catalogo.py` faz.

## As 34 listas (`out/data/balance/`)

| Lista | Nº | Classe | O que é |
| ----- | -: | ------ | ------- |
| `craft_data` | 2101 | `CraftDefinition` | receitas de item (inclui alquimia, sermão, autópsia) |
| `objs_data` | 1318 | `ObjectDefinition` | objetos do mundo: bancadas, árvores, móveis, NPCs |
| `items_data` | 1157 | `ItemDefinition` | itens (770 em uso; o resto é `not_used`) |
| `craft_obj_data` | 533 | `ObjectCraftDefinition` | receitas de **construção** (montar/remover objeto) |
| `techs_data` | 187 | `TechDefinition` | nós da árvore de tecnologia |
| `spawners_data` | 169 | `SpawnerDefinition` | o que nasce onde |
| `achievements_data` | 125 | `AchievementDefinition` | conquistas Steam |
| `logics_data` | 81 | `LogicDefinition` | lógicas de objeto (fornalha, horta, colmeia…) |
| `souls_data` | 63 | `SoulDefinition` | almas |
| `bodies_data` | 54 | `BodyDefinition` | corpos e suas partes |
| `world_zones_data` | 45 | `WorldZoneDefinition` | zonas do mapa |
| `perks_data` | 44 | `PerkDefinition` | perks (entram na qualidade do craft) |
| `buffs_data` | 34 | `BuffDefinition` | buffs |
| `quests_data` | 32 | `QuestDefinition` | quests |
| `vendors_data` | 31 | `VendorDefinition` | comerciantes e o que compram/vendem |
| `fishes_data` | 26 | `FishDefinition` | peixes |
| `object_groups` | 22 | `ObjectGroupDefinition` | agrupamento de objetos |
| `product_types_data` | 22 | `ProductTypeDefinition` | tipos de produto (taverna) |
| `cutscenes_data` | 17 | `CutscenesDLCDefinition` | cutscenes de DLC |
| `projectiles_data` | 10 | `ProjectileDefinition` | projéteis |
| `pray_events_data` · `tech_branches_data` · `tools_data` | 8 | — | eventos de oração · ramos da árvore · tipos de ferramenta |
| `reservoirs_data` · `transport_paths` | 5 | — | pesqueiros · rotas de transporte |
| `tavern_events` | 4 | `TavernEventDefinition` | eventos da taverna |
| `works_data` | 3 | `WorkDefinition` | trabalhos |
| `auras_data` · `workers_data` | 2 | — | auras · zumbis trabalhadores |
| `chars_data`, `grade_levels`, `grave_requirement_data`, `jobs_atom_data`, `jobs_data` | 0 | — | **vazias neste build** |

Total: **6.116 definições**. Cada lista vira um arquivo em `out/data/balance/<lista>.json`,
fiel ao binário (nada filtrado, nada renomeado) — é o baseline do diff entre versões.

## Os tipos que aparecem em tudo

### `BalanceBaseObject`
Só um `id` string. Toda definição herda dele, e o `id` é a chave universal: liga receita a
item, item a localização, tecnologia a receita.

### `SmartExpression` — números que não são números
Quase todo campo numérico do balanceamento é uma expressão, não uma constante:

```json
{"_expression": "10-Ppar(\"p_woodworker\")*5", "_simplified": false, "_simpified_float": 0.0}
```

`_simplified = true` quer dizer que a expressão é um número puro e `_simpified_float` vale.
Senão, é fórmula: `Ppar("p_woodworker")` é o nível do perk do jogador. O helper
`expr()` do `catalogo.py` devolve o número quando dá, e a string crua quando não dá — a
fórmula é informação, não ruído.

### `GameRes` — saco de recursos
Duas listas paralelas, `_res_type` × `_res_v`. É assim que se lê o custo de uma tecnologia:

```json
{"_res_type": ["r","g","b","v","gratitude_points"], "_res_v": [40,5,0,0,0]}
```

40 pontos vermelhos e 5 verdes. Os cinco tipos estão em `TechDefinition.TECH_POINTS`:
`r` vermelho, `g` verde, `b` azul, `v` roxo, `gratitude_points` gratidão.

### `Item` — ⚠️ `value` é campo morto
Uma entrada/saída de receita é um `Item`, e ele tem **três** campos de quantidade:
`value` (int), `min_value` e `max_value` (SmartExpression).

**Quem manda é `min_value`/`max_value`.** É o que `WorldGameObject.GetCraftAmountCounter`
avalia. O `value` ficou para trás e mente:

| receita | `value` | `min_value` | verdade |
| ------- | ------: | ----------- | ------- |
| `flitch_2` (serra) | 1 | `7+Ppar("p_woodworker")*2` | **7** tábuas, +2 por nível do perk |
| `wood_balk_1` (serra) | 1 | `3+Ppar("p_woodworker")` | **3** vigas |
| `flitch` (cavalete) | 6 | `6` | 6 tábuas |

Foi esse detalhe que fez a primeira rodada do catálogo divergir da wiki do fandom. A
correção está em `Names.item_ref()`.

## Receitas

`CraftDefinition` é grande; o que importa para a wiki:

| Campo | Significado |
| ----- | ----------- |
| `craft_in` | ids dos objetos onde a receita aparece (a "estação") |
| `needs` / `output` | entradas e saídas (`Item`, ver acima) |
| `needs_from_wgo` | o que a estação consome dela mesma — na prática, `fire` (lenha) |
| `craft_time`, `energy`, `sanity` | custo, como `SmartExpression` |
| `difficulty`, `linked_perks`, `linked_buffs` | entram no cálculo de qualidade (estrelas) |
| `hidden`, `needs_unlock` | receita escondida / precisa de desbloqueio |
| `craft_type` | `ResourcesBasedCraft`, `Survey`, `MixedCraft`, `Fixing`, `AlchemyDecompose`, `PrayCraft`, `RatBuff`, `RefugeeCampCraft` |

Saídas cujo id é `r`/`g`/`b`/`v`/`gratitude_points` **não são itens**: são os pontos de
tecnologia que o craft dá. O catálogo separa isso em `pontos_tecnologia`.

`ObjectCraftDefinition` (construção) estende a anterior e muda uma coisa importante:
`craft_in` fica vazio e a "estação" é `builder_ids` (o canteiro de obras). `build_type`
diz `Put` (constrói, consome `needs`) ou `Remove` (demole, devolve em `output`; id com
prefixo `:r:`).

### Qualidade (estrelas)
`GetMultiqualityResult` é explícito: `valor = média da qualidade dos ingredientes + soma
das estrelas dos perks + buffs − dificuldade da receita`, e esse valor vira as três
probabilidades de 1, 2 e 3 estrelas via `Clamp`. Está em
`out/src-csharp/Assembly-CSharp/CraftDefinition.cs`.

## Localização (`GJL`)

Duas listas paralelas `txt_ids` × `txts`, 10.961 pares por idioma, 11 idiomas
(`en de fr pt-br es ru it pl ja zh_cn ko`). Convenções:

| Chave | Conteúdo |
| ----- | -------- |
| `<id>` | nome do item/objeto/tecnologia |
| `<id>_d` | descrição |
| `tbranch_<n>` | nome do ramo da árvore de tecnologia |

Item com estrelas (`id:2`) cai no id base — é o que `ItemDefinition.GetItemName` faz, e o
`Names.name()` do catálogo repete.

O locale também traz uma tabela `aliases`, e o `GJL.L()` do jogo a consulta **antes** do
dicionário, em cadeia: `1h_ore_metal` → `t_iron_ore_2` → "Minério de ferro". O
`carregar_locale()` resolve os aliases do mesmo jeito. Sem isso, 51 itens, 20 bancadas e
7 tecnologias ficavam sem nome, e os órgãos modificados (`blood:blood_1_0`) caíam no nome
base, "Sangue", em vez de "Sangue modificado". Alias cujo alvo não está no dicionário faz o
jogo mostrar o alvo cru ("Advanced gravestones"), o que não é tradução, e é ignorado.

**24 itens em uso não têm nome nem com alias**: são todos bônus de sermão (`b_circle:1`,
`b_techpoint_red:3`…). Nesses o catálogo deixa `pt`/`en` nulos em vez de inventar nome.


---

# O GK2: o mesmo desenho, reescrito

`Resources.Load<GameBalance>("GameBalance")` continua trazendo o jogo inteiro. O que mudou:

| | GK1 | GK2 |
| --- | --- | --- |
| Asset | `game_data` | `GameBalance` |
| Classes | `ItemDefinition`, `CraftDefinition`… | `ItemDef`, `CraftDef`… (sufixo `Def`) |
| Campos | `snake_case` (`base_price`) | `camelCase` (`basePrice`) |
| Expressões | `SmartExpression` | `LazyExpression` (em `LazyBearTechnology.dll`) |
| Localização | `GJL` | `LazyBearTechnology.LL` |
| Quantidade de receita | `min_value`/`max_value` (e `value` morto) | `count`, com `minValue`/`maxValue` para faixa |
| Saída de receita | lista `output` | objeto `outputItems` com `chanceOutputItems` |
| Pontos de tecnologia | itens de saída com id `r`/`g`/`b` | campos próprios `techRed`/`techGreen`/`techBlue` |
| Custo de tecnologia | `GameRes price` | `redSpheresPrice`/`greenSpheresPrice`/`blueSpheresPrice` (int) |
| Ramo da tecnologia | `branch_type` → locale `tbranch_<n>` | `tab` (`TechTreeTab`) → locale `tech_tab_<Nome>` |

As convenções que **não** mudaram: `id` é a chave universal, o nome vem do locale na chave
`<id>`, a descrição em `<id>_d`, e id com sufixo (`item:2`) cai no id base.

## As 33 listas do GK2 (`out/gk2/data/balance/`)

| Lista | Nº |
| ----- | -: |
| `alchemyMixSourceDefs` | 6960 |
| `wgoDefs` | 1443 |
| `craftDefs` | 825 |
| `itemDefs` | 814 |
| `buildingDefs` | 614 |
| `questDefs` | 538 |
| `surveyDefs` | 396 |
| `inspirationDefs` | 382 |
| `perkDefs` | 310 |
| `wsoDefs` | 303 |
| `techDefs` | 236 |
| `talentLevelUpDefs` | 179 |
| `worldZoneDefs` | 105 |
| `vendorOrderDefs` | 74 |
| `bodyDefs` | 71 |
| `townBuildingDefs` | 63 |
| `gameLogicsDefs` | 61 |
| `sermonDefs` | 58 |
| `constDefs` | 53 |
| `achievementDefs` | 38 |
| `fighterDefs` | 34 |
| `fishingDefs` | 33 |
| `talentExpLevelDefs` | 30 |
| `alchemyFormulaDefs` | 29 |
| `fightDefinitions` | 26 |
| `vendorDefs` | 21 |
| `toolTypes` · `wgoGroupDefs` | 12 |
| `gameResSystemDefs` | 10 |
| `porterStationDefs` | 9 |
| `mercenariesDefs` | 7 |
| `talentDefs` | 5 |
| `sermonConfigDefs` | 3 |

**13.754 definições** — mais que o dobro do GK1, numa demo. `wgoDefs` (World Game Objects)
é o que o GK1 chamava de `objs_data`; `wsoDefs` são objetos de cenário estáticos;
`alchemyMixSourceDefs`, com quase 7 mil linhas, é a tabela de combinação da alquimia.

## `LazyExpression`

```json
{"expressionString": "1", "pureValueType": 1, "pureValueFloat": 1.0, "pureValueBool": false}
```

`pureValueType` é o enum `PureValueType`: `0 None`, `1 Float`, `2 Bool`, `3 String`.
Quando é `Float`, `pureValueFloat` vale; quando é `None` e há `expressionString`, é
fórmula. O helper `expr()` de `catalogo_gk2.py` aplica exatamente isso.

## Receita no GK2

```json
{
  "id": "wooden_plank",
  "craftsIn": ["woodworking_workbench_1", "woodworking_workbench_2"],
  "needItems": [{"id": "flitch", "count": {...}, "groupType": 0}],
  "outputItems": {
    "chanceOutputItems": [{"id": "wooden_plank", "count": {...}, "minValue": {...},
                           "maxValue": {...}, "chance": {...}, "isStarGroup": false}],
    "groupChanceOutputItems": []
  },
  "duration": {...}, "energyPerTick": {...}, "techRed": {...}
}
```

Duas pegadinhas:

- **`outputItems` é objeto, não lista.** As saídas estão em `chanceOutputItems`; as
  alternativas com chance ficam em `groupChanceOutputItems[].chanceItems` (só uma receita
  usa isso na demo, `energy_potion_3`, com chance `perk_alchemist`).
- **Os pontos de tecnologia saíram das saídas.** No GK1 eram itens de id `r`/`g`/`b`
  misturados no `output`; aqui são `techRed`/`techGreen`/`techBlue`, cada um uma expressão.

## Escopo da demo

`TechDef.isAvailableInDemo` separa o que a demo libera: **185 das 236 tecnologias estão
fora**. O arquivo traz o balanceamento do jogo em desenvolvimento inteiro — útil para
antecipar conteúdo, arriscado para publicar como fato: pode mudar até o lançamento.
