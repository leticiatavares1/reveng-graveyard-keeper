"""Registro dos jogos suportados.

Os dois Graveyard Keeper compartilham a arquitetura -- Unity/Mono, todo o
balanceamento num ScriptableObject unico, localizacao em ScriptableObjects
irmaos -- mas mudam de nome em quase tudo: a classe, o assembly, o nome do
asset e os campos da localizacao. Tudo que difere mora aqui; o resto do
pipeline e o mesmo para os dois.
"""
from __future__ import annotations

import os
from dataclasses import dataclass, field

STEAM = os.path.expanduser("~/.local/share/Steam/steamapps/common")


@dataclass(frozen=True)
class Game:
    id: str
    nome: str
    steam_appid: int
    #: Pasta `*_Data` da instalacao.
    data_dir: str
    #: Versao da Unity do build (o `inventario.sh` imprime a do disco).
    unity: str
    #: Assemblies proprios do estudio, que o `decompila.sh` processa.
    assemblies: tuple[str, ...]
    #: Nome do MonoBehaviour do balanceamento dentro de resources.assets.
    balance_asset: str
    #: (assembly, classe) do balanceamento.
    balance_type: tuple[str, str]
    #: (assembly, classe) da localizacao.
    locale_type: tuple[str, str]
    #: Nomes dos campos serializados da localizacao, que mudaram entre os jogos.
    locale_fields: dict[str, str]
    #: Sufixo de chave de descricao no locale (`<id>_d` nos dois jogos).
    desc_suffix: str = "_d"
    #: Observacao que entra na saida, quando o build nao e o jogo completo.
    aviso: str = ""

    @property
    def out(self) -> str:
        return os.path.join("out", self.id)

    def env_data_dir(self) -> str:
        """Permite apontar para outra instalacao com GK1_DATA / GK2_DATA / GK_DATA."""
        return (os.environ.get(f"{self.id.upper()}_DATA")
                or os.environ.get("GK_DATA")
                or self.data_dir)


GK1 = Game(
    id="gk1",
    nome="Graveyard Keeper",
    steam_appid=599140,
    data_dir=f"{STEAM}/Graveyard Keeper/Graveyard Keeper_Data",
    unity="2020.3.17f1",
    assemblies=("Assembly-CSharp", "Assembly-CSharp-firstpass"),
    balance_asset="game_data",
    balance_type=("Assembly-CSharp.dll", "GameBalance"),
    locale_type=("Assembly-CSharp-firstpass.dll", "GJL"),
    locale_fields={"ids": "txt_ids", "txts": "txts",
                   "aliases1": "aliases_1", "aliases2": "aliases_2"},
)

GK2 = Game(
    id="gk2",
    nome="Graveyard Keeper 2 (demo)",
    steam_appid=5075680,
    data_dir=f"{STEAM}/Graveyard Keeper 2 Demo/GraveyardKeeper2Demo_Data",
    unity="6000.3.9f1",
    assemblies=("Assembly-CSharp", "Assembly-CSharp-firstpass", "LazyBearTechnology"),
    balance_asset="GameBalance",
    balance_type=("Assembly-CSharp.dll", "GameBalance"),
    # A classe da localizacao vive num namespace: sem o nome completo o gerador
    # devolve "Object reference not set to an instance of an object".
    locale_type=("LazyBearTechnology.dll", "LazyBearTechnology.LL"),
    locale_fields={"ids": "txtIds", "txts": "txts",
                   "aliases1": "aliases1", "aliases2": "aliases2"},
    aviso=("Build de DEMO: o balanceamento vem completo no arquivo, mas parte do "
           "conteudo esta marcada como indisponivel na demo (ver isAvailableInDemo "
           "em techDefs) e pode mudar no jogo final."),
)

GAMES = {g.id: g for g in (GK1, GK2)}


def get(game_id: str) -> Game:
    try:
        return GAMES[game_id]
    except KeyError:
        raise SystemExit(f"Jogo desconhecido: {game_id!r}. Use um de: {', '.join(GAMES)}")
