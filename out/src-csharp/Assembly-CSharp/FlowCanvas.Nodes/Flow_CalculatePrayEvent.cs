using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Calculate Pray Event", 0)]
public class Flow_CalculatePrayEvent : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_id = AddValueInput<string>("custom id");
		FlowOutput flow_out = AddFlowOutput("Out");
		int _people = 0;
		int _faith = 0;
		int _faith_bonus = 0;
		float _money = 0f;
		bool _success = false;
		AddValueOutput("people", () => _people);
		AddValueOutput("faith", () => _faith);
		AddValueOutput("faith bonus", () => _faith_bonus);
		AddValueOutput("money", () => _money);
		AddValueOutput("success", () => _success);
		AddFlowInput("In", delegate(Flow f)
		{
			PrayLogics.PrayResult prayResult = PrayLogics.CalculatePray(string.IsNullOrEmpty(par_id.value) ? GUIElements.me.pray_craft.pray_craft.linked_sub_id : par_id.value);
			_people = prayResult.people;
			_faith = prayResult.faith;
			_faith_bonus = prayResult.faith_bonus;
			_money = prayResult.money + prayResult.money_bonus;
			_success = prayResult.success;
			flow_out.Call(f);
		});
	}
}
