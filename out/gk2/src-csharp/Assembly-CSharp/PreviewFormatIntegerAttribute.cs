using UnityEngine;

public class PreviewFormatIntegerAttribute : PropertyAttribute
{
	public string PreviewFormatInteger { get; private set; }

	public PreviewFormatIntegerAttribute(string format)
	{
		PreviewFormatInteger = format;
	}
}
