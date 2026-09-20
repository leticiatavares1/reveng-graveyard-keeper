using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("On Variable Change", 0)]
[Description("Called when the target variable change. (Not whenever it is set).")]
[Category("Events/Other")]
public class VariableChangedEvent : EventNode
{
	[BlackboardOnly]
	public BBParameter<object> targetVariable;

	private FlowOutput fOut;

	private object newValue;

	public override string name => $"{base.name} [{targetVariable}]";

	public override void OnGraphStarted()
	{
		if (targetVariable.varRef != null)
		{
			targetVariable.varRef.onValueChanged += OnChanged;
		}
	}

	public override void OnGraphStoped()
	{
		if (targetVariable.varRef != null)
		{
			targetVariable.varRef.onValueChanged -= OnChanged;
		}
	}

	protected override void RegisterPorts()
	{
		if (targetVariable.varRef != null)
		{
			fOut = AddFlowOutput("Out");
			AddValueOutput("Value", targetVariable.refType, () => newValue);
		}
	}

	private void OnChanged(string name, object value)
	{
		newValue = value;
		fOut.Call(default(Flow));
	}

	private void OnVariableRefChange(Variable newVarRef)
	{
		GatherPorts();
	}
}
