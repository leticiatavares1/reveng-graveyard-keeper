using System;
using UnityEngine;

[Serializable]
public class MoveWgoOperation : SaveFixWgoOperation
{
	private const float DefaultSearchRadius = 0.01f;

	[SerializeField]
	private string wgoId;

	[SerializeField]
	private Vector3 fromPosition;

	[SerializeField]
	private bool hasFromPosition;

	[SerializeField]
	private Vector3 newPosition;

	[SerializeField]
	[Tooltip("Used when uniqueId is missing. Search within this XZ radius of fromPosition.")]
	private float searchRadius = 0.01f;

	[SerializeField]
	private SGuid wgoUniqueId;

	public SGuid WgoUniqueId => wgoUniqueId;

	public string WgoId => wgoId;

	public Vector3 FromPosition => fromPosition;

	public bool HasFromPosition => hasFromPosition;

	public Vector3 NewPosition => newPosition;

	public float SearchRadius => searchRadius;

	public string DebugWgoId => wgoId;

	protected override string SummaryBody
	{
		get
		{
			string arg = ((!string.IsNullOrEmpty(wgoId)) ? wgoId : wgoUniqueId?.ToString());
			if (hasFromPosition)
			{
				return $"{arg}  {fromPosition}  ->  {newPosition}";
			}
			return $"{arg}  ->  {newPosition}";
		}
	}

	public override string Info()
	{
		return "move wgo";
	}

	public override bool TryGetTargetUniqueId(out SGuid uniqueId)
	{
		uniqueId = wgoUniqueId;
		return !SGuid.IsNullOrEmpty(wgoUniqueId);
	}

	public void SetTarget(SGuid uniqueId, Vector3 toPosition, string id)
	{
		wgoUniqueId = uniqueId;
		newPosition = toPosition;
		wgoId = id;
	}

	public void SetFromPosition(Vector3 position)
	{
		fromPosition = position;
		hasFromPosition = true;
	}

	public override void Apply(SaveFixContext ctx)
	{
		if (!TryMoveByUniqueId(ctx) && !TryMoveRelatedNearFromPosition(ctx))
		{
			ctx.LogWarning($"{Summary}: uniqueId [{wgoUniqueId}] id [{wgoId}] not found in save, skip");
		}
	}

	private bool TryMoveByUniqueId(SaveFixContext ctx)
	{
		if (SGuid.IsNullOrEmpty(wgoUniqueId))
		{
			return false;
		}
		if (!ctx.MoveWgo(wgoUniqueId, newPosition))
		{
			return false;
		}
		ctx.Log(Summary + ": moved by uniqueId");
		return true;
	}

	private bool TryMoveRelatedNearFromPosition(SaveFixContext ctx)
	{
		if (!hasFromPosition || string.IsNullOrEmpty(wgoId))
		{
			return false;
		}
		WgoData wgoData;
		GameSceneData sceneData;
		if (IsTreeId(wgoId))
		{
			if (!ctx.TryFindWgoByGroupNear("trees", fromPosition, searchRadius, out wgoData, out sceneData))
			{
				return false;
			}
		}
		else if (!ctx.TryFindWgoByIdNear(wgoId, fromPosition, searchRadius, out wgoData, out sceneData))
		{
			return false;
		}
		if (!ctx.MoveWgo(wgoData.UniqueId, newPosition))
		{
			return false;
		}
		ctx.Log($"{Summary}: moved related [{wgoData.id}] [{wgoData.UniqueId}] near {fromPosition}");
		return true;
	}

	private static bool IsTreeId(string id)
	{
		if (GameBalance.Me != null)
		{
			return GameBalance.Me.HasWgoIdByGroup("trees", id);
		}
		return false;
	}
}
