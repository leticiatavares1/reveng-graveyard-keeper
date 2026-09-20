# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Leia o `README.md` antes de mexer em qualquer coisa** — ele é o documento mestre
(o que é o jogo, o pipeline, a estrutura e os achados).

## O que este projeto é

Extração de dados dos dois **Graveyard Keeper** (Unity + backend **Mono**) para alimentar
a wiki em `../keeper-wiki-fnd/`. O projeto se espelha, em espírito e estrutura, no
`~/programing/kromos-group/metrics-reveng`: scripts reprodutíveis, saída de decompilação
versionada, documentação por assunto em `docs/`.

Dois jogos, um pipeline: **`gk1`** (Graveyard Keeper, Unity 2020.3.17f1) e **`gk2`**
(demo do Graveyard Keeper 2, Unity 6000.3.9f1). Todo script recebe o jogo como primeiro
argumento e escreve em `out/<jogo>/`.

Tudo em português (pt-BR), inclusive comentários de código.

## Comandos

```sh
./scripts/setup.sh                                  # cria .venv e instala UnityPy
./scripts/inventario.sh        gk1|gk2              # versão do build e arquivos
./scripts/decompila.sh         gk1|gk2              # C# -> out/<jogo>/src-csharp/
./.venv/bin/python scripts/extrai-balance.py gk1|gk2   # -> out/<jogo>/data/balance/
./.venv/bin/python scripts/extrai-locales.py gk1|gk2   # -> out/<jogo>/data/locales/
./.venv/bin/python scripts/catalogo.py       gk1|gk2   # -> out/<jogo>/{data/wiki,catalogo}
```

`GK1_DATA` / `GK2_DATA` apontam para a pasta `*_Data` quando a instalação não é a padrão
da Steam.

## Regras

- **A pasta do jogo é somente leitura.** Nenhum script escreve nela, nem em `~/.steam`.
- **`out/data/balance/` é fiel ao binário.** Não filtre, não renomeie, não "limpe" campo
  ali — é o baseline do `git diff` entre builds do jogo. Normalização e escolha editorial
  acontecem depois, no `catalogo.py`, e saem em `out/data/wiki/`.
- **Nunca procure objeto por `path_id`** — ele muda a cada build. Use o nome
  (`gk/assets.py:monobehaviours`).
- **Diferença entre jogos mora em `scripts/gk/games.py`.** Classe, assembly, nome do asset,
  nomes dos campos de localização: tudo que muda entre `gk1` e `gk2` fica no registro. Se
  você está prestes a escrever `if game.id == "gk1"` fora de `catalogo_gk1.py`, provavelmente
  falta um campo no `Game`.
- **Classe em namespace precisa do nome completo** no gerador de TypeTree
  (`LazyBearTechnology.LL`, não `LL`) — senão ele falha com "Object reference not set".
- **Nome de item não é compartilhado entre os jogos.** 37 dos 90 ids comuns têm tradução
  pt-BR diferente no GK1 e no GK2. Nunca reuse glossário de um no outro.
- **Desconfie de campo que parece óbvio.** `Item.value` existe, é um int, e está errado:
  a quantidade real de uma receita é `min_value`/`max_value` (SmartExpression). Antes de
  publicar um número, confira contra o comportamento do jogo ou contra a wiki do fandom;
  quando divergir, ache a razão no C# decompilado antes de decidir quem está certo.
- **Não invente nome.** Item sem entrada na localização fica com `pt`/`en` nulos.
- Ao mexer em `scripts/gk/typetree.py`, releia `docs/03` — as duas correções ali não são
  cosméticas: sem elas a leitura estoura ou, pior, devolve lixo plausível.

## Commits

Use a skill do projeto `.claude/skills/commit` para qualquer commit: Conventional Commits
(Angular) em pt-BR, com os termos técnicos em inglês — a mesma convenção da
`keeper-wiki-fnd`, com os escopos e as validações deste repositório. **Nunca coloque o
Claude como coautor nem cite IA na mensagem** (sem `Co-Authored-By: Claude`, sem
"Generated with Claude Code").
