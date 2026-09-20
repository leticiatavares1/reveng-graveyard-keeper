"""Acesso aos arquivos serializados da Unity dos jogos."""
from __future__ import annotations

import os
import struct
from typing import Iterator

import UnityPy

from . import typetree
from .games import Game

#: Onde mora tudo que interessa: o balanceamento e os `lng_*`.
RESOURCES = "resources.assets"

# Cabecalho de um MonoBehaviour: m_GameObject(PPtr 12) + m_Enabled(1, alinhado a 4)
# + m_Script(PPtr 12); o `m_Name` e a primeira string logo depois.
_NAME_OFFSET = 12 + 4 + 12


def load(game: Game, filename: str = RESOURCES):
    path = os.path.join(game.env_data_dir(), filename)
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
    """(nome, objeto) de cada MonoBehaviour cujo nome passa em `predicate`.

    Sempre por NOME: o `path_id` nao e estavel entre builds.
    """
    for obj in env.objects:
        if obj.type.name != "MonoBehaviour":
            continue
        name = raw_name(obj.get_raw_data())
        if name is None:
            continue
        if predicate is None or predicate(name):
            yield name, obj


def read_balance(game: Game) -> dict:
    """O ScriptableObject de balanceamento inteiro, como dict."""
    env = load(game)
    for _, obj in monobehaviours(env, lambda n: n == game.balance_asset):
        return obj.read_typetree(typetree.tree(game, *game.balance_type))
    raise SystemExit(
        f"MonoBehaviour {game.balance_asset!r} nao encontrado em {RESOURCES} de {game.nome}"
    )


def read_locales(game: Game) -> Iterator[dict]:
    """Cada localizacao, normalizada para {id, strings, aliases}."""
    env = load(game)
    f = game.locale_fields
    for _, obj in monobehaviours(env, lambda n: n.startswith("lng_")):
        d = obj.read_typetree(typetree.tree(game, *game.locale_type))
        yield {
            "id": d["id"],
            "strings": dict(zip(d[f["ids"]], d[f["txts"]])),
            "aliases": dict(zip(d[f["aliases1"]], d[f["aliases2"]])),
        }
