using UnityEngine;

public class RuntimeValueAttribute : PropertyAttribute
{
	public readonly string header;

	public readonly bool read_only;

	public RuntimeValueAttribute()
	{
		header = "";
		read_only = false;
	}

	public RuntimeValueAttribute(bool read_only)
	{
		header = "";
		this.read_only = read_only;
	}

	public RuntimeValueAttribute(string header, bool read_only = false)
	{
		this.header = header;
		this.read_only = read_only;
	}
}
