using System;
using System.Linq;

[Serializable]
public class InteractionEvent
{
	public enum Type
	{
		None,
		Talk,
		Pray,
		Reward,
		Fishing,
		Work
	}

	public Type type;

	public string str;

	public bool isFake;

	public string CustomIcon => type switch
	{
		Type.None => string.Empty, 
		Type.Talk => "icon_speech_bubble", 
		Type.Pray => "icon_pray_bubble", 
		Type.Reward => "icon_coins_bubble", 
		Type.Fishing => "icon_fishing_bubble", 
		Type.Work => "icon_view_bubble", 
		_ => string.Empty, 
	};

	public InteractionEvent(string str, bool isFake = false)
	{
		this.str = str;
		this.isFake = isFake;
		type = str.Split('_').Last() switch
		{
			"talk" => Type.Talk, 
			"pray" => Type.Pray, 
			"reward" => Type.Reward, 
			"fishing" => Type.Fishing, 
			"work" => Type.Work, 
			_ => Type.Talk, 
		};
	}
}
