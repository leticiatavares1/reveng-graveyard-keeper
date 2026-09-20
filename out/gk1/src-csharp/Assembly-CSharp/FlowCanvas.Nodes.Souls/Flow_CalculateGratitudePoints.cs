using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes.Souls;

[Name("Calculate Points For Released Soul", 0)]
[Category("Game Actions/Souls")]
public class Flow_CalculateGratitudePoints : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Item> _in_item;

	private ValueOutput<float> _out_gp;

	private float _out_gp_value;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", CalculatePoints);
		@out = AddFlowOutput("Out");
		_in_item = AddValueInput<Item>("Healed Soul");
		_out_gp = AddValueOutput("GP", () => _out_gp_value);
	}

	private void CalculatePoints(Flow flow)
	{
		if (_in_item.value == null || _in_item.value.IsEmpty())
		{
			Debug.LogError("Item is null or empty");
			@out.Call(flow);
		}
		else
		{
			_out_gp_value = SoulsHelper.CalculatePointsAfterSoulRelease(_in_item.value);
			@out.Call(flow);
		}
	}
}
