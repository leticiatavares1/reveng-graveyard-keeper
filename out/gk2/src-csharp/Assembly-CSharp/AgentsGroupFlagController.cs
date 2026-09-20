using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;

public class AgentsGroupFlagController : MonoBehaviour, IWgoCustomComponent<WCCD_AgentGroupFlagController>
{
	private const string IS_SET_ON_POINT_KEY = "is_set_on_point";

	private const string ATTACHED_SGUID_KEY = "attached_sguid";

	private Wgo flagWgo;

	[SerializeField]
	private AgentsGroupBehaviourController agentsController;

	[SerializeField]
	[Space]
	private List<Wgo> preSetWgos = new List<Wgo>();

	[SerializeField]
	private List<SGuid> presetWgoUIds = new List<SGuid>();

	private bool isInited;

	public SGuid AttachedSGuid
	{
		get
		{
			return SGuid.Parse(flagWgo.Data.GameResStr.Get("attached_sguid", SGuid.Empty.ToString()));
		}
		set
		{
			flagWgo.Data.GameResStr.Set("attached_sguid", (!SGuid.IsNullOrEmpty(value)) ? value.ToString() : SGuid.Empty.ToString());
		}
	}

	public bool IsSetAtPoint
	{
		get
		{
			Wgo wgo = flagWgo;
			if ((object)wgo == null)
			{
				return false;
			}
			return wgo.Data.GetGameResInt("is_set_on_point") == 1;
		}
		set
		{
			flagWgo?.Data.SetGameRes("is_set_on_point", value ? 1 : 0);
		}
	}

	public Wgo FlagWgo
	{
		get
		{
			if (flagWgo != null)
			{
				return flagWgo;
			}
			flagWgo = GetComponent<WgoPart>()?.Wgo;
			if (!flagWgo)
			{
				Debug.LogError("Flag Wgo is null", this);
				return null;
			}
			return flagWgo;
		}
	}

	public AgentsGroupBehaviourController AgentsController => agentsController;

	public FightingCapturePoint CapturePoint { get; private set; }

	public static bool IsInteractionLocked(Wgo flagWgo)
	{
		return flagWgo.Data.GetGameResInt("is_interaction_locked") == 1;
	}

	public static void SetInteractionLocked(Wgo flagWgo, bool isLocked)
	{
		flagWgo.Data.SetGameRes("is_interaction_locked", isLocked ? 1 : 0);
	}

	public WCCD_AgentGroupFlagController OnSave()
	{
		return new WCCD_AgentGroupFlagController
		{
			presetWgoUIds = preSetWgos.Select((Wgo wgo) => wgo.Data.UniqueId).ToList()
		};
	}

	public void OnLoad(WCCD_AgentGroupFlagController data)
	{
		presetWgoUIds = data.presetWgoUIds;
	}

	public void OnUnload()
	{
		presetWgoUIds.Clear();
	}

	public void Init()
	{
		if (isInited)
		{
			return;
		}
		isInited = true;
		base.enabled = false;
		preSetWgos = presetWgoUIds.Select(GameScene.GetWgoViewGlobal).ToList();
		flagWgo = GetComponent<WgoPart>()?.Wgo;
		if (!flagWgo)
		{
			Debug.LogError("Flag Wgo is null", this);
			isInited = false;
			return;
		}
		agentsController.Init();
		LazySingleton<FightingGameController>.Instance.RegisterTargetNonPersistent(flagWgo);
		MainGame.PlayerController.CurrentGameScene.GameSceneData.OnWgoDataPreRemove += HandleZombieRemoved;
		for (int i = 0; i < preSetWgos.Count; i++)
		{
			Wgo wgo = preSetWgos[i];
			if ((bool)wgo)
			{
				wgo.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
				FightingAgent fightingAgent = agentsController.AddWgoAsAgent(wgo);
				fightingAgent.FlagController = this;
				fightingAgent.AssignWeaponFromInventory();
			}
		}
	}

	public void DeInit()
	{
		isInited = false;
		ClearCapturePointBinding();
		if ((bool)agentsController)
		{
			for (int num = agentsController.Agents.Count - 1; num >= 0; num--)
			{
				FightingAgent fightingAgent = agentsController.Agents[num];
				if ((bool)fightingAgent)
				{
					fightingAgent.FlagController = null;
				}
				agentsController.RemoveAgent(fightingAgent);
			}
			agentsController.DeInit();
		}
		if (MainGame.PlayerController.TryGetCurrentGameScene(out var gameScene) && gameScene.GameSceneData != null)
		{
			gameScene.GameSceneData.OnWgoDataPreRemove -= HandleZombieRemoved;
		}
	}

	public void SetEnabled(bool isEnabled)
	{
		base.enabled = isEnabled;
	}

	public void BindCapturePoint(FightingCapturePoint capturePoint)
	{
		CapturePoint = capturePoint;
		if (!(agentsController == null))
		{
			FightingLine fightingLine = ((!(capturePoint != null)) ? null : capturePoint.Sector?.fightingLine);
			agentsController.FightingLine = fightingLine;
		}
	}

	public void ClearCapturePointBinding()
	{
		BindCapturePoint(null);
	}

	private void OnDestroy()
	{
		GameScene currentGameScene = MainGame.PlayerController.CurrentGameScene;
		if ((bool)currentGameScene)
		{
			currentGameScene.GameSceneData.OnWgoDataPreRemove -= HandleZombieRemoved;
		}
		if (flagWgo != null)
		{
			LazySingleton<FightingGameController>.Instance.UnregisterTarget(flagWgo);
		}
	}

	public void CustomUpdate(float deltaTime)
	{
		if (base.enabled)
		{
			agentsController.CustomUpdate(deltaTime);
		}
	}

	private void HandleZombieRemoved(WgoData wgoData)
	{
		agentsController.RemoveAgent(wgoData.UniqueId);
	}
}
