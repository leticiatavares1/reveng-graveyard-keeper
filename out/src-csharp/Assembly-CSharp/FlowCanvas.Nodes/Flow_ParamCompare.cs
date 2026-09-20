using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Param compare", 0)]
[Description("If WGO is null, then self")]
public class Flow_ParamCompare : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetOutputPort("==").isConnected)
			{
				return base.name + " <color=#FF2020>[!!!]</color>";
			}
			if (!GetOutputPort("!=").isConnected && (!GetOutputPort(">").isConnected || !GetOutputPort("<").isConnected))
			{
				return base.name + " <color=#FF2020>[!!!]</color>";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_param = AddValueInput<string>("param");
		ValueInput<float> par_value = AddValueInput<float>("value");
		WorldGameObject _wgo = null;
		FlowOutput flow_eq = AddFlowOutput("==");
		FlowOutput flow_neq = AddFlowOutput("!=");
		FlowOutput flow_more = AddFlowOutput(">");
		FlowOutput flow_less = AddFlowOutput("<");
		AddValueOutput("WGO", () => _wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			_wgo = WGOParamOrSelf(par_wgo);
			if (!(_wgo == null))
			{
				float param = _wgo.GetParam(par_param.value);
				if (param.EqualsTo(par_value.value))
				{
					flow_eq.Call(f);
				}
				else
				{
					flow_neq.Call(f);
				}
				if (param > par_value.value)
				{
					flow_more.Call(f);
				}
				if (param < par_value.value)
				{
					flow_less.Call(f);
				}
			}
		});
	}
}
