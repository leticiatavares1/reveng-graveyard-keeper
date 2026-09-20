# 1. Inventário dos builds

Regerar com `./scripts/inventario.sh gk1` / `gk2`.

## Os dois jogos, lado a lado

| | **GK1** | **GK2 (demo)** |
| --- | --- | --- |
| Steam appid / buildid | 599140 / 22583570 | 5075680 / 25344626 |
| Engine | Unity **2020.3.17f1** | Unity **6000.3.9f1** |
| Backend de script | **Mono** | **Mono** |
| Build analisado | Linux nativo | Windows (roda em Proton) |
| Assembly do estúdio | `Assembly-CSharp.dll` (2.587 KiB) | `Assembly-CSharp.dll` (3.813 KiB) + `LazyBearTechnology.dll` (505 KiB) |
| `.cs` recuperados | 3.110 | 2.744 |
| Onde está o balanceamento | `resources.assets` → `game_data` | `resources.assets` → `GameBalance` |
| Distribuição de assets | `level0`…`level27` | **Addressables**: 24.062 bundles, 876 MB |

Os dois são Mono, e isso define o projeto: existe uma `Assembly-CSharp.dll` de verdade,
que o `ilspycmd` devolve como C# quase original — o mesmo decompilador e a mesma pegadinha
de runtime do `metrics-reveng`. Se fosse IL2CPP, o caminho seria Ghidra.

## GK1 — arquivos serializados

| Arquivo | Tamanho | Contém |
| ------- | ------: | ------ |
| `resources.assets` | 93,8 MB | **tudo que importa**: `game_data`, os 11 `lng_*`, sprites |
| `level0` … `level27` | 5 KB – 70 MB | cenas (mapa, objetos posicionados, NPCs) |
| `globalgamemanagers` | 9,6 MB | configuração do player; é de onde sai a versão da Unity |

53.728 MonoBehaviours, 43.988 GameObjects, 21.030 Sprites.

## GK2 — arquivos serializados

| Arquivo | Tamanho | Contém |
| ------- | ------: | ------ |
| `resources.assets` | 59,1 MB | `GameBalance`, os 11 `lng_*`, `DialogData`, fontes |
| `sharedassets0.resource` | 276 MB | áudio |
| `StreamingAssets/aa/` | 876 MB | **Addressables**: arte e cenas, 24.062 bundles |

Só 841 MonoBehaviours em `resources.assets` — bem menos que o GK1, porque quase tudo migrou
para os bundles. **O balanceamento não migrou**: continua em `resources.assets`, carregado
por `Resources.Load<GameBalance>("GameBalance")`. Os bundles não precisam ser abertos para
extrair dado de jogo.

### Presentes no GK2 e ausentes no GK1

- **`StreamingAssets/ModdingTools/VoiceOvers/`** — a Lazy Bear publicou suporte oficial a
  mod de voz, com um `voiceover_lines.json` (todas as falas dubláveis, nos 11 idiomas) e um
  README explicando o formato. É dado de jogo entregue de graça, em texto puro.
- **`DialogData`** (0,31 MB em `resources.assets`) — os diálogos, via
  `Resources.Load<DialogDataContainer>("Locales/DialogData")`.
- **`lng_tr`** — turco entrou na lista de idiomas; o GK1 tem 11 sem turco, o GK2 tem 11 com.

## O que ainda não foi extraído

- **Sprites/ícones** — a UnityPy exporta; falta decidir o que a wiki pode usar.
- **Cenas** (GK1 `level*`) e **Addressables** (GK2) — posições de NPC, zonas, spawns.
- **Diálogos do GK2** (`DialogData` + `voiceover_lines.json`).
