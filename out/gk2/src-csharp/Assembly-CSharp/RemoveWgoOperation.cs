using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class RemoveWgoOperation : SaveFixWgoOperation
{
	[SerializeField]
	private List<string> debugWgoIds = new List<string>();

	[SerializeField]
	private List<SGuid> wgoUniqueIds = new List<SGuid>();

	public IReadOnlyList<SGuid> WgoUniqueIds => wgoUniqueIds;

	public IReadOnlyList<string> DebugWgoIds => debugWgoIds;

	public override int InfoCount => wgoUniqueIds?.Count ?? 0;

	protected override string SummaryBody
	{
		get
		{
			int num = wgoUniqueIds?.Count ?? 0;
			if (num == 0)
			{
				return "(empty)";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(num);
			stringBuilder.Append((num == 1) ? " object" : " objects");
			if (debugWgoIds != null && debugWgoIds.Count > 0)
			{
				stringBuilder.Append("  (");
				int num2 = Mathf.Min(debugWgoIds.Count, 3);
				for (int i = 0; i < num2; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(debugWgoIds[i]);
				}
				if (debugWgoIds.Count > num2)
				{
					stringBuilder.Append(", …");
				}
				stringBuilder.Append(')');
			}
			return stringBuilder.ToString();
		}
	}

	public override string Info()
	{
		return "remove wgo";
	}

	public override bool ContainsUniqueId(SGuid uniqueId)
	{
		return IndexOf(uniqueId) >= 0;
	}

	public void AddTarget(SGuid uniqueId, string wgoId)
	{
		if (!SGuid.IsNullOrEmpty(uniqueId) && !ContainsUniqueId(uniqueId))
		{
			if (wgoUniqueIds == null)
			{
				wgoUniqueIds = new List<SGuid>();
			}
			if (debugWgoIds == null)
			{
				debugWgoIds = new List<string>();
			}
			wgoUniqueIds.Add(uniqueId);
			debugWgoIds.Add(string.IsNullOrEmpty(wgoId) ? uniqueId.ToString() : wgoId);
		}
	}

	public override void Apply(SaveFixContext ctx)
	{
		if (wgoUniqueIds == null || wgoUniqueIds.Count == 0)
		{
			ctx.LogWarning(Summary + ": list is empty");
			return;
		}
		for (int i = 0; i < wgoUniqueIds.Count; i++)
		{
			SGuid sGuid = wgoUniqueIds[i];
			string text = ((i < debugWgoIds.Count) ? debugWgoIds[i] : sGuid?.ToString());
			if (SGuid.IsNullOrEmpty(sGuid))
			{
				ctx.LogWarning($"{Summary}: empty uniqueId at index {i}, skip");
				continue;
			}
			if (!ctx.RemoveWgoData(sGuid))
			{
				ctx.LogWarning($"{Summary}: uniqueId [{sGuid}] ({text}) not found in save, skip");
				continue;
			}
			RemoveDelayedEventReference(ctx, sGuid);
			ctx.WarnAboutDanglingReferences(sGuid, text);
			ctx.Log($"{Summary}: removed [{text}] [{sGuid}]");
		}
	}

	private static void RemoveDelayedEventReference(SaveFixContext ctx, SGuid uniqueId)
	{
		List<SGuid> list = ctx.GameSave.wgoDelayedEventSystemData?.wgoUniqueIds;
		if (list == null)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == uniqueId)
			{
				list.RemoveAt(num);
			}
		}
	}

	private int IndexOf(SGuid uniqueId)
	{
		if (wgoUniqueIds == null || SGuid.IsNullOrEmpty(uniqueId))
		{
			return -1;
		}
		for (int i = 0; i < wgoUniqueIds.Count; i++)
		{
			if (wgoUniqueIds[i] == uniqueId)
			{
				return i;
			}
		}
		return -1;
	}
}
