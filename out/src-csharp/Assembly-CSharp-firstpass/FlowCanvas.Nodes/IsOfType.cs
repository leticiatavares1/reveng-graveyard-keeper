using System;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Returns whether the input object is of type T as well as the object casted to T if so")]
[Category("Logic Operators")]
public class IsOfType : PureFunctionNode<bool, object, Type>
{
	public object OBJECT { get; private set; }

	public override bool Invoke(object OBJECT, Type type)
	{
		this.OBJECT = OBJECT;
		if (OBJECT != null)
		{
			return type.RTIsAssignableFrom(OBJECT.GetType());
		}
		return false;
	}
}
