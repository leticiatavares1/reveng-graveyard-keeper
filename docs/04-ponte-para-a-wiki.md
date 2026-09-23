# 4. Ponte para a keeper-wiki-fnd

O objetivo do projeto: trocar "conferido na wiki do fandom" por **"extraído do jogo"**.

A `keeper-wiki-fnd` tem uma regra dura no seu `CLAUDE.md` — *não escreva fato do jogo sem
fonte*. Hoje a fonte é a Graveyard Keeper Wiki (fandom) + guias. Com este projeto a fonte
passa a ser o binário do jogo, que é a única fonte que não erra de propósito nem
desatualiza sozinha.

## O que sai daqui pronto para consumo

| Arquivo | GK1 | GK2 | Conteúdo |
| ------- | --: | --: | -------- |
| `out/<jogo>/data/wiki/itens.json` | 1.157 | 814 | id, nome pt-BR e en oficiais, descrição, tipo, preço-base, qualidade, pilha e (só GK1) `icone`, `estrela`, `grupo`, `pode_usar`, `ao_usar`, `ao_usar_expr` |
| `out/<jogo>/data/wiki/receitas.json` | 2.634 | 825 | estações (com `icone`, só GK1), entradas, saídas, tempo, energia, pontos de tecnologia, quem desbloqueia, e (construção) `objeto_construido.icone` |
| `out/<jogo>/data/wiki/tecnologias.json` | 187 | 236 | ramo/aba (com `icone`, só GK1), custo, pré-requisitos, o que libera |
| `out/<jogo>/icones/*.png` | 952 | — | ícone de item, bancada, objeto de construção e ramo de tecnologia, mais as três estrelas de qualidade e os 6 glifos do HUD de "dias da semana" |

Os dois jogos saem no **mesmo formato normalizado**, apesar de o balanceamento bruto ser
bem diferente — é para isso que existe um adaptador por jogo em `scripts/gk/catalogo_<jogo>.py`.
O GK2 acrescenta `energia_por_tick`, `e_automatica`, `combustivel` e, nas tecnologias,
`disponivel_na_demo`.

Formato de uma receita:

```json
{
  "id": "flitch_2",
  "origem": "craft",
  "tipo": "ResourcesBasedCraft",
  "estacoes": [{"id": "mf_saw_1", "pt": null, "en": null, "icone": null}],
  "entradas": [{"id": "wood", "pt": "Tora", "en": "Log", "qtd": 1.0}],
  "saidas": [{"id": "flitch", "pt": "Tábua", "en": "Flitch",
              "qtd": null, "qtd_expr": "7+Ppar(\"p_woodworker\")*2"}],
  "pontos_tecnologia": {"r": 1},
  "tempo_s": 2.0, "energia": "10-Ppar(\"p_woodworker\")*5",
  "oculta": false, "precisa_desbloquear": false,
  "liberada_por": ["Sawmill"]
}
```

## Ícone, estrela e grupo de qualidade

Três campos do `itens.json` do GK1 existem para a wiki mostrar o item como o jogo mostra.

**`icone`** é o nome do sprite, pela regra de `ItemDefinition.GetIcon()`: o campo `icon` do
balanceamento, ou `"i_" + id` quando ele vem vazio (483 itens). O PNG correspondente sai em
`out/gk1/icones/<icone>.png` pelo `extrai-sprites.py`.

**`estrela`** é 1, 2 ou 3 — a estrela de bronze, prata e ouro que o jogo desenha por cima do
ícone —, ou `null`. A condição é a de `GetQualityIconName()`: `quality_type == Stars` e
`quality >= 1`, e o sprite é `item_star_<estrela>.png`. Repare que **ter sufixo `:N` no id
não basta**: `hamp_crop:1/2/3` tem sufixo, mas `quality_type` Default e `quality` 0 — no
jogo, cânhamo não tem estrela nenhuma.

**`grupo`** é o id do item sem o sufixo de qualidade (`pumpkin_crop:2` → `pumpkin_crop`),
para os 242 itens que são níveis de um mesmo item. É a parte de
`Item.InitMultiqualityItems()` que interessa aqui: um id **sem definição própria** vira o
grupo dos ids que começam com `<id>:`. Daí sair de 1.157 itens para **998 entradas** — 915
itens sem grupo mais 83 grupos.

