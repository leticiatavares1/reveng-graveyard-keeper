"""TypeTree das classes do jogo, geradas a partir das DLLs Mono.

O build do Graveyard Keeper nao embute TypeTree nos arquivos serializados
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

#: Pasta `*_Data` do jogo. Pode ser trocada pela variavel de ambiente GK_DATA.
DATA_DIR = os.environ.get(
    "GK_DATA",
    os.path.expanduser(
        "~/.local/share/Steam/steamapps/common/Graveyard Keeper/Graveyard Keeper_Data"
    ),
)

#: Versao da Unity do build (lida de `globalgamemanagers`).
UNITY_VERSION = os.environ.get("GK_UNITY_VERSION", "2020.3.17f1")

ALIGN_FLAG = 0x4000

_gen: TypeTreeGenerator | None = None
_cache: dict[tuple[str, str], TypeTreeNode] = {}


def managed_dir() -> str:
    return os.path.join(DATA_DIR, "Managed")


def generator() -> TypeTreeGenerator:
    global _gen
    if _gen is None:
        if not os.path.isdir(managed_dir()):
            raise SystemExit(
                f"Pasta do jogo nao encontrada: {DATA_DIR}\n"
                "Aponte a variavel de ambiente GK_DATA para o `*_Data` do jogo."
            )
        _gen = TypeTreeGenerator(UNITY_VERSION)
        _gen.load_local_dll_folder(managed_dir())
    return _gen


def tree(assembly: str, cls: str) -> TypeTreeNode:
    """Arvore de tipos de `cls`, pronta para `ObjectReader.read_typetree()`."""
    key = (assembly, cls)
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
        for n in generator().get_nodes(assembly, cls)
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
