using System;
using UnityEngine;

[Serializable]
public class ModifyWgoCustomTagOperation : SaveFixWgoOperation
{
	[SerializeField]
	private string wgoId;

	[SerializeField]
	private string customTag;

	[SerializeField]
	private SGuid wgoUniqueId;

	public SGuid WgoUniqueId => wgoUniqueId;

	public string WgoId => wgoId;

	public string CustomTag => customTag;

	public override bool OccupiesUniqueId => false;

	protected override string SummaryBody => ((!string.IsNullOrEmpty(wgoId)) ? wgoId : wgoUniqueId?.ToString()) + "  tag=[" + customTag + "]";

	public override string Info()
	{
		return "wgo custom tag";
	}

	public override bool TryGetTargetUniqueId(out SGuid uniqueId)
	{
		uniqueId = wgoUniqueId;
		return !SGuid.IsNullOrEmpty(wgoUniqueId);
	}

	public void SetTarget(SGuid uniqueId, string id, string tag)
	{
		wgoUniqueId = uniqueId;
		wgoId = id;
		customTag = tag;
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
		string text = wgoData.CustomTag;
		wgoData.CustomTag = customTag;
		ctx.Log(Summary + ": customTag [" + text + "] -> [" + customTag + "]");
	}
}
