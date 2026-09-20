using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Illustrations GUI", 0)]
public class FlowFlow_IllustrationsGUI_Vendor : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("do open").value)
			{
				return "Open Illustrations GUI";
			}
			return "Close Illustrations GUI";
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> in_do_open = AddValueInput<bool>("do open");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_do_open.value)
			{
				if (GUIElements.me.illustrations_gui.is_open)
				{
					Debug.LogError("Can not open IllustrationsGUI: window is already open");
				}
				else
				{
					GUIElements.me.illustrations_gui.Open();
				}
			}
			else if (GUIElements.me.illustrations_gui.is_open)
			{
				GUIElements.me.illustrations_gui.Hide();
			}
			else
			{
				Debug.LogError("Can not close IllustrationsGUI: window is already closed");
			}
			flow_out.Call(f);
		});
	}
}
