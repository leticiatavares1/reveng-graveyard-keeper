# reveng-graveyard-keeper

Engenharia reversa do **Graveyard Keeper** (Lazy Bear Games / tinyBuild, Unity) para
extrair e catalogar **todos os itens, receitas e mecânicas** do jogo — a base de dados que
alimenta a [`keeper-wiki-fnd`](../keeper-wiki-fnd/), a wiki de fã em português.

**O problema que ele resolve:** a wiki é escrita à mão a partir da Graveyard Keeper Wiki
(fandom) e de guias. Isso não escala (são 1.157 itens e 2.634 receitas), envelhece a cada
patch e já introduziu erro — metade dos nomes em português da wiki não bate com a tradução
oficial do jogo (ver [§7](#7-achados)). Aqui a fonte passa a ser **o binário do jogo**.

> ⚠️ Uso restrito: extração de dados de um jogo **comprado**, para uma wiki de fã sem fins
> lucrativos. Não redistribuir binários, assets nem o fonte recuperado.

---

## 1. O que dá para extrair

O jogo é **Unity 2020.3.17f1 com backend Mono** — não IL2CPP. Isso muda tudo: existe uma
`Assembly-CSharp.dll` de verdade, que decompila para C# legível com o mesmo `ilspycmd` do
[`metrics-reveng`](../../kromos-group/metrics-reveng/README.md). O lado de dados é
igualmente generoso:

| Fonte | Onde | O que dá |
| ----- | ---- | -------- |
| **`game_data`** (4,3 MB) | `resources.assets` | **todo o balanceamento**: 6.116 definições em 34 listas |
| **`lng_*`** (11 idiomas) | `resources.assets` | 10.961 textos por idioma — inclui **pt-BR oficial** |
| `Assembly-CSharp.dll` | `Managed/` | as regras: fórmulas de qualidade, custo de energia, lógica |
| `level0`…`level27` | raiz do `_Data` | cenas: posições, NPCs, zonas (**ainda não extraído**) |
| 21.030 Sprites | `resources.assets` | ícones e arte (**ainda não extraído**) |

Detalhe do build em [`docs/01-inventario.md`](docs/01-inventario.md).

---

## 2. Origem dos arquivos

| Caminho | Conteúdo |
| ------- | -------- |
| `~/.local/share/Steam/steamapps/common/Graveyard Keeper/` | instalação analisada (Steam, Linux) |
| `…/Graveyard Keeper_Data/` | arquivos serializados + `Managed/` |
| `…/Managed/Assembly-CSharp{,-firstpass}.dll` | os dois assemblies do estúdio |

A pasta do jogo nunca é escrita — a extração é **só leitura**. Para apontar para outra
instalação (Windows, Proton, outra máquina), exporte `GK_DATA` com o caminho do `*_Data`.

Build analisado: Steam buildid **22583570**, Unity **2020.3.17f1**.

---

## 3. Pré-requisitos

| Ferramenta | Instalação | Uso |
| ---------- | ---------- | --- |
| `python3` + `uv` | já na máquina | roda a extração |
| `UnityPy` + `TypeTreeGeneratorAPI` | `./scripts/setup.sh` | lê os arquivos serializados |
| `ilspycmd` | `dotnet tool install -g ilspycmd --version '8.*'` | decompila o C# |
| `file`, `strings` | binutils/coreutils | inventário |

> **Pegadinha herdada do metrics-reveng:** o `ilspycmd` 8.x tem alvo .NET 6 e esta máquina
> só tem runtime 8/10 — é preciso `DOTNET_ROLL_FORWARD=LatestMajor`. O
> `scripts/decompila.sh` já exporta.

---

## 4. O pipeline

```sh
./scripts/setup.sh                              # 1. cria .venv e instala as libs
./scripts/inventario.sh                         # 2. versão, assemblies, arquivos
./scripts/decompila.sh                          # 3. C# -> out/src-csharp/ (3.110 .cs)
./.venv/bin/python scripts/extrai-balance.py    # 4. game_data -> out/data/balance/
./.venv/bin/python scripts/extrai-locales.py    # 5. lng_*     -> out/data/locales/
./.venv/bin/python scripts/catalogo.py          # 6. cruza tudo -> out/data/wiki + out/catalogo
```

Os passos 4-6 levam segundos e são idempotentes. O passo 3 leva ~15 s.

### Passo 3 — decompilar

Mesma técnica do metrics-reveng: um projeto `.csproj` por assembly, um arquivo por tipo.
É daqui que saem as regras que o dado sozinho não conta — como a qualidade de um craft é
calculada, quando uma receita pode ser enfileirada, o que `min_value` significa.

### Passos 4 e 5 — ler os arquivos serializados

**O achado que faz o projeto funcionar:** o build **não embute TypeTree**, então a UnityPy
sozinha não sabe ler nenhum `MonoBehaviour`. A árvore é reconstruída a partir da
`Assembly-CSharp.dll` pelo `TypeTreeGeneratorAPI` — e ainda precisa de **duas correções**
para casar com o leitor da UnityPy (alinhamento depois de `m_Enabled`; `List<T>` emitido
com o tipo do elemento em vez de `vector`). As duas estão em `scripts/gk/typetree.py`,
explicadas em [`docs/03-pipeline-typetree.md`](docs/03-pipeline-typetree.md).

A saída é **fiel ao binário**: nada filtrado, nada renomeado. É o baseline para o diff
entre versões — a cada patch da Lazy Bear, rodar de novo e dar `git diff` mostra
exatamente o que mudou no balanceamento, sem release notes.

### Passo 6 — catálogo

Cruza balanceamento + localização e produz as duas coisas que o resto do mundo consome:
`out/data/wiki/*.json` (para virar conteúdo) e `out/catalogo/*.md` (para ler).

---

## 5. O modelo de dados em uma frase

**Todo o jogo é um ScriptableObject.** `Resources.Load<GameBalance>("game_data")` traz 34
listas de definição ligadas por `id` string; os nomes visíveis vivem separados nos `lng_*`
e se juntam pelo mesmo `id`. Rastrear uma mecânica = `id → definição → SmartExpression →
a classe C# que avalia`.

Classes, campos e armadilhas em [`docs/02-modelo-de-dados.md`](docs/02-modelo-de-dados.md).

---

## 6. Estrutura do projeto

```
reveng-graveyard-keeper/
├── README.md                     # este arquivo
├── scripts/
│   ├── setup.sh                  # cria o .venv
│   ├── inventario.sh             # versão do build, assemblies, serializados
│   ├── decompila.sh              # ilspycmd -> out/src-csharp/
│   ├── gk/typetree.py            # ← TypeTree a partir da DLL + as 2 correções
│   ├── gk/assets.py              # achar MonoBehaviour por NOME (path_id não é estável)
│   ├── extrai-balance.py         # game_data -> uma lista por arquivo
│   ├── extrai-locales.py         # lng_* -> id → texto, 11 idiomas
│   └── catalogo.py               # cruza tudo; normaliza quantidades e nomes
├── docs/
│   ├── 01-inventario.md          # o que é o build, o que tem dentro
│   ├── 02-modelo-de-dados.md     # GameBalance, as 34 listas, SmartExpression, GameRes
│   ├── 03-pipeline-typetree.md   # por que não lia e como passou a ler
│   └── 04-ponte-para-a-wiki.md   # como isso vira conteúdo da keeper-wiki-fnd
└── out/
    ├── src-csharp/               # C# recuperado (versionado)
    ├── data/balance/             # 34 listas, fiéis ao binário (versionado)
    ├── data/locales/             # 11 idiomas (versionado)
    ├── data/wiki/                # itens/receitas/tecnologias normalizados (versionado)
    └── catalogo/                 # itens.md, receitas.md, tecnologias.md (versionado)
```

**Regra do `.gitignore`**, herdada do metrics-reveng: versionar o que foi escrito à mão
**e a saída da extração**. Ela é a base de conhecimento do dia a dia e o baseline da
comparação entre builds. Ficam de fora só binários e o que é pesado-e-regenerável.

---

## 7. Achados

| Achado | Onde | Em uma linha |
| ------ | ---- | ------------ |
| **O build não tem TypeTree** | `docs/03` | Sem TypeTree, `MonoBehaviour` é byte cru. A árvore vem da `Assembly-CSharp.dll`, com duas correções que a `UnityPy` exige. É o que destrava tudo. |
| **`Item.value` é campo morto** | `docs/02` | A quantidade real de uma receita está em `min_value`/`max_value` (SmartExpression). `flitch_2` tem `value=1` mas produz **7** tábuas. A primeira rodada do catálogo divergiu da wiki do fandom por causa disso — a wiki estava certa. |
| **O jogo tem pt-BR oficial, e a wiki não usa** | `docs/04` | `lng_pt-br` traz 10.961 textos traduzidos. **12 das 25 receitas** escritas à mão na wiki usam nome diferente do oficial. Pior caso: "Tábua de madeira" na wiki é "Placa de madeira" no jogo, e "Tábua" no jogo é outro item. |
| **Quase todo número é fórmula** | `docs/02` | `SmartExpression` — `energia = 10-Ppar("p_woodworker")*5`. Número fixo é a exceção, não a regra; perks entram direto no custo e no rendimento. |
| **75 itens em uso não têm nome** | `docs/02` | Itens ativos sem entrada na localização (`onion_crop`, `1h_ore_metal`). O catálogo deixa nulo em vez de inventar. |
| **387 itens são `not_used`** | `out/data/wiki/itens.json` | Um terço do `items_data` é conteúdo cortado ou de teste. Filtrar antes de virar wiki. |

---

## 8. Status

**Eixo 1 — ler o jogo**

- [x] Inventariar o build (Unity 2020.3.17f1, Mono) → `docs/01`
- [x] Decompilar os dois assemblies → `out/src-csharp/` (3.110 `.cs`)
- [x] Resolver o TypeTree e ler `MonoBehaviour` → `docs/03`
- [x] Extrair `game_data`: 6.116 definições em 34 listas → `out/data/balance/`
- [x] Extrair as 11 localizações → `out/data/locales/`
- [ ] Extrair sprites/ícones (21.030) — decidir antes o que a wiki pode usar
- [ ] Extrair as cenas (`level*`): posições, NPCs, zonas, spawns do mapa

**Eixo 2 — catalogar**

- [x] Itens, receitas (craft + construção) e tecnologias normalizados → `out/data/wiki/`
- [x] Catálogos legíveis por estação de trabalho → `out/catalogo/`
- [ ] Catalogar quests, almas/NPCs, comerciantes, peixes, conquistas
- [ ] Grafo de dependência (o que precisa do quê) — a matéria-prima de um guia de progressão

**Eixo 3 — alimentar a wiki**

- [x] Mapear o dado extraído para o tipo `Recipe` da `keeper-wiki-fnd` → `docs/04`
- [ ] **Decisão pendente (Letícia):** adotar o nome oficial em pt-BR como canônico e manter
      o nome do fandom como apelido de busca
- [ ] Gerador que emite `recipes.ts` direto do `out/data/wiki/receitas.json`
- [ ] Rotina de atualização: a cada patch, reextrair e dar `git diff` no `out/data/`
