using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class WorldZoneDef : BalanceBaseObject
{
	public enum DisplayType
	{
		Hidden = -1,
		None,
		Sum
	}

	[AutoParse("builder_id")]
	public string builderId;

	[AutoParse("has_custom_quality_zones")]
	public bool hasCustomQualityZones;

	[AutoParse("display_type")]
	public DisplayType displayType;

	[AutoParse("quality_icon")]
	public string qualityIcon;

	[AutoParse("build_desk_icon")]
	public string buildDeskIcon;

	[AutoParse("string_format")]
	public string stringFormat;

	[AutoParse("porter_start_point")]
	public string porterStartPoint;

	[AutoParse("porter_end_point")]
	public string porterEndPoint;

	[AutoParse("on_enter_expressions")]
	public List<LazyExpression> onEnterExpressions = new List<LazyExpression>();

	[AutoParse("on_exit_expressions")]
	public List<LazyExpression> onExitExpressions = new List<LazyExpression>();

	[AutoParse("expressions_on_max_quality_increased")]
	public List<LazyExpression> expressionsOnMaxQualityIncreased = new List<LazyExpression>();
}
