# 3. O pipeline de leitura: TypeTree

Este é o achado técnico que faz o projeto existir. Sem ele, `game_data` é um bloco de
4,3 MB de bytes sem forma.

## O problema

Um arquivo serializado da Unity normalmente carrega o **TypeTree** de cada tipo: a lista
de campos, na ordem, com tipo e regra de alinhamento. É o que permite a uma ferramenta
externa ler um `MonoBehaviour` sem conhecer o código do jogo.

O build do Graveyard Keeper **não traz TypeTree**:

```python
>>> obj.serialized_type
SerializedType(class_id=114, ..., node=None, m_ClassName=None)
```

`node=None`. A UnityPy consegue listar os objetos, ler o nome e devolver os bytes crus —
e nada mais. `obj.read()` não sabe o que é aquilo.

## A solução

O `TypeTreeGeneratorAPI` reconstrói a árvore **a partir da `Assembly-CSharp.dll`**: lê os
campos da classe e aplica as regras de serialização da Unity. Como o jogo é Mono, a DLL
tem os tipos de verdade, e a árvore sai completa (3.805 nós só para `GameBalance`).

```python
gen = TypeTreeGenerator("2020.3.17f1")
gen.load_local_dll_folder(".../Managed")
nodes = gen.get_nodes("Assembly-CSharp.dll", "GameBalance")
```

## As duas correções (sem elas, nada lê)

O gerador e o leitor da UnityPy não concordam em dois pontos. Ambas as correções estão em
`scripts/gk/typetree.py`, e as duas foram descobertas por `EOFError`.

### 1. Alinhamento depois de `m_Enabled`

O gerador sintetiza o cabeçalho do `MonoBehaviour` (`m_GameObject`, `m_Enabled`,
`m_Script`, `m_Name`) mas emite `m_Enabled` com `m_MetaFlag = 0`. A Unity grava esse
`UInt8` e **alinha em 4 bytes** logo depois. Sem a flag, tudo que vem depois sai
deslocado em 3 bytes.

```python
if node["m_Level"] == 1 and node["m_Name"] == "m_Enabled":
    node["m_MetaFlag"] |= 0x4000        # align
```

### 2. `List<T>` vem nomeado com o tipo do **elemento**

A Unity emite um `List<string>` como um nó de tipo `vector`. O gerador emite como
`string`, com a subárvore de vetor pendurada. O leitor da UnityPy testa o `m_Type`
**antes** de olhar os filhos:

```
string txt_ids          <- gerador diz "string"
  Array Array
    int size            <- 10961
    string data         <- os textos
```

Resultado: ele lia os 4 bytes de `size` como o comprimento de **uma** string e tentava
consumir 10.961 bytes de texto de uma vez. `EOFError: read_str out of bounds`.

A regra que separa os dois casos: uma string de verdade tem a subárvore
`Array > (int size, char data)`. Qualquer outro nó com filho `Array` é vetor.

```python
is_real_string = node["m_Type"] == "string" and nodes[i + 3]["m_Type"] == "char"
if not is_real_string:
    node["m_Type"] = "vector"
```

## Validação

Não basta "não estourar" — um deslocamento pode produzir lixo plausível. A leitura foi
conferida por **parse manual dos bytes** (`gk/assets.py:raw_name` usa a mesma aritmética)
e por dados que só batem se tudo estiver certo:

- `lng_pt-br`: 10.961 ids e 10.961 textos, pareados, terminando exatamente no fim do blob;
- `bloody_nails_wash`: 1 prego sangrento + 1 areia do rio + 1 água → 1 prego, 2 de energia,
  na bancada de cozinhar — que é o que o jogo faz;
- `flitch` no cavalete: 1 tora → 6 tábuas, +1 ponto vermelho.

## Pegadinhas para a próxima vez

- **`path_id` não é estável entre builds.** Procure pelo nome (`game_data`, `lng_*`).
- **`ilspycmd` 8.x tem alvo .NET 6** e a máquina só tem 8/10: é preciso
  `DOTNET_ROLL_FORWARD=LatestMajor`. O `scripts/decompila.sh` já exporta.
- **A versão da Unity passada ao gerador importa.** Ela muda regras de alinhamento; use a
  que o `globalgamemanagers` informa (`scripts/inventario.sh` imprime).
