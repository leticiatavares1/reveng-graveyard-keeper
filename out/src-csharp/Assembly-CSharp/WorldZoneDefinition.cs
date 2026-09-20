using System;

[Serializable]
public class WorldZoneDefinition : BalanceBaseObject
{
	public enum QualityCalcMethod
	{
		None,
		Sum,
		Average
	}

	public QualityCalcMethod calc_method;

	public string quality_icon;

	public string hud_descr_str;

	public string gui_descr_str;

	public string string_format;

	public string zone_group;

	public GameRes zone_params = new GameRes();
}
