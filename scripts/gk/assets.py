"""Acesso aos arquivos serializados da Unity do Graveyard Keeper."""
from __future__ import annotations

import os
import struct
from typing import Iterator

import UnityPy

from . import typetree

#: Onde mora tudo que interessa: `game_data` (o balanceamento) e os `lng_*`.
RESOURCES = "resources.assets"

# Cabecalho de um MonoBehaviour: m_GameObject(PPtr 12) + m_Enabled(1, alinhado a 4)
# + m_Script(PPtr 12); o `m_Name` e a primeira string logo depois.
_NAME_OFFSET = 12 + 4 + 12


def load(filename: str = RESOURCES):
    path = os.path.join(typetree.DATA_DIR, filename)
    if not os.path.isfile(path):
        raise SystemExit(f"Arquivo nao encontrado: {path}")
    return UnityPy.load(path)


def raw_name(raw: bytes) -> str | None:
    """Le o `m_Name` direto dos bytes, sem precisar de TypeTree."""
    if len(raw) < _NAME_OFFSET + 4:
        return None
    size = struct.unpack_from("<i", raw, _NAME_OFFSET)[0]
    if not 0 < size < 256 or _NAME_OFFSET + 4 + size > len(raw):
        return None
    try:
        return raw[_NAME_OFFSET + 4 : _NAME_OFFSET + 4 + size].decode("utf8")
    except UnicodeDecodeError:
        return None


def monobehaviours(env, predicate=None) -> Iterator[tuple[str, object]]:
    """(nome, objeto) de cada MonoBehaviour cujo nome passa em `predicate`."""
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        name = raw_name(obj.get_raw_data())
        if name is None:
            continue
        if predicate is None or predicate(name):
            yield name, obj


def read(obj, assembly: str, cls: str) -> dict:
    return obj.read_typetree(typetree.tree(assembly, cls))
