using System;
using System.Globalization;
using System.Text;
using LitJson;

public class NewsItemData
{
	private const string PREFIX = "";

	private const string SUFFIX = "\n";

	public string version;

	public bool is_upcoming;

	public string items;

	public DateTime date;

	public int progress = -1;

	public bool visible = true;

	public NewsItemData(string version, JsonData data)
	{
		if (version == "info")
		{
			NewsGUI.update_url = (string)data["url"];
			visible = false;
			return;
		}
		is_upcoming = version == "upcoming";
		if (data.Keys.Contains("visible"))
		{
			visible = (int)data["visible"] == 1;
		}
		if (is_upcoming)
		{
			this.version = (string)data["version"];
			progress = (int)data["progress"];
		}
		else
		{
			this.version = version;
			if (visible)
			{
				float num = float.Parse(version, CultureInfo.InvariantCulture);
				if (num > NewsGUI.last_ver)
				{
					NewsGUI.last_ver = num;
				}
			}
		}
		date = DateTime.Parse((string)data["date"], new CultureInfo("ru-RU"));
		JsonData jsonData = data["items"];
		StringBuilder stringBuilder = new StringBuilder();
		int count = jsonData.Count;
		for (int i = 0; i < count; i++)
		{
			string text = (string)jsonData[i];
			stringBuilder.Append("");
			text = text.Replace("(!)", "[ff0000][!][-]");
			text = text.Replace("(+)", "[ffff00][+][-]");
			stringBuilder.Append(text);
			stringBuilder.Append("\n");
		}
		items = stringBuilder.ToString();
	}
}
