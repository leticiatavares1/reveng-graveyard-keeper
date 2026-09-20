# 2. O modelo de dados do jogo

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

**75 itens em uso não têm entrada na localização** (ex.: `onion_crop`, `1h_ore_metal`).
Nesses o catálogo deixa `pt`/`en` nulos em vez de inventar nome.
