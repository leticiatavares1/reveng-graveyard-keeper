using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DevTestAgentsController : LazySingleton<DevTestAgentsController>
{
	private const string DEFAULT_AI_ADDRESSABLE_PATH = "Dev Test Player Attack AI.asset";

	[SerializeField]
	private DevTestPlayerAttackAI defaultTestAI;

	private AgentsGroupBehaviourController agentsController;

	private List<FightingAgent> testAgents = new List<FightingAgent>();

	private List<Wgo> testWgos = new List<Wgo>();

	private bool aiLoaded;

	private GameScene GameScene => MainGame.PlayerController.CurrentGameScene;

	public IReadOnlyList<FightingAgent> TestAgents => testAgents;

	public int AgentsCount => testAgents.Count;

	protected override void Awake()
	{
		base.Awake();
		InitializeController();
		LoadDefaultAI();
	}

	private void InitializeController()
	{
		if (agentsController == null)
		{
			GameObject gameObject = new GameObject("DevTestAgentsController_GroupController");
			gameObject.transform.SetParent(base.transform);
			agentsController = gameObject.AddComponent<AgentsGroupBehaviourController>();
			agentsController.TargetTeam = LazyConsts.Fighting.TeamType.Player;
		}
	}

	private void LoadDefaultAI()
	{
		if (!aiLoaded && !(defaultTestAI != null))
		{
			defaultTestAI = Addressables.LoadAssetAsync<DevTestPlayerAttackAI>("Dev Test Player Attack AI.asset").WaitForCompletion();
			aiLoaded = true;
			if (defaultTestAI == null)
			{
				Debug.LogWarning("[DevTestAgentsController] Failed to load default AI from: Dev Test Player Attack AI.asset");
			}
		}
	}

	public FightingAgent SpawnTestAgent(string fighterDefId, Vector3 position, AgentAI ai = null)
	{
		if (string.IsNullOrEmpty(fighterDefId))
		{
			Debug.LogError("[DevTestAgentsController] Cannot spawn agent: fighterDefId is null or empty");
			return null;
		}
		string currentGameSceneId = MainGame.PlayerData.currentGameSceneId;
		WgoData data = new WgoData(fighterDefId, position, currentGameSceneId);
		Wgo wgo = GameScene.AddWgoData(data);
		if (wgo == null)
		{
			Debug.LogError("[DevTestAgentsController] Failed to spawn WGO with id: " + fighterDefId);
			return null;
		}
		FightingAgent fightingAgent = agentsController.AddWgoAsAgent(wgo);
		fightingAgent.SetGraphMask(LazyConsts.Navigation.Graph.RuinedTemple);
		AgentAI agentAI = ((ai != null) ? ai : defaultTestAI);
		if (agentAI != null)
		{
			fightingAgent.SetAgentAI(agentAI);
		}
		fightingAgent.SetPosition(position);
		testAgents.Add(fightingAgent);
		testWgos.Add(wgo);
		wgo.IsActiveCombatant = true;
		Debug.Log($"[DevTestAgentsController] Spawned test agent: {fighterDefId} at {position}");
		return fightingAgent;
	}

	public void ClearAllTestAgents()
	{
		for (int num = testAgents.Count - 1; num >= 0; num--)
		{
			RemoveTestAgent(num);
		}
		testAgents.Clear();
		testWgos.Clear();
		Debug.Log("[DevTestAgentsController] Cleared all test agents");
	}

	public void RemoveTestAgent(int index)
	{
		if (index < 0 || index >= testAgents.Count)
		{
			return;
		}
		FightingAgent fightingAgent = testAgents[index];
		Wgo wgo = testWgos[index];
		if (fightingAgent != null)
		{
			fightingAgent.ClearCommand();
			if (agentsController != null)
			{
				agentsController.RemoveAgent(wgo.Data.UniqueId);
			}
		}
		if (wgo != null)
		{
			SGuid uniqueId = wgo.Data.UniqueId;
			MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(uniqueId);
			Object.Destroy(wgo.gameObject);
		}
		testAgents.RemoveAt(index);
		testWgos.RemoveAt(index);
	}

	public void RemoveTestAgent(FightingAgent agent)
	{
		int num = testAgents.IndexOf(agent);
		if (num >= 0)
		{
			RemoveTestAgent(num);
		}
	}

	private void Update()
	{
		if (testAgents.Count != 0 && !(agentsController == null))
		{
			float deltaTime = Time.deltaTime;
			agentsController.CustomUpdate(deltaTime);
		}
	}
}
