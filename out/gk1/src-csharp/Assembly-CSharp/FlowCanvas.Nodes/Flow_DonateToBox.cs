using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Donate to box", 0)]
[Category("Game Actions")]
public class Flow_DonateToBox : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("Box WGO");
		ValueInput<float> par_money = AddValueInput<float>("Money");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			EffectBubblesManager.ShowImmediately(worldGameObject.bubble_pos, Trading.FormatMoney(par_money.value));
			worldGameObject.AddToParams("_money", par_money.value);
			Sounds.PlaySound("donations_coin");
			flow_out.Call(f);
		});
	}
}
