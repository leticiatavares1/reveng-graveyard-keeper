using System;

[Serializable]
public class PerkDefinition : BalanceBaseObject
{
	public GameRes output_res = new GameRes();

	public string icon = "";

	public float stars;

	public bool show = true;

	public string GetIcon()
	{
		if (!string.IsNullOrEmpty(icon))
		{
			return icon;
		}
		return "i_" + id;
	}

	public string GetDescriptionIfExists()
	{
		string text = id + "_d";
		string text2 = GJL.L(text);
		if (!(text == text2))
		{
			return text2;
		}
		return string.Empty;
	}
}