Isso resolve um buraco do dado: **164 receitas pedem o grupo, não o nível** — 80 grupos
diferentes (`pumpkin_crop`, `fish_pike`, `meal:baked_pumpkin`) —, e esses ids não existem
em `items_data`. Quem consumir
`receitas.json` sem conhecer o grupo trata a entrada como "não é item" e perde o link.

Duas coisas ficam **de fora** do `grupo`, de propósito:

- `test_paper` e `test_scroll` têm definição própria *e* variantes com sufixo; para o jogo o
  grupo nem chega a existir, e aqui também não.
- os **grupos de prefixo** (`meal`, `snack`, `dessert`, `chisel`, `cover`, `fillet_fish`,
  `brain`, `heart`, `lungs`, `intestine`, `organs`) juntam itens **diferentes** — "qualquer
  refeição", "qualquer cinzel" —, não níveis do mesmo item. São 38 receitas pedindo um
  desses onze; é outro conceito e merece outro tratamento.

## O que o item faz quando o jogador usa

Três campos respondem "para que serve isto?", que é a pergunta que a lista de
receitas não responde: comida não é ingrediente de nada, e a ficha dela acabava
em "nenhuma receita gasta este item".

**`pode_usar`** é o `can_be_used` do jogo: o item pode ser usado do inventário.
240 itens podem.

**`ao_usar`** é a parte **fixa** do efeito, `{recurso: número}`, vinda de
`params_on_use`: `{"energy": 24.0}` na abóbora assada de prata,
`{"energy": 80.0, "hp": -20.0}` na infusão. A saúde vem de um campo separado do
par `_res_type`/`_res_v`, e **pode ser negativa** — cogumelo venenoso só cobra.
189 itens têm efeito fixo: 161 só energia, 19 energia com saúde, 2 só saúde
(`pot_heal` e `shr_agaric`) e 7 que dão ponto de tecnologia.

**`ao_usar_expr`** é a parte que é fórmula, crua, pela mesma regra do `qtd_expr`:
`AddPpar("energy", 20*Ppar("food_multiplier"))` é o ganho que depende de perk, e
`AddBuff("buff_longtimer")` é o buff que a comida dá. 153 itens têm. Nenhuma das
229 expressões do GK1 vem simplificada em constante, então nenhuma vira número
sem avaliar a expressão.

> **Leia `ao_usar` junto com `pode_usar`.** Em ferramenta (`can_be_used` falso) o
> jogo guarda no mesmo campo o **custo** de energia por golpe: `axe_1` tem
> `{"energy": -1.0}`, e isso não é o que o machado devolve. São 26 itens assim.

### O que falta de arte

Dos 834 nomes de ícone, **703 têm sprite** em `resources.assets`. Dos 131 que faltam, 127
são de item `nao_usado` — arte que saiu do jogo e deixou a definição para trás. Os outros
quatro faltam **no jogo também**: `hop_honey:1/2/3` (Hidromel) e `lungs:lungs_1` (Pulmões)
caem no `"i_" + id`, e `i_hop_honey:1` não existe em coleção nenhuma. Não se inventa
substituto parecido: quem consome mostra o vazio.

## Ícone de bancada, objeto de construção e ramo de tecnologia

Item não é a única coisa com sprite no jogo. Três outros campos, todos só em GK1, seguem
a mesma regra — nome do sprite, `null` quando não existe, nunca inventado:

**`receitas[].estacoes[].icone`** segue a MESMA regra de `WorldGameObject.
GetUniversalObjectInfo()` — a função que decide o ícone no painel de interação do jogo
—, não só o `custom_icon` do `ObjectDefinition` (`objs_data`). Não existe classe própria
de "bancada"; é o mesmo objeto de qualquer coisa colocável no mapa, e o `custom_icon`
sozinho cobre pouco (87 dos 228): a maioria dos objetos nunca precisou de um, porque o
jogo tem fallback por convenção de nome que muda com `interaction_type`:

