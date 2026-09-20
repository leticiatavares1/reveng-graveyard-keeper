using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Play Music", 0)]
[Category("Game Actions")]
[Description("Play Music by name")]
public class Flow_PlayMusic : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort("sound").isConnected && GetInputValuePort("sound").isDefaultValue)
			{
				return "Stop Music";
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
		AddValueInput<string>("sound");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			Debug.LogError("Use of a depricated 'PlayMusic' Flow block");
			flow_out.Call(f);
		});
	}
}
