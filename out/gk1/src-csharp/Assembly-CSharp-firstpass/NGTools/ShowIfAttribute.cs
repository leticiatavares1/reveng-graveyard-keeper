using UnityEngine;

namespace NGTools;

public class ShowIfAttribute : PropertyAttribute
{
	public readonly string fieldName;

	public readonly Ops @operator;

	public readonly MultiOps multiOperator;

	public readonly object[] values;

	public ShowIfAttribute(string fieldName, Ops @operator, object value)
	{
		this.fieldName = fieldName;
		this.@operator = @operator;
		multiOperator = MultiOps.None;
		values = new object[1] { value };
	}

	public ShowIfAttribute(string fieldName, MultiOps multiOperator, params object[] values)
	{
		this.fieldName = fieldName;
		@operator = Ops.None;
		this.multiOperator = multiOperator;
		this.values = values;
	}
}