| `interaction_type` | Regra |
| --- | --- |
| Craft (1) | ícone da receita "Put" que ergue este objeto, senão `custom_icon`, senão `"i_b_" + id` |
| RunScript (2) / Builder (4) | `custom_icon`, senão `"i_z_" + id` |
| mesa de autópsia | `custom_icon`, senão `"i_b_" + id` (sem a troca pela receita) |
| Chest, Grave, None | só `custom_icon` — o jogo não tem fallback pra esses, `UniversalObjectInfo.icon` fica `null` mesmo |

Mais três ids (`grave_ground`, `mf_balsamation_1/2`) têm ícone escrito direto no C#, fora
de qualquer regra. Com o fallback, a cobertura sobe pra **169 das 228** (74%) — inclusive
a Bancada de carpintaria (`mf_workbench_1`), que tem `custom_icon` vazio mas cai no
fallback `i_b_mf_workbench_1`. Ex. de override por receita: `mf_alchemy_mill` (Moinho de
alquimia) → `i_b_alchemy_millstone`. Como no ícone de item, uma fração do fallback
adivinhado não tem sprite de verdade (nomes de objeto de teste/debug, como
`i_z_slava_test_builder`) — o `extrai-sprites.py` reporta quem falta, e quem consome
mostra o vazio.

**`receitas[].objeto_construido.icone`** é o `icon` da própria receita de construção
(`craft_obj_data`) — o sprite que o menu de construção mostra para aquele resultado.
Cobertura alta: **185 objetos distintos** (96% das 533 receitas de construção). Preferido
ao `custom_icon` do objeto erguido porque a demolição (`acao: "Remove"`) usa o mesmo
ícone da construção original, e nem todo objeto erguido tem `custom_icon` próprio.

**`tecnologias[].ramo.icone`** é fixo, `"i_tbranch_" + ramo.n` — não existe ícone por
tecnologia individual, só por ramo (`i_tbranch_1` a `i_tbranch_8`, um por
especialidade/cor). As 187 tecnologias de um mesmo ramo repetem o mesmo ícone, do
mesmo jeito que uma bancada repete o dela em cada receita que a usa.

Os três entram no mesmo `extrai-sprites.py`, na mesma pasta `out/gk1/icones/`, sem
mudar o leitor de sprites — são sprites como outro qualquer em `resources.assets`.

## Mapeando para o tipo `Recipe`

`src/lib/content/types.ts` da wiki pede:

```ts
interface Recipe { id; station; time?; ingredients; result; category; en?; note? }
```

A conversão, campo a campo:

| `Recipe` | De onde vem | Observação |
| -------- | ----------- | ---------- |
| `id` | `id` da receita | o id do jogo é estável entre builds; serve de chave |
| `station` | `estacoes[].pt` | **uma receita com N estações vira N `Recipe`**, como a wiki já faz com `ripa-cavalete`/`ripa-serra` |
| `ingredients` | `entradas` | `{name: pt, qty: qtd}` |
| `result` | `saidas[0]` | receita com mais de uma saída não cabe no tipo atual |
| `en` | `saidas[0].en` | nome oficial em inglês, que a busca da wiki já usa |
| `time` | `tempo_s` | formatar (`"2s"`); é o tempo só de craft automático |
| `note` | `pontos_tecnologia` | vira "+1 vermelho", que é o que a wiki escreve hoje |
| `category` | **decisão editorial** | o jogo não tem categoria; continua curadoria humana |

Filtros recomendados antes de gerar conteúdo: descartar `oculta: true` (62 receitas),
descartar item com `nao_usado: true` (387 itens), e tratar `origem: "construcao"` como
uma seção separada — construção não é receita de bancada.

### O que não converte automático

- **`qtd_expr`.** Quando a quantidade depende de perk (`7+Ppar("p_woodworker")*2`), o
  número base é o que o jogador vê sem o perk; o resto merece uma nota de texto. Idem
  `energia`.
- **Qualidade/estrelas.** A fórmula existe (docs/02), mas a probabilidade depende dos
  perks do jogador — é conteúdo de artigo, não de tabela.
- **Nome da estação.** Muito objeto de bancada não tem entrada na localização
  (`mf_saw_1`, `sawhorse`). O nome bonito continua sendo escolha humana.

