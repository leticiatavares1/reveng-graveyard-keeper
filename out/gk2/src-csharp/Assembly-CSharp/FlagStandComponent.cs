using LazyBearTechnology;
using UnityEngine;

public class FlagStandComponent : MonoBehaviour, IWgoCustomComponent<WCCD_FlagStandComponent>
{
	[SerializeField]
	private Wgo linkedFlag;

	[SerializeField]
	private SGuid flagSguid;

	[SerializeField]
	private FlagPlacementPoint flagPlacementPoint;

	private Wgo standWgo;

	public FlagPlacementPoint FlagPlacementPoint => flagPlacementPoint;

	public Wgo StandWgo
	{
		get
		{
			if (!standWgo)
			{
				standWgo = GetComponentInParent<Wgo>(includeInactive: true);
			}
			return standWgo;
		}
	}

	public WgoData FlagWgoData => MainGame.Instance.GameSave.worldData.GetWgoData(flagSguid);

	public WCCD_FlagStandComponent OnSave()
	{
		WCCD_FlagStandComponent wCCD_FlagStandComponent = new WCCD_FlagStandComponent();
		if (!linkedFlag)
		{
			return wCCD_FlagStandComponent;
		}
		wCCD_FlagStandComponent.flagSGuid = linkedFlag.Data.UniqueId;
		return wCCD_FlagStandComponent;
	}

	public void OnLoad(WCCD_FlagStandComponent data)
	{
		if (data != null)
		{
			if (!SGuid.IsNullOrEmpty(data.flagSGuid))
			{
				flagSguid = data.flagSGuid;
				linkedFlag = GameScene.GetWgoViewGlobal(flagSguid);
			}
			standWgo = null;
			OnEnableLogic();
		}
	}

	public void OnUnload()
	{
		linkedFlag = null;
		flagSguid = SGuid.Empty;
		standWgo = null;
	}

	public void AttachFlag(Wgo flag, bool bindCapturePoint = true)
	{
		linkedFlag = flag;
		flagSguid = flag.Data.UniqueId;
		if (!bindCapturePoint)
		{
			flag.GetComponentInChildren<AgentsGroupFlagController>()?.ClearCapturePointBinding();
		}
		else
		{
			TryBindFlagToCapturePoint();
		}
	}

	public void TryBindFlagToCapturePoint(FightingCapturePoint capturePoint = null)
	{
		Wgo wgo = (linkedFlag ? linkedFlag : GameScene.GetWgoViewGlobal(flagSguid));
		if (!wgo)
		{
			return;
		}
		AgentsGroupFlagController componentInChildren = wgo.GetComponentInChildren<AgentsGroupFlagController>();
		if ((bool)componentInChildren)
		{
			if ((object)capturePoint == null)
			{
				capturePoint = ResolveCapturePoint();
			}
			if ((bool)capturePoint)
			{
				capturePoint.BindAllyFlagController(componentInChildren);
				return;
			}
			componentInChildren.ClearCapturePointBinding();
			Debug.LogWarning($"FlagStand '{base.name}' at {(StandWgo ? StandWgo.Data.Position : base.transform.position)} is not linked to any sector FightingCapturePoint. " + "Place the stand inside a sector CP radius, or assign allyFlagStand on the capture point.", this);
		}
	}

	private FightingCapturePoint ResolveCapturePoint()
	{
		FightingCapturePoint componentInParent = GetComponentInParent<FightingCapturePoint>(includeInactive: true);
		if ((bool)componentInParent)
		{
			if (componentInParent.isBasePoint)
			{
				return null;
			}
			return componentInParent;
		}
		Vector3 worldPosition = (StandWgo ? StandWgo.Data.Position : base.transform.position);
		return LazySingleton<FightingGameController>.Instance?.CurrentLevel?.FindCapturePointForFlagStand(worldPosition);
	}

	public void DetachFlag()
	{
		if (!SGuid.IsNullOrEmpty(flagSguid))
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(flagSguid);
			if ((bool)wgoViewGlobal)
			{
				AgentsGroupFlagController componentInChildren = wgoViewGlobal.GetComponentInChildren<AgentsGroupFlagController>();
				if ((bool)componentInChildren)
				{
					componentInChildren.ClearCapturePointBinding();
					wgoViewGlobal.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
					LazySingleton<FightingGameController>.Instance.AllDynamicObjectsInZone.Add(wgoViewGlobal);
					LazySingleton<FightingGameController>.Instance.customFlagControllers.Add(componentInChildren);
				}
			}
		}
		linkedFlag = null;
		flagSguid = SGuid.Empty;
	}

	public void DestroyStandWgo()
	{
		if ((bool)StandWgo)
		{
			StandWgo.RemoveWithData();
		}
	}

	public void TryToSyncFlagPosition()
	{
		if (FlagWgoData != null && (bool)flagPlacementPoint)
		{
			FlagWgoData.Position = flagPlacementPoint.transform.position;
		}
	}

	private void OnEnable()
	{
		standWgo = null;
		OnEnableLogic();
	}

	private void OnEnableLogic()
	{
		TryToSyncFlagPosition();
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(flagSguid);
		if ((bool)wgoViewGlobal)
		{
			AgentsGroupFlagController.SetInteractionLocked(wgoViewGlobal, isLocked: true);
			if ((bool)StandWgo)
			{
				StandWgo.Data.GameResStr.Set("flag_stand_sguid", wgoViewGlobal.Data.UniqueId.ToString());
			}
		}
	}
}
