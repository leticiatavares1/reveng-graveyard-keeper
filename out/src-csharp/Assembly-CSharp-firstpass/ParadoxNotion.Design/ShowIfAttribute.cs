using System;

namespace ParadoxNotion.Design;

[AttributeUsage(AttributeTargets.Field)]
public class ShowIfAttribute : Attribute
{
	public string fieldName;

	public int checkValue;

	public ShowIfAttribute(string fieldName, int checkValue)
	{
		this.fieldName = fieldName;
		this.checkValue = checkValue;
	}
}
