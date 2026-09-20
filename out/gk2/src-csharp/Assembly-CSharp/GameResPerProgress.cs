using System;
using LazyBearTechnology;

[Serializable]
public class GameResPerProgress
{
	public int sucessfulProgressTick;

	public GameRes gameRes;

	public GameResPerProgress(int sucessfulProgressTick, GameRes gameRes)
	{
		this.sucessfulProgressTick = sucessfulProgressTick;
		this.gameRes = gameRes;
	}
}
