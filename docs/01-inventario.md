# 1. Inventário do build

Regerar com `./scripts/inventario.sh`.

## O que é o jogo, tecnicamente

| Item | Valor |
| ---- | ----- |
| Estúdio | Lazy Bear Games |
| Engine | **Unity 2020.3.17f1** |
| Backend de script | **Mono** (não IL2CPP) — por isso o C# volta legível |
| Steam AppID / buildid | 599140 / 22583570 |
| Instalação analisada | `~/.local/share/Steam/steamapps/common/Graveyard Keeper/` |

O build é Mono, e isso define todo o projeto: existe `Assembly-CSharp.dll`, um assembly
.NET de verdade, que o `ilspycmd` devolve como C# quase original — o mesmo decompilador e
a mesma pegadinha de runtime do `metrics-reveng`. Se fosse IL2CPP, o caminho seria Ghidra.

## Binários do jogo (decompiláveis)

| Arquivo | Tamanho | Papel |
| ------- | ------: | ----- |
| `Managed/Assembly-CSharp.dll` | 2.587 KiB | **o jogo**: definições de balanceamento, GUI, lógica |
| `Managed/Assembly-CSharp-firstpass.dll` | 2.260 KiB | plugins pré-compilados: NGUI, `GJL` (localização), `SmartExpression` |

Todo o resto de `Managed/` é runtime da Unity, BCL do Mono ou biblioteca de terceiros
(DOTween, Rewired, Odin/Sirenix, A* Pathfinding, LitJson, Newtonsoft) — não é código do
estúdio e não interessa à extração.

## Arquivos serializados

| Arquivo | Tamanho | Contém |
| ------- | ------: | ------ |
| `resources.assets` | 93,8 MB | **tudo que importa**: `game_data`, os 11 `lng_*`, sprites, ícones |
| `level0` … `level27` | 5 KB – 70 MB | cenas (mapa, objetos posicionados, NPCs) |
| `globalgamemanagers` | 9,6 MB | configuração do player; é de onde sai a versão da Unity |

`resources.assets` tem **53.728 MonoBehaviours**, 43.988 GameObjects e 21.030 Sprites.
Os dois MonoBehaviours que sustentam este projeto:

| Objeto | Tamanho | Classe | O que é |
| ------ | ------: | ------ | ------- |
| `game_data` | 4,35 MB | `GameBalance` | o balanceamento inteiro, em um ScriptableObject só |
| `lng_pt-br` (e mais 10) | ~0,85 MB | `GJL` | a localização oficial, id → texto |

> Os `path_id` **não** são estáveis entre builds. Os scripts sempre acham o objeto
> **pelo nome** (`gk/assets.py:raw_name`), nunca por id.

## O que ainda não foi extraído

- **Sprites/ícones** (21 mil) — a UnityPy exporta; falta decidir o que a wiki pode usar.
- **Cenas** (`level*`) — onde estão as posições de NPC, zonas e spawns do mapa.
- **Diálogos / quests em FlowCanvas** — o grafo está em `Assembly-CSharp.dll` como classes
  `Flow_*`/`NodeCanvas.*`, mas os dados do grafo vivem nas cenas.
