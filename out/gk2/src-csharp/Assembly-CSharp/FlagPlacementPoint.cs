using LazyBearTechnology;
using UnityEngine;

public class FlagPlacementPoint : MonoBehaviour
{
	private const string FLAG_STAND_ONE_TIME_ID = "flag_stand_one_time";

	private WgoData parentWgoData;

	private Wgo flagWgo;

	public Transform flagStandSpawnTransform;

	private Vector3 flagStandSpawnPosition = Vector3.zero;

	public Vector3 FlagStandSpawnPosition => flagStandSpawnPosition;

	public Wgo FlagWgo
	{
		get
		{
			return flagWgo;
		}
		set
		{
			flagWgo = value;
		}
	}

	public void Init(WgoData wgoData)
	{
		parentWgoData = wgoData;
		if (parentWgoData == null)
		{
			Debug.Log("FlagPlacementPoint must be a child of Wgo", base.gameObject);
			return;
		}
		if (!flagStandSpawnTransform)
		{
			Debug.Log("[FlagPlacementPoint]: FlagStandSpawnTransform is not set, using transform of FlagPlacementPoint", base.gameObject);
			flagStandSpawnTransform = base.transform;
		}
		flagStandSpawnPosition = flagStandSpawnTransform.position;
		parentWgoData.OnRemoveFromData += TrySpawnPlacementPointAndAttachFlag;
	}

	public void DeInit()
	{
		if (parentWgoData != null && !flagStandSpawnPosition.magnitude.EqualsTo(0f, 0.0001f))
		{
			parentWgoData.OnRemoveFromData -= TrySpawnPlacementPointAndAttachFlag;
		}
	}

	private void TrySpawnPlacementPointAndAttachFlag()
	{
		Debug.Log($"TrySpawnPlacementPointAndAttachFlag: ParentWgo: {parentWgoData}, FlagWgo: {flagWgo}");
		if (parentWgoData == null)
		{
			return;
		}
		FightingGameController instance = LazySingleton<FightingGameController>.Instance;
		if ((object)instance != null && instance.IsClearingFightEnvironment)
		{
			parentWgoData.OnRemoveFromData -= TrySpawnPlacementPointAndAttachFlag;
			return;
		}
		if ((bool)flagWgo && !flagStandSpawnPosition.magnitude.EqualsTo(0f, 0.0001f))
		{
			WgoData wgoData = new WgoData("flag_stand_one_time", flagStandSpawnPosition, FlagWgo.Data.WorldId);
			MainGame.Instance.GameSave.worldData.AddWgoData(wgoData);
			LazySingleton<FightingGameController>.Instance.AddTemporaryWgoData(wgoData);
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
			if ((bool)wgoViewGlobal)
			{
				AgentsGroupFlagController componentInChildren = flagWgo.GetComponentInChildren<AgentsGroupFlagController>();
				if ((bool)componentInChildren)
				{
					componentInChildren.IsSetAtPoint = false;
				}
				FlagStandComponent componentInChildren2 = wgoViewGlobal.GetComponentInChildren<FlagStandComponent>();
				if ((bool)componentInChildren2)
				{
					componentInChildren2.AttachFlag(flagWgo);
					LazySingleton<FightingGameController>.Instance.FlagStandComponents.Add(componentInChildren2);
				}
				wgoViewGlobal.Data.GameResStr.Set("flag_stand_sguid", flagWgo.Data.UniqueId.ToString());
			}
		}
		parentWgoData.OnRemoveFromData -= TrySpawnPlacementPointAndAttachFlag;
	}
}
