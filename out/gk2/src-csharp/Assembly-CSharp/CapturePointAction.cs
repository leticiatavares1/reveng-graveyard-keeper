using System;

[Serializable]
public abstract class CapturePointAction
{
	public LazyConsts.Fighting.TeamType teamType;

	public virtual void Execute(LazyConsts.Fighting.TeamType teamType, FightingCapturePoint point)
	{
	}

	public virtual void Execute(LazyConsts.Fighting.TeamType teamType, FightingLevel level)
	{
	}
}
