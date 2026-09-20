using System;
using UnityEngine;

[Serializable]
public class RefreshWgoDataOperation : SaveFixWgoOperation
{
	[SerializeField]
	private string wgoId;

	[SerializeField]
	private string customTag;

	[SerializeField]
	private bool isHidden;

	[SerializeField]
	private SGuid wgoUniqueId;

	public SGuid WgoUniqueId => wgoUniqueId;

	public string WgoId => wgoId;

	public string CustomTag => customTag;

	public bool IsHidden => isHidden;

	protected override string SummaryBody
	{
		get
		{
			string arg = ((!string.IsNullOrEmpty(wgoId)) ? wgoId : wgoUniqueId?.ToString());
			return $"{arg}  tag=[{customTag}]  hidden={isHidden}";
		}
	}

	public override string Info()
	{
		return "refresh wgo";
	}

	public override bool TryGetTargetUniqueId(out SGuid uniqueId)
	{
		uniqueId = wgoUniqueId;
		return !SGuid.IsNullOrEmpty(wgoUniqueId);
	}

	public void SetTarget(SGuid uniqueId, string id, string tag, bool hidden)
	{
		wgoUniqueId = uniqueId;
		wgoId = id;
		customTag = tag;
		isHidden = hidden;
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
		wgoData.CustomTag = customTag;
		wgoData.IsHidden = isHidden;
		ctx.Log(Summary + ": updated customTag/isHidden");
	}
}
