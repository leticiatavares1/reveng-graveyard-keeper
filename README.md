# reveng-graveyard-keeper

Engenharia reversa dos jogos **Graveyard Keeper** (Lazy Bear Games / tinyBuild, Unity) para
extrair e catalogar **todos os itens, receitas e mecânicas** — a base de dados que alimenta
a [`keeper-wiki-fnd`](../keeper-wiki-fnd/), a wiki de fã em português.

**O problema que ele resolve:** a wiki é escrita à mão a partir da Graveyard Keeper Wiki
(fandom) e de guias. Isso não escala (só o GK1 tem 1.157 itens e 2.634 receitas), envelhece
a cada patch e já introduziu erro — metade dos nomes em português da wiki não bate com a
tradução oficial do jogo (ver [§7](#7-achados)). Aqui a fonte passa a ser **o binário**.

| Jogo | Build | Engine | Definições | Textos por idioma |
| ---- | ----- | ------ | ---------: | ----------------: |
| **GK1** — Graveyard Keeper | Steam `22583570` | Unity 2020.3.17f1 (Mono) | 6.116 | 10.961 × 11 |
| **GK2** — Graveyard Keeper 2 (**demo**) | Steam `25344626` | Unity 6000.3.9f1 (Mono) | 13.754 | 9.370 × 11 |

> ⚠️ Uso restrito: extração de dados de jogos **comprados/instalados legalmente**, para uma
> wiki de fã sem fins lucrativos. Não redistribuir binários, assets nem o fonte recuperado.

---

## 1. O que dá para extrair

Os dois jogos são **Unity com backend Mono** — não IL2CPP. Isso muda tudo: existe uma
`Assembly-CSharp.dll` de verdade, que decompila para C# legível com o mesmo `ilspycmd` do
[`metrics-reveng`](../../kromos-group/metrics-reveng/README.md). E os dois guardam o jogo
inteiro num ScriptableObject só.

| Fonte | Onde | O que dá |
| ----- | ---- | -------- |
| **balanceamento** (~4,3 MB nos dois) | `resources.assets` | **tudo**: itens, receitas, objetos, tecnologias, quests |
| **`lng_*`** (11 idiomas cada) | `resources.assets` | os textos oficiais — inclui **pt-BR** |
| `Assembly-CSharp.dll` (+ `LazyBearTechnology.dll` no GK2) | `Managed/` | as regras: fórmulas, custos, lógica |
| cenas / Addressables | `level*` (GK1), `StreamingAssets/aa` (GK2, 876 MB) | mapa, NPCs, arte (**ainda não extraído**) |
| `DialogData` (GK2) | `resources.assets` | diálogos (**ainda não extraído**) |

Detalhe dos dois builds em [`docs/01-inventario.md`](docs/01-inventario.md).

---

## 2. Origem dos arquivos

| Caminho | Conteúdo |
| ------- | -------- |
| `~/.local/share/Steam/steamapps/common/Graveyard Keeper/` | GK1 (Steam, Linux nativo) |
| `~/.local/share/Steam/steamapps/common/Graveyard Keeper 2 Demo/` | GK2 demo (build Windows, roda em Proton) |

A pasta do jogo nunca é escrita — a extração é **só leitura**. Para apontar para outra
instalação, exporte `GK1_DATA` ou `GK2_DATA` com o caminho do `*_Data`.

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

Todo script recebe o jogo (`gk1` ou `gk2`) como primeiro argumento.

```sh
./scripts/setup.sh                                  # 1. uma vez: cria .venv e instala as libs
./scripts/inventario.sh gk2                         # 2. versão, assemblies, arquivos
./scripts/decompila.sh gk2                          # 3. C# -> out/gk2/src-csharp/
./.venv/bin/python scripts/extrai-balance.py gk2    # 4. balanceamento -> out/gk2/data/balance/
./.venv/bin/python scripts/extrai-locales.py gk2    # 5. lng_*         -> out/gk2/data/locales/
./.venv/bin/python scripts/catalogo.py gk2          # 6. cruza tudo    -> out/gk2/{data/wiki,catalogo}
```

Os passos 4-6 levam segundos e são idempotentes. O passo 3 leva ~15 s.

### Passos 4 e 5 — ler os arquivos serializados

**O achado que faz o projeto funcionar:** nenhum dos builds embute TypeTree, então a UnityPy
sozinha não sabe ler nenhum `MonoBehaviour`. A árvore é reconstruída a partir da
`Assembly-CSharp.dll` pelo `TypeTreeGeneratorAPI` — e ainda precisa de **duas correções**
para casar com o leitor da UnityPy (alinhamento depois de `m_Enabled`; `List<T>` emitido
com o tipo do elemento em vez de `vector`). As duas estão em `scripts/gk/typetree.py`,
explicadas em [`docs/03-pipeline-typetree.md`](docs/03-pipeline-typetree.md), e valem
igual para Unity 2020 e Unity 6.

A saída é **fiel ao binário**: nada filtrado, nada renomeado. É o baseline para o diff
entre versões — a cada patch, rodar de novo e dar `git diff` mostra exatamente o que mudou
no balanceamento, sem release notes.

### Passo 6 — catálogo

Cruza balanceamento + localização e produz `out/<jogo>/data/wiki/*.json` (para virar
conteúdo) e `out/<jogo>/catalogo/*.md` (para ler). O esquema do balanceamento mudou muito
entre os dois jogos, então cada um tem o seu adaptador em `scripts/gk/catalogo_<jogo>.py`.

---

## 5. O modelo de dados em uma frase

**Todo o jogo é um ScriptableObject.** `Resources.Load<GameBalance>(...)` traz 33-34 listas
de definição ligadas por `id` string; os nomes visíveis vivem separados nos `lng_*` e se
juntam pelo mesmo `id`. Rastrear uma mecânica = `id → definição → expressão → a classe C#
que avalia`.

O GK2 é o mesmo desenho reescrito: as classes viraram `*Def`, os campos passaram para
camelCase e o `SmartExpression` virou `LazyExpression`. Classes, campos e armadilhas dos
dois em [`docs/02-modelo-de-dados.md`](docs/02-modelo-de-dados.md).

---

## 6. Estrutura do projeto

```
reveng-graveyard-keeper/
├── README.md                     # este arquivo
├── scripts/
│   ├── setup.sh                  # cria o .venv
│   ├── inventario.sh <jogo>      # versão do build, assemblies, serializados
│   ├── decompila.sh  <jogo>      # ilspycmd -> out/<jogo>/src-csharp/
│   ├── extrai-balance.py <jogo>  # balanceamento -> uma lista por arquivo
│   ├── extrai-locales.py <jogo>  # lng_* -> id → texto
│   ├── catalogo.py       <jogo>  # cruza tudo; normaliza quantidades e nomes
│   └── gk/
│       ├── games.py              # ← registro dos jogos: tudo que difere mora aqui
│       ├── typetree.py           # ← TypeTree a partir da DLL + as 2 correções
│       ├── assets.py             # achar MonoBehaviour por NOME (path_id não é estável)
│       ├── catalogo_comum.py     # Names, quantidades, tabelas
│       └── catalogo_gk1.py · catalogo_gk2.py   # um adaptador por jogo
├── docs/
│   ├── 01-inventario.md          # o que é cada build, o que tem dentro
│   ├── 02-modelo-de-dados.md     # GameBalance, as listas, expressões, armadilhas
│   ├── 03-pipeline-typetree.md   # por que não lia e como passou a ler
│   └── 04-ponte-para-a-wiki.md   # como isso vira conteúdo da keeper-wiki-fnd
└── out/
    ├── gk1/{src-csharp,data/{balance,locales,wiki},catalogo}
    └── gk2/{src-csharp,data/{balance,locales,wiki},catalogo}
```

**Regra do `.gitignore`**, herdada do metrics-reveng: versionar o que foi escrito à mão
**e a saída da extração**. Ela é a base de conhecimento do dia a dia e o baseline da
comparação entre builds. Ficam de fora só binários e o que é pesado-e-regenerável.

---

## 7. Achados

| Achado | Onde | Em uma linha |
| ------ | ---- | ------------ |
| **Nenhum build tem TypeTree** | `docs/03` | Sem TypeTree, `MonoBehaviour` é byte cru. A árvore vem da `Assembly-CSharp.dll`, com duas correções que a `UnityPy` exige. É o que destrava tudo — e funciona igual em Unity 2020 e Unity 6. |
| **Classe em namespace precisa do nome completo** | `docs/03` | `LL` falha com "Object reference not set to an instance of an object"; `LazyBearTechnology.LL` funciona. Meia hora perdida nisso. |
| **`Item.value` é campo morto (GK1)** | `docs/02` | A quantidade real está em `min_value`/`max_value`. `flitch_2` tem `value=1` mas produz **7** tábuas. A wiki do fandom estava certa e o catálogo errado. |
| **O jogo tem pt-BR oficial, e a wiki não usa** | `docs/04` | **12 das 25 receitas** escritas à mão na wiki usam nome diferente do oficial. Pior caso: "Tábua de madeira" na wiki é "Placa de madeira" no jogo, e "Tábua" no jogo é outro item. |
| **GK1 e GK2 traduzem o mesmo item de formas diferentes** | `docs/04` | Dos 90 ids de item que existem nos dois, **37 têm nome pt-BR diferente** — `flitch` é "Tábua" no GK1 e "Placa" no GK2; `ceramic_1` muda de "Potes de cerâmica" para "Placa de Argila". Glossário compartilhado entre os jogos seria erro. |
| **Quase todo número é fórmula** | `docs/02` | `energia = 10-Ppar("p_woodworker")*5`. Número fixo é a exceção; perks entram direto no custo e no rendimento. |
| **A demo do GK2 traz o balanceamento inteiro** | `docs/01` | 13.754 definições, mais que o dobro do GK1 — mas **185 das 236 tecnologias** estão marcadas `isAvailableInDemo = false`. O dado é do jogo completo em desenvolvimento e pode mudar. |

---

## 8. Status

**Eixo 1 — ler os jogos**

- [x] Inventariar os dois builds → `docs/01`
- [x] Decompilar os assemblies → `out/gk1/src-csharp` (3.110 `.cs`), `out/gk2/src-csharp` (2.744)
- [x] Resolver o TypeTree e ler `MonoBehaviour` → `docs/03`
- [x] Extrair o balanceamento: 6.116 (GK1) + 13.754 (GK2) definições
- [x] Extrair as localizações: 11 idiomas em cada jogo
- [ ] Extrair sprites/ícones — decidir antes o que a wiki pode usar
- [ ] Extrair cenas (GK1 `level*`) e Addressables (GK2, 24.062 bundles)
- [ ] Extrair `DialogData` do GK2 (diálogos + o `voiceover_lines.json` do ModdingTools)

**Eixo 2 — catalogar**

- [x] Itens, receitas e tecnologias normalizados nos dois jogos → `out/<jogo>/data/wiki/`
- [x] Catálogos legíveis por estação de trabalho → `out/<jogo>/catalogo/`
- [ ] Catalogar quests, NPCs, comerciantes, peixes, conquistas
- [ ] Grafo de dependência (o que precisa do quê) — matéria-prima de um guia de progressão

**Eixo 3 — alimentar a wiki**

- [x] Mapear o dado extraído para o tipo `Recipe` da `keeper-wiki-fnd` → `docs/04`
- [ ] **Decisão pendente (Letícia):** adotar o nome oficial em pt-BR como canônico e manter
      o nome do fandom como apelido de busca
- [ ] Gerador que emite `recipes.ts` direto do `out/<jogo>/data/wiki/receitas.json`
- [ ] Rotina de atualização: a cada patch, reextrair e dar `git diff` no `out/`
