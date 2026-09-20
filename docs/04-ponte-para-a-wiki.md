# 4. Ponte para a keeper-wiki-fnd

O objetivo do projeto: trocar "conferido na wiki do fandom" por **"extraído do jogo"**.

A `keeper-wiki-fnd` tem uma regra dura no seu `CLAUDE.md` — *não escreva fato do jogo sem
fonte*. Hoje a fonte é a Graveyard Keeper Wiki (fandom) + guias. Com este projeto a fonte
passa a ser o binário do jogo, que é a única fonte que não erra de propósito nem
desatualiza sozinha.

## O que sai daqui pronto para consumo

| Arquivo | Registros | Conteúdo |
| ------- | --------: | -------- |
| `out/data/wiki/itens.json` | 1.157 | id, nome pt-BR e en oficiais, descrição, tipo, preço-base, qualidade, pilha |
| `out/data/wiki/receitas.json` | 2.634 | estações, entradas, saídas, tempo, energia, pontos de tecnologia, quem desbloqueia |
| `out/data/wiki/tecnologias.json` | 187 | ramo, custo em pontos, pré-requisitos, o que libera |

Formato de uma receita:

```json
{
  "id": "flitch_2",
  "origem": "craft",
  "tipo": "ResourcesBasedCraft",
  "estacoes": [{"id": "mf_saw_1", "pt": null, "en": null}],
  "entradas": [{"id": "wood", "pt": "Tora", "en": "Log", "qtd": 1.0}],
  "saidas": [{"id": "flitch", "pt": "Tábua", "en": "Flitch",
              "qtd": null, "qtd_expr": "7+Ppar(\"p_woodworker\")*2"}],
  "pontos_tecnologia": {"r": 1},
  "tempo_s": 2.0, "energia": "10-Ppar(\"p_woodworker\")*5",
  "oculta": false, "precisa_desbloquear": false,
  "liberada_por": ["Sawmill"]
}
```

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

## Rodando

```sh
./scripts/setup.sh                 # uma vez
./.venv/bin/python scripts/extrai-balance.py
./.venv/bin/python scripts/extrai-locales.py
./.venv/bin/python scripts/catalogo.py
```

Para ler com olho humano antes de virar conteúdo: `out/catalogo/receitas.md` (receitas
agrupadas por estação), `out/catalogo/itens.md` e `out/catalogo/tecnologias.md`.
