using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Create ZombieWgoData", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_CreateZombieWgoData : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> id;

	private ValueInput<string> worldId;

	private ValueOutput<ZombieWgoData> zombieData;

	private ZombieWgoData spawnedData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SpawnWgoData);
		@out = AddFlowOutput("out".CapitalizeFirst());
		id = AddValueInput<string>("id".CapitalizeFirst());
		worldId = AddValueInput<string>("worldId".CapitalizeFirst());
		zombieData = AddValueOutput("zombieData".CapitalizeFirst(), () => spawnedData);
	}

	private void SpawnWgoData(Flow flow)
	{
		string text = worldId.value;
		if (string.IsNullOrEmpty(text))
		{
			text = MainGame.PlayerData.currentGameSceneId;
		}
		Item zombieItem = GameBalance.Me.GetData<BodyDef>(id.value).GenerateItem();
		spawnedData = MainGame.ZombieSystemData.CreateZombieDrop("zombie", MainGame.PlayerData.position.Value, text, zombieItem);
		@out.Call(flow);
	}
}
