using System;

namespace ParadoxNotion.Design;

public class NameAttribute : Attribute
{
	public string name;

	public int priority;

	public NameAttribute(string name, int priority = 0)
	{
		this.name = name;
		this.priority = priority;
	}
}
