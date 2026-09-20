using System;

[Serializable]
public class QuestLinkedElement : IAutoParsable
{
	public QuestElementInfoType type;

	public string id;

	public int count;

	public override string ToString()
	{
		if (string.IsNullOrEmpty(id))
		{
			return string.Empty;
		}
		string text = id;
		if (count != 0)
		{
			text += $"={count}";
		}
		text = "[" + text + "]";
		return type switch
		{
			QuestElementInfoType.Item => "item" + text, 
			QuestElementInfoType.GameRes => "gameres" + text, 
			QuestElementInfoType.Building => "building" + text, 
			QuestElementInfoType.Craft => "craft" + text, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
