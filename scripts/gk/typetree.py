"""TypeTree das classes do jogo, geradas a partir das DLLs Mono.

Nenhum dos dois jogos embute TypeTree nos arquivos serializados
(`SerializedType.node is None`), entao a UnityPy nao sabe sozinha como ler um
MonoBehaviour. O TypeTreeGeneratorAPI reconstroi a arvore lendo a
`Assembly-CSharp.dll` -- mas emite dois detalhes diferentes do que o leitor da
UnityPy espera. As duas correcoes estao em `tree()` e sao a chave de todo o
pipeline; ver docs/03-pipeline-typetree.md.
"""
from __future__ import annotations

import os

from UnityPy.helpers.TypeTreeGenerator import TypeTreeGenerator
from UnityPy.helpers.TypeTreeNode import TypeTreeNode

from .games import Game

ALIGN_FLAG = 0x4000

_gens: dict[str, TypeTreeGenerator] = {}
_cache: dict[tuple[str, str, str], TypeTreeNode] = {}


def managed_dir(game: Game) -> str:
    return os.path.join(game.env_data_dir(), "Managed")


def generator(game: Game) -> TypeTreeGenerator:
    gen = _gens.get(game.id)
    if gen is None:
        if not os.path.isdir(managed_dir(game)):
            raise SystemExit(
                f"Pasta do jogo nao encontrada: {game.env_data_dir()}\n"
                f"Aponte {game.id.upper()}_DATA para o `*_Data` de {game.nome}."
            )
        gen = TypeTreeGenerator(os.environ.get("GK_UNITY_VERSION", game.unity))
        gen.load_local_dll_folder(managed_dir(game))
        _gens[game.id] = gen
    return gen


def tree(game: Game, assembly: str, cls: str) -> TypeTreeNode:
    """Arvore de tipos de `cls`, pronta para `ObjectReader.read_typetree()`.

    `cls` precisa do nome COMPLETO quando a classe esta em namespace (o
    `LazyBearTechnology.LL` do GK2); com o nome curto o gerador falha com
    "Object reference not set to an instance of an object".
    """
    key = (game.id, assembly, cls)
    if key in _cache:
        return _cache[key]

    nodes = [
        {
            "m_Level": n.m_Level,
            "m_Type": n.m_Type,
            "m_Name": n.m_Name,
            "m_MetaFlag": n.m_MetaFlag,
            "m_ByteSize": 0,
            "m_Version": 1,
        }
        for n in generator(game).get_nodes(assembly, cls)
    ]

    for i, node in enumerate(nodes):
        # (1) A Unity alinha em 4 bytes depois de `m_Enabled`; o gerador sintetiza
        #     o cabecalho do MonoBehaviour sem essa flag e tudo depois sai torto.
        if node["m_Level"] == 1 and node["m_Name"] == "m_Enabled":
            node["m_MetaFlag"] |= ALIGN_FLAG

        # (2) O gerador nomeia `List<T>`/`T[]` com o tipo do ELEMENTO, e nao com
        #     "vector". A UnityPy decide "isso e uma string?" pelo m_Type antes de
        #     olhar os filhos, entao um `List<string>` era lido como UMA string
        #     gigante (EOFError). Uma string de verdade tem a subarvore
        #     Array > (int size, char data); qualquer outro no com filho Array e vetor.
        has_array_child = (
            i + 3 < len(nodes)
            and nodes[i + 1]["m_Type"] == "Array"
            and nodes[i + 1]["m_Level"] == node["m_Level"] + 1
        )
        if has_array_child:
            is_real_string = node["m_Type"] == "string" and nodes[i + 3]["m_Type"] == "char"
            if not is_real_string:
                node["m_Type"] = "vector"

    root = TypeTreeNode.from_list(nodes)
    _cache[key] = root
    return root
