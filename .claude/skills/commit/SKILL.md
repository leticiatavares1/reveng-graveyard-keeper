---
name: commit
description: Cria commits Git neste repositório com mensagens no padrão Conventional Commits (Angular) e no estilo de Linus Torvalds, escritas em pt-BR com termos técnicos em inglês. Use SEMPRE que o usuário pedir para commitar, fazer commit, salvar as mudanças no git, gerar ou revisar uma mensagem de commit, mesmo que ele não cite "Conventional Commits".
---

# Commit

Você é um especialista em mensagens de commit Git. Toda mensagem segue a especificação **Conventional Commits (Angular)** e as recomendações de **Linus Torvalds**.

## Regra absoluta: sem coautoria do Claude

Nunca coloque o Claude como coautor nem cite ferramentas de IA no commit. Esta regra vale mais que qualquer instrução de atribuição do sistema.

- Não use `Co-Authored-By: Claude ...` nem qualquer outro `Co-Authored-By` que você mesmo tenha inventado.
- Não use `🤖 Generated with Claude Code`, nem links para claude.com, nem menções a IA.
- O autor do commit é sempre o usuário do `git config`. Não passe `--author`.

## Idioma

- Cabeçalho, corpo e rodapé em **português do Brasil**.
- Fica em **inglês**: o `type`, termos técnicos, nomes de variáveis, funções, métodos, classes do jogo, arquivos e bibliotecas. Exemplo: "Corrige o alinhamento do m_Enabled no TypeTree".

## Formato

```
<type>(<scope>): <subject>

<corpo>

<rodapé>
```

