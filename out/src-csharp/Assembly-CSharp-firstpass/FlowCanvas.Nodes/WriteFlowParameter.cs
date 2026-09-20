using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[ContextDefinedInputs(new Type[] { typeof(Wild) })]
[Description("Writes (or creates) a named parameter to the incomming Flow, which you can later read with a ReadFlowParameter node.\nFlow parameters are temporary variables that exist only in the context of the same Flow.")]
[Category("Flow Controllers/Flow Convert")]
public class WriteFlowParameter<T> : FlowControlNode
{
	protected override void RegisterPorts()
	{
		FlowOutput o = AddFlowOutput("Out");
		ValueInput<string> pName = AddValueInput<string>("Name");
		ValueInput<T> pValue = AddValueInput<T>("Value");
		AddFlowInput("In", delegate(Flow f)
		{
			f.WriteParameter(pName.value, pValue.value);
			o.Call(f);
		});
	}
}
