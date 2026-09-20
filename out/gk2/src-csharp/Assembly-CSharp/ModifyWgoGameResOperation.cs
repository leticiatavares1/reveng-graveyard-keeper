using System;
using System.Text;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ModifyWgoGameResOperation : SaveFixWgoOperation
{
	[SerializeField]
	private string wgoId;

	[SerializeField]
	private GameRes gameRes = new GameRes();

	[SerializeField]
	private SGuid wgoUniqueId;

	public SGuid WgoUniqueId => wgoUniqueId;

	public string WgoId => wgoId;

	public GameRes GameRes => gameRes;

	public override bool OccupiesUniqueId => false;

	public override int InfoCount => (gameRes?.List?.Count).GetValueOrDefault();

	protected override string SummaryBody
	{
		get
		{
			string text = ((!string.IsNullOrEmpty(wgoId)) ? wgoId : wgoUniqueId?.ToString());
			int valueOrDefault = (gameRes?.List?.Count).GetValueOrDefault();
			if (valueOrDefault == 0)
			{
				return text + "  (empty)";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(text);
			stringBuilder.Append("  ");
			int num = Mathf.Min(valueOrDefault, 3);
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				GameResAtom gameResAtom = gameRes.List[i];
				stringBuilder.Append(string.IsNullOrEmpty(gameResAtom.type) ? "?" : gameResAtom.type);
				stringBuilder.Append('=');
				stringBuilder.Append(gameResAtom.value);
			}
			if (valueOrDefault > num)
			{
				stringBuilder.Append(", …");
			}
			return stringBuilder.ToString();
		}
	}

	public override string Info()
	{
		return "wgo gameres";
	}

	public override bool TryGetTargetUniqueId(out SGuid uniqueId)
	{
		uniqueId = wgoUniqueId;
		return !SGuid.IsNullOrEmpty(wgoUniqueId);
	}

	public void SetTarget(SGuid uniqueId, string id)
	{
		wgoUniqueId = uniqueId;
		wgoId = id;
		if ((object)gameRes == null)
		{
			gameRes = new GameRes();
		}
	}

	public override void Apply(SaveFixContext ctx)
	{
		if (SGuid.IsNullOrEmpty(wgoUniqueId))
		{
			ctx.LogError(Summary + ": wgoUniqueId is empty");
			return;
		}
		if (!ctx.TryGetWgo(wgoUniqueId, out var wgoData, out var _))
		{
			ctx.LogWarning($"{Summary}: uniqueId [{wgoUniqueId}] id [{wgoId}] not found in save, skip");
			return;
		}
		if (gameRes == null || gameRes.List == null || gameRes.List.Count == 0)
		{
			ctx.LogWarning(Summary + ": GameRes is empty");
			return;
		}
		for (int i = 0; i < gameRes.List.Count; i++)
		{
			GameResAtom gameResAtom = gameRes.List[i];
			if (gameResAtom == null || string.IsNullOrEmpty(gameResAtom.type))
			{
				ctx.LogWarning($"{Summary}: empty atom type at index {i}, skip");
				continue;
			}
			float num = wgoData.GetGameRes(gameResAtom.type);
			wgoData.SetGameRes(gameResAtom.type, gameResAtom.value);
			ctx.Log($"{Summary}: set [{gameResAtom.type}] {num} -> {gameResAtom.value}");
		}
	}
}
