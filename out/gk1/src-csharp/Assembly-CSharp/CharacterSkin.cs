using System;
using System.Text;

[Serializable]
public class CharacterSkin
{
	public int weapon = 1;

	public int head = 1;

	public int body = 1;

	public int legs = 1;

	private static StringBuilder _sb = new StringBuilder();

	public string ReplaceSpriteName(string s)
	{
		if (s.Length < 7)
		{
			return s;
		}
		string text = s.Substring(4, 3);
		int num = -1;
		if (text != null && text == "wpn")
		{
			num = weapon;
		}
		switch (num)
		{
		case -1:
			return s;
		case 0:
			return "transparent_2x2";
		default:
			_sb.Length = 0;
			_sb.Append(num.ToString("000"));
			_sb.Append(s.Substring(3));
			return _sb.ToString();
		}
	}
}
