using UnityEngine;

namespace NGTools;

public class GHeaderAttribute : PropertyAttribute
{
	public readonly bool first;

	public readonly string header;

	public GHeaderAttribute(string header, bool first = false)
	{
		this.header = header;
		this.first = first;
	}
}
