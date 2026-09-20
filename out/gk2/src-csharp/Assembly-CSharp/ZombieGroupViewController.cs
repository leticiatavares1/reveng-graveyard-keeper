using System.Collections.Generic;
using LazyBearTechnology;
using Pathfinding.RVO;
using UnityEngine;

[RequireComponent(typeof(RVOSimulator))]
public class ZombieGroupViewController : LazySingleton<ZombieGroupViewController>
{
	private const float ZOMBIE_RADIUS = 0.22f;

	private const string DEV_ZOMBIE_FIGHTERS_TEST_SCENE = "Dev_ZombieFigtersTest";

	private const float PLAYER_SPAWN_OFFSET = 4f;

	[Space]
	[SerializeField]
	private string zombieWgoId = "zmb_wild_01";

	private GameScene GameScene => MainGame.PlayerController.CurrentGameScene;

	private FightingGameController FgController => LazySingleton<FightingGameController>.Instance;

	private AgentsGroupBehaviourController AgentsController => FgController.BaseDefenseAgentsController;

	public void SpawnZombiesOnGrid(int amount, Vector3 gridRightDownPos)
	{
		string worldId = ResolveSpawnGameSceneId();
		Vector3 vector = ResolveGridOrigin(gridRightDownPos);
		int num = Mathf.CeilToInt(Mathf.Sqrt(amount));
		for (int i = 0; i < amount; i++)
		{
			int num2 = i / num;
			int num3 = -i % num;
			Vector3 position = vector + new Vector3((float)num3 * 0.484f, 0f, (float)num2 * 0.484f);
			WgoData data = new WgoData(zombieWgoId, position, worldId);
			Wgo wgo = GameScene.AddWgoData(data);
			ConfigureDevSpawnNavGraph(AgentsController.AddWgoAsAgent(wgo));
		}
	}

	private static void ConfigureDevSpawnNavGraph(FightingAgent agent)
	{
		if (!(agent == null))
		{
			agent.SetGraphMask(LazyConsts.Navigation.Graph.RuinedTemple, LazyConsts.Navigation.Graph.Fighting_Recast);
		}
	}

	public Wgo SpawnZombie(WgoData wgoData)
	{
		return GameScene.AddWgoData(wgoData);
	}

	public void Init(Wgo zombie, List<PathfindingPenalty> penalties = null)
	{
		AgentsController.AddWgoAsAgent(zombie, penalties);
	}

	public void DespawnZombies(int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		int agentsCount = AgentsController.AgentsCount;
		amount = Mathf.Min(amount, agentsCount);
		for (int num = agentsCount - 1; num >= agentsCount - amount; num--)
		{
			FightingAgent fightingAgent = AgentsController.Agents[num];
			if (!(fightingAgent == null))
			{
				Wgo wgo = fightingAgent.Wgo;
				SGuid uniqueId = wgo.Data.UniqueId;
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(uniqueId);
				AgentsController.RemoveAgent(uniqueId);
				Object.Destroy(wgo.gameObject);
			}
		}
	}

	public void DespawnZombie(Wgo zombie)
	{
		if (!(zombie == null))
		{
			SGuid uniqueId = zombie.Data.UniqueId;
			MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(uniqueId);
			FightingAgent component = zombie.GetComponent<FightingAgent>();
			AgentsController.RemoveAgent(component.Wgo.Data.UniqueId);
			Object.Destroy(zombie.gameObject);
		}
	}

	public void DespawnAllZombies()
	{
		foreach (FightingAgent agent in AgentsController.Agents)
		{
			WgoData wgoData = agent?.Wgo.Data;
			if (wgoData != null)
			{
				SGuid uniqueId = wgoData.UniqueId;
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(uniqueId);
			}
		}
		AgentsController.RemoveAllAgents();
	}

	public void PlaceAllToStartOnGrid(Vector3 gridRightDownPos)
	{
		if (AgentsController.AgentsCount == 0)
		{
			return;
		}
		Vector3 vector = ResolveGridOrigin(gridRightDownPos);
		int num = Mathf.CeilToInt(Mathf.Sqrt(AgentsController.AgentsCount));
		for (int i = 0; i < AgentsController.Agents.Count; i++)
		{
			if (!(AgentsController.Agents[i] == null))
			{
				int num2 = i / num;
				int num3 = -i % num;
				Vector3 position = vector + new Vector3((float)num3 * 0.484f, 0f, (float)num2 * 0.484f);
				AgentsController.Agents[i].SetPosition(position);
			}
		}
	}

	private string ResolveSpawnGameSceneId()
	{
		if (!(MainGame.PlayerData.currentGameSceneId == "Dev_ZombieFigtersTest"))
		{
			return MainGame.PlayerData.currentGameSceneId;
		}
		return "Dev_ZombieFigtersTest";
	}

	private Vector3 ResolveGridOrigin(Vector3 defaultGridRightDownPos)
	{
		if (MainGame.PlayerData.currentGameSceneId != "Dev_ZombieFigtersTest")
		{
			Vector2 vector = MainGame.PlayerData.Direction * 4f;
			return MainGame.PlayerData.position.Value + new Vector3(vector.x, 0f, vector.y);
		}
		return defaultGridRightDownPos;
	}

	private void Update()
	{
		if (!MainGame.IsGamePaused)
		{
			AgentsController.CustomUpdate(Time.deltaTime);
		}
	}
}
