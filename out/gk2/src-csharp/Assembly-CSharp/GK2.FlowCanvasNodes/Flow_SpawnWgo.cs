using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Spawn WgoData", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SpawnWgo : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> id;

	private ValueInput<string> customTag;

	private new ValueInput<Vector3> position;

	private ValueInput<string> worldId;

	private ValueOutput<WgoData> wgoData;

	private WgoData spawnedData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SpawnWgoData);
		@out = AddFlowOutput("out".CapitalizeFirst());
		id = AddValueInput<string>("id".CapitalizeFirst());
		customTag = AddValueInput<string>("customTag".CapitalizeFirst());
		position = AddValueInput<Vector3>("position".CapitalizeFirst());
		worldId = AddValueInput<string>("worldId".CapitalizeFirst());
		wgoData = AddValueOutput("wgoData".CapitalizeFirst(), () => spawnedData);
	}

	private void SpawnWgoData(Flow flow)
	{
		string text = worldId.value;
		if (string.IsNullOrEmpty(text))
		{
			text = MainGame.PlayerData.currentGameSceneId;
		}
		MainGame.Instance.GameSave.worldData.AddWgoData(id.value, position.value, text, customTag.value, out spawnedData);
		@out.Call(flow);
	}
}
