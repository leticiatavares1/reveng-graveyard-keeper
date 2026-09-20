using System;

[Serializable]
public class BuffDefinition : BalanceBaseObject
{
	public enum BuffOverlayType
	{
		Set,
		Add
	}

	public bool is_hidden;

	public GameRes res = new GameRes();

	public SmartExpression length;

	public bool do_not_show_timer;

	public float tick_period;

	public SmartExpression se_start;

	public SmartExpression se_finish;

	public SmartExpression se_tick;

	public string custom_icon;

	public GameRes condition_player_res = new GameRes();

	public float craft_q;

	public BuffOverlayType overlay_type;

	public string GetIconName()
	{
		if (!string.IsNullOrEmpty(custom_icon))
		{
			return custom_icon;
		}
		return "b_" + id;
	}

	public string GetLocalizedName()
	{
		return GJL.L(id);
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
