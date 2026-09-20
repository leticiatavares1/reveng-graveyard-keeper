using ParadoxNotion.Design;

namespace FlowCanvas.Nodes.Refugees;

[Category("Game Actions/Refugees")]
[Name("Get Refugee By Index", 0)]
public class Flow_GetRefugee : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<int> wgo_index = AddValueInput<int>("Refugee Index", "refugee_index");
		AddValueOutput("WGO", () => (wgo_index.value >= MainGame.me.save.refugees_camp_data.active_refugee_list.Count) ? null : MainGame.me.save.refugees_camp_data.active_refugee_list[wgo_index.value].world_game_object);
	}
}
