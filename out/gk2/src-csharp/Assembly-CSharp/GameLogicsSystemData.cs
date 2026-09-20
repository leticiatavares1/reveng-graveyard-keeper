using System;
using System.Collections.Generic;

[Serializable]
public class GameLogicsSystemData
{
	public List<GameLogicData> gameLogics = new List<GameLogicData>();

	public void PrepareForGame()
	{
		gameLogics.RemoveAll((GameLogicData x) => !GameBalance.Me.gameLogicsDefs.Contains(x.Definition));
		foreach (GameLogicDef def in GameBalance.Me.gameLogicsDefs)
		{
			if (!gameLogics.Exists((GameLogicData x) => x.Definition == def))
			{
				gameLogics.Add(new GameLogicData(def.id));
			}
		}
		gameLogics.ForEach(delegate(GameLogicData data)
		{
			data.Init();
		});
	}

	public void PrepareForNewGame()
	{
		gameLogics.Clear();
		foreach (GameLogicDef def in GameBalance.Me.gameLogicsDefs)
		{
			if (!gameLogics.Exists((GameLogicData x) => x.Definition == def))
			{
				gameLogics.Add(new GameLogicData(def.id));
			}
		}
		gameLogics.ForEach(delegate(GameLogicData data)
		{
			data.Init();
		});
	}
}
