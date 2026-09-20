using System;

[Serializable]
public class CPA_SetWgoHidden : CapturePointAction
{
	public bool isHidden;

	public string wgoUniqueId;

	public override void Execute(LazyConsts.Fighting.TeamType teamType, FightingLevel level)
	{
		if (base.teamType == teamType && !string.IsNullOrEmpty(wgoUniqueId))
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(SGuid.Parse(wgoUniqueId));
			if (wgoData != null)
			{
				wgoData.IsHidden = false;
			}
		}
	}
}
