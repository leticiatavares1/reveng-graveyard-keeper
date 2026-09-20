using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[DoNotList]
[Name("Tag", 0)]
[Description("An easy way to get a Tag name")]
public class TagVariable : VariableNode
{
	[TagField]
	public BBParameter<string> tagName = "Untagged";

	public override string name => tagName.value;

	protected override void RegisterPorts()
	{
		AddValueOutput("Tag", () => tagName.value);
	}

	public override void SetVariable(object o)
	{
		if (o is string)
		{
			tagName.value = (string)o;
		}
	}
}
