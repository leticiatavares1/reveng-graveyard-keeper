using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class AddWgoFromContentOperation : SaveFixWgoOperation
{
	private const float OccupiedGraveModuleRadius = 0.05f;

	[SerializeField]
	private string debugLabel;

	[SerializeField]
	private AssetReferenceGameObject contentDataRef;

	[SerializeField]
	private SGuid wgoUniqueId;

	public AssetReferenceGameObject ContentDataRef => contentDataRef;

	public SGuid WgoUniqueId => wgoUniqueId;

	public string DebugLabel => debugLabel;

	protected override string SummaryBody
	{
		get
		{
			if (!string.IsNullOrEmpty(debugLabel))
			{
				return debugLabel;
			}
			return wgoUniqueId?.ToString();
		}
	}

	public override string Info()
	{
		return "add wgo";
	}

	public override bool TryGetTargetUniqueId(out SGuid uniqueId)
	{
		uniqueId = wgoUniqueId;
		return !SGuid.IsNullOrEmpty(wgoUniqueId);
	}

	public void SetTarget(AssetReferenceGameObject contentRef, SGuid uniqueId, string label)
	{
		contentDataRef = contentRef;
		wgoUniqueId = uniqueId;
		debugLabel = label;
	}

	public override void Apply(SaveFixContext ctx)
	{
		if (contentDataRef == null || string.IsNullOrEmpty(contentDataRef.AssetGUID))
		{
			ctx.LogError(Summary + ": contentDataRef is empty");
		}
		else if (SGuid.IsNullOrEmpty(wgoUniqueId))
		{
			ctx.LogError(Summary + ": wgoUniqueId is empty");
		}
		else if (ctx.HasWgo(wgoUniqueId))
		{
			ctx.Log(Summary + ": already present in save, skip");
		}
		else
		{
			if (!ctx.TryGetContentPart(contentDataRef.AssetGUID, out var part, out var sceneData, out var config))
			{
				return;
			}
			WgoData wgoData = FindWgo(part, wgoUniqueId);
			if (wgoData == null)
			{
				ctx.LogError($"{Summary}: uniqueId [{wgoUniqueId}] not found in content [{part.name}]");
				return;
			}
			WgoData wgoData2 = wgoData.CreateDataFromMe(config.sceneGlobalPosition, sceneData.id, copySGuid: true);
			if (wgoData.startReses != null)
			{
				foreach (StartReses.StartItemData startItem in wgoData.startReses.startItems)
				{
					wgoData2.Inventory.AddItemToInventory(new Item(startItem.id, startItem.count));
				}
				wgoData2.SetGameRes(wgoData.startReses.startGameRes);
				wgoData2.GameResStr.Set(wgoData.startReses.startGameResStr);
			}
			if (IsGraveyardModule(wgoData2.id) && ctx.TryFindWgoByGroupNear("graveyard_modules", wgoData2.Position, 0.05f, out var wgoData3, out var _))
			{
				ctx.Log(string.Format("{0}: skip, [{1}] [{2}] of group [{3}] within {4} of {5}", Summary, wgoData3.id, wgoData3.UniqueId, "graveyard_modules", 0.05f, wgoData2.Position));
			}
			else
			{
				ctx.AddWgoData(sceneData, wgoData2);
				ctx.Log(Summary + ": added to scene [" + sceneData.id + "]");
				TryRegisterPrebuiltWorldZoneQualitySlot(ctx, part, wgoData2);
			}
		}
	}

	private void TryRegisterPrebuiltWorldZoneQualitySlot(SaveFixContext ctx, SceneWgoContentPart contentPart, WgoData wgoData)
	{
		if (wgoData != null && WorldZoneBakedData.TryGetPrebuiltWgoParams(contentPart.WorldZones, wgoData.UniqueId, out var prebuiltParams) && ctx.GameSave.worldData.TryExecutePrebuiltWgoAfterBuildingExpressions(wgoData, prebuiltParams))
		{
			ctx.Log(Summary + ": registered prebuilt world zone quality slot");
		}
	}

	private static WgoData FindWgo(SceneWgoContentPart contentPart, SGuid uniqueId)
	{
		if (contentPart?.Wgos == null)
		{
			return null;
		}
		foreach (WgoData wgo in contentPart.Wgos)
		{
			if (wgo != null && wgo.UniqueId == uniqueId)
			{
				return wgo;
			}
		}
		return null;
	}

	private static bool IsGraveyardModule(string id)
	{
		if (GameBalance.Me != null)
		{
			return GameBalance.Me.HasWgoIdByGroup("graveyard_modules", id);
		}
		return false;
	}
}