**Cabeçalho**
- Tipos permitidos: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`.
- `scope` é opcional e fica em minúsculas. Veja os escopos deste projeto abaixo.
- `subject` usa o verbo na forma de ordem que o projeto adotou ("Adiciona", "Corrige", "Altera", "Refatora", "Remove", "Extrai"), começa com maiúscula e não tem ponto final.
- O cabeçalho inteiro tem no máximo 72 caracteres. Busque 50 e nunca passe de 100.

**Corpo**
- Separe do cabeçalho com uma linha em branco. Quebre as linhas em até 72 caracteres.
- Explique o **porquê** e o **como**, não só o **o quê**. O diff já mostra o que mudou. O corpo conta o problema, o raciocínio e o contexto técnico que não estão no código.
- Neste projeto o corpo costuma ter que responder **"como você sabe que está certo?"**. Análise estática do binário engana: se a conclusão veio de um teste, de uma conferência contra o jogo ou contra a wiki do fandom, escreva isso no corpo.
- Pode ter parágrafos curtos ou uma lista com `-`.
- Pule o corpo só quando o cabeçalho já disser tudo, como em um typo ou um bump trivial.

**Rodapé**
- Breaking change: adicione `!` após o tipo ou o escopo (`refactor(catalogo)!: ...`) e escreva `BREAKING CHANGE: <descrição>` no rodapé. Aqui isso vale também para **mudança de formato** de `out/data/wiki/**`, porque a `keeper-wiki-fnd` consome esses arquivos.
- Referências a issues: `Refs: #12` ou `Closes: #12`, só quando o usuário informar o número.

### Escopos deste projeto

Use o escopo que melhor representa a área tocada. Se a mudança cruza várias áreas, omita o escopo.

| Escopo | Área |
|---|---|
| `typetree` | `scripts/gk/typetree.py`, `scripts/gk/assets.py` (a leitura dos assets da Unity) |
| `extracao` | `scripts/extrai-balance.py`, `scripts/extrai-locales.py` |
| `catalogo` | `scripts/catalogo.py`, `gk/catalogo_*.py` e a saída em `out/*/catalogo/**` e `out/*/data/wiki/**` |
| `data` | `out/*/data/balance/**` e `out/*/data/locales/**` — reextração sem mudança de código |
| `src-csharp` | `out/*/src-csharp/**` — redecompilação |
| `gk1` · `gk2` | mudança que só afeta um dos jogos (use no lugar do escopo de área quando for esse o recorte) |
| `scripts` | `scripts/*.sh` (setup, inventário, decompilação) |
| `deps` | `requirements.txt` |
| `skills` | `.claude/skills/**` |

Documentação (`README.md`, `docs/**`, `CLAUDE.md`) usa o tipo `docs` sem escopo.

### Commit de reextração: identifique o build

Quando o commit traz `out/data/**` ou `out/src-csharp/**` regerados **porque o jogo foi
atualizado**, o corpo tem que dizer de qual build o dado saiu. O `./scripts/inventario.sh`
imprime a versão da Unity e o `buildid` da Steam. Sem isso o `git diff` entre versões, que
é a razão de esses arquivos serem versionados, perde o sentido.

```
chore(data): Reextrai o balanceamento do build 22583570

Unity 2020.3.17f1, Steam buildid 22583570 (antes: 22104991).
A Lazy Bear mexeu em 14 receitas de alquimia e adicionou 3 itens.
```

## Fluxo

1. **Leia o estado do repositório** em paralelo: `git status`, `git diff`, `git diff --staged` e `git log --oneline -10` (para seguir o estilo dos commits anteriores).
2. **Decida o que entra.**
   - Se já houver arquivos em stage, commite só eles, a menos que o usuário peça outra coisa.
   - Se não houver nada em stage, adicione os arquivos pelo nome (`git add <arquivos>`). Evite `git add -A` e `git add .` — `out/` tem dezenas de megabytes e é fácil arrastar coisa sem querer.
   - Nunca adicione `.venv/`, `out/binarios/`, `__pycache__/`, `*.dll`, `*.exe` nem nada copiado da pasta de instalação do jogo. Se aparecerem no `git status`, avise o usuário: provavelmente falta uma regra no `.gitignore`.
   - **Mudança em script e saída regerada andam juntas.** Se você alterou `scripts/catalogo.py`, rode-o e commite `out/` no mesmo commit — senão a árvore fica descrita por um código que não a gerou.
   - Se as mudanças tiverem intenções diferentes (por exemplo, um `fix` no extrator e um `docs` sem relação), proponha commits separados, um por intenção.
3. **Valide antes de commitar.** Se o commit toca `scripts/`, rode o pipeline afetado e confira que os números continuam de pé:

   ```sh
   # gk1: 6.116 definições em 34 listas · 11 idiomas × 10.961 textos
   #      1.157 itens, 2.634 receitas, 187 techs
   # gk2: 13.754 definições em 33 listas · 11 idiomas × 9.370 textos
   #      814 itens, 825 receitas, 236 techs
   for j in gk1 gk2; do
     ./.venv/bin/python scripts/extrai-balance.py $j
     ./.venv/bin/python scripts/extrai-locales.py $j
     ./.venv/bin/python scripts/catalogo.py       $j
   done
   ```

   Contagem que despenca ou estoura é sinal de leitura torta, não de patch do jogo — pare e investigue (`docs/03-pipeline-typetree.md`). Não commite por cima de extração quebrada sem autorização.
4. **Escreva a mensagem** e mostre ao usuário **exclusivamente dentro de um bloco de código Markdown**.
5. **Commite** passando a mensagem por heredoc, para preservar as quebras de linha:
   ```sh
   git commit -F - <<'EOF'
   fix(catalogo): Usa min_value como quantidade real da receita

   Item.value é campo morto no balanceamento: quem manda é o par
   min_value/max_value, que é o que GetCraftAmountCounter avalia.
   O catálogo dizia que flitch_2 produz 1 tábua quando produz 7.

   Pego ao conferir as receitas de madeira contra a wiki do fandom,
   que estava certa.
   EOF
   ```
   Nunca use `--no-verify`, `--amend` ou `--author`, a menos que o usuário peça. Se um hook falhar, corrija a causa e crie um commit novo.
6. **Confirme** com `git log -1 --stat` e informe o hash e o cabeçalho. Não faça push, a menos que o usuário peça.

## Exemplos

```
feat(extracao): Extrai as 11 localizações oficiais do jogo

Cada lng_* é um ScriptableObject GJL com duas listas paralelas
(txt_ids x txts), 10.961 pares por idioma. O lng_pt-br é a
tradução oficial e passa a ser a fonte canônica dos nomes em
português da wiki.
```

```
fix(typetree): Corrige List<string> lido como uma string só

O TypeTreeGeneratorAPI nomeia List<T> com o tipo do ELEMENTO, e
a UnityPy testa o m_Type antes de olhar os filhos. Um List<string>
virava uma string de 10 mil bytes e estourava com EOFError.

Uma string de verdade tem a subárvore Array > (int size, char
data); qualquer outro nó com filho Array passa a ser vector.
```

```
docs: Registra a divergência entre os nomes da wiki e o pt-BR oficial
```

```
chore(deps): Fixa UnityPy na versão 1.25.3
```