## ⚠️ O achado que muda conteúdo já publicado

**O jogo tem tradução oficial em português, e ela não é a que a wiki está usando.**
`GJL.AVAILABLE_LOCALE_NAMES` lista "Português do Brasil", e `lng_pt-br` traz os 10.961
textos. Comparando com as 25 receitas escritas à mão na wiki, **12 usam um nome diferente
do oficial**:

| en | wiki hoje | oficial no jogo |
| -- | --------- | --------------- |
| Flitch | Ripa | **Tábua** |
| Wood billet | Tarugo de madeira | **Pedaço de madeira** |
| Wooden plank | Tábua de madeira | **Placa de madeira** |
| A polished brick of stone | Tijolo de pedra polido | Tijolo polido de pedra |
| Wooden marker | Marco de madeira | Marcador de madeira |
| Clean paper | Papel limpo | Papel vazio |
| Ink | Tinta de escrever | Tinta |
| Pigskin paper | Papel de pele de porco | Papel de couro de porco |
| Wooden/Stone grave fence | Cerca de túmulo de madeira/pedra | Cerca de madeira/pedra de túmulo |

As três primeiras são as perigosas: em português, o que a wiki chama de **"Tábua de
madeira"** o jogo chama de "Placa de madeira", enquanto **"Tábua"** no jogo é outro item
(o Flitch). Um leitor com o jogo aberto em português lê a receita errada.

**Recomendação:** adotar o nome oficial como nome canônico e guardar o nome do fandom como
apelido de busca. `Recipe.en` já existe para isso; caberia um `alias?: string[]` no tipo
para o nome antigo continuar achável. É decisão da Letícia — este projeto só traz o dado.

## ⚠️ O GK2 não pode herdar o glossário do GK1

A wiki já prevê dois jogos (`content/gk1/`, `content/gk2/`), e a tentação óbvia é reusar a
tradução de um no outro. **Não dá.** Dos 90 ids de item que existem nos dois jogos, **37
têm nome pt-BR diferente**:

| id | GK1 | GK2 |
| -- | --- | --- |
| `flitch` | Tábua | **Placa** |
| `ceramic_1` | Potes de cerâmica | **Placa de Argila** |
| `ceramic_2` | Jarro de cerâmica | **Urna de Argila** |
| `axe_1` / `axe_2` | Machado I / II | **Machado de Bronze / de Ferro** |
| `ash` | Ash *(não traduzido)* | **Cinzas** |
| `detail_1` | Peças simples de ferro | Peça de Ferro simples |
| `bag_alchemy` | Bolsa do alquimista | Bolsa de Alquimista |

Além do nome, mudou a convenção: o GK2 usa **Caixa Alta em Cada Palavra** ("Bancada de
Carpintaria I"), o GK1 não ("Bancada de cozinhar"). E o mesmo id pode ser outro item:
`ceramic_1` deixou de ser "potes" e virou "placa de argila".

Em números de locale: das 183 chaves que existem nos dois `lng_pt-br`, **103 têm texto
diferente**. A conclusão para a wiki é que nome de item é dado **por jogo**, nunca
compartilhado — que é, felizmente, como o `GameContent` já está modelado.

## Escopo da demo do GK2

O `GameBalance` da demo traz o balanceamento do **jogo completo em desenvolvimento**:
13.754 definições, com **185 das 236 tecnologias** marcadas `disponivel_na_demo: false`.
Serve para antecipar conteúdo, mas não é fato publicável — pode mudar até o lançamento.
Se for virar conteúdo, marque como provisório e filtre por `disponivel_na_demo`.

## Rodando

```sh
./scripts/setup.sh                                  # uma vez
./.venv/bin/python scripts/extrai-balance.py gk2
./.venv/bin/python scripts/extrai-locales.py gk2
./.venv/bin/python scripts/catalogo.py gk2
./.venv/bin/python scripts/extrai-sprites.py gk1    # ícones; depois do catálogo
```

Para ler com olho humano antes de virar conteúdo: `out/<jogo>/catalogo/receitas.md`
(receitas agrupadas por estação), `itens.md` e `tecnologias.md`.
