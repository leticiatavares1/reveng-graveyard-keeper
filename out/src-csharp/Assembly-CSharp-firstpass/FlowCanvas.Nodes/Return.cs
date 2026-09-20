using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Should always be used to return out of a Custom Function. The return value is only required if the Custom Function returns a value as well.")]
[Color("d86b13")]
[Category("Functions/Custom")]
[ContextDefinedInputs(new Type[] { typeof(object) })]
public class Return : FlowControlNode
{
	protected override void RegisterPorts()
	{
		ValueInput<object> returnPort = AddValueInput<object>("Value");
		AddFlowInput(" ", delegate(Flow f)
		{
			f.Return(returnPort.value);
		});
	}
}
