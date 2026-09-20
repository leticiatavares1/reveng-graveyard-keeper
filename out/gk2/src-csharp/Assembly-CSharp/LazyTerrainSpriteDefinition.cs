using System;

[Serializable]
public class LazyTerrainSpriteDefinition
{
	public string name;

	public byte tx;

	public byte x;

	public byte y;

	public byte w;

	public byte h;

	public string type = "-";

	public void GetTypeAndIndex(out string stype, out int idx)
	{
		if (type.Length > 1)
		{
			string text = type;
			if (char.IsDigit(text[text.Length - 1]))
			{
				string text2 = type;
				idx = int.Parse(text2[text2.Length - 1].ToString() ?? "");
				string text3 = type;
				stype = text3.Substring(0, text3.Length - 1);
				return;
			}
		}
		stype = type;
		idx = 0;
	}
}
