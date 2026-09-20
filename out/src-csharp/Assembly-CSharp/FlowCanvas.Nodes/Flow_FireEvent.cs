using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Fire Event", 0)]
public class Flow_FireEvent : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_event = AddValueInput<string>("event");
		ValueInput<float> par_delay = AddValueInput<float>("delay (s)");
		WorldGameObject _wgo = null;
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("WGO", () => _wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject o = WGOParamOrSelf(par_wgo);
			_wgo = o;
			if (o == null)
			{
				Debug.LogError("WGO GoTo error: WGO #1 is null");
				flow_out.Call(f);
			}
			else
			{
				float delay = par_delay.value;
				if (!string.IsNullOrEmpty(par_event.value))
				{
					GJTimer.AddTimer(0.03f, delegate
					{
						_wgo = o;
						o.FireEvent(par_event.value, delay);
					});
				}
				flow_out.Call(f);
			}
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("event");
	}
}
