using System;
using LazyBearTechnology;

[Serializable]
public class WSODef : BalanceBaseObject
{
	[AutoParse("replacement_config_id")]
	public string replacementConfigId;

	[AutoParse("town_quality")]
	public int townQuality;

	[NonSerialized]
	private ConstructorPartReplacementConfig cachedReplacementConfig;

	public ConstructorPartReplacementConfig ReplacementConfig
	{
		get
		{
			if (!cachedReplacementConfig && !string.IsNullOrEmpty(replacementConfigId))
			{
				cachedReplacementConfig = ConstructorPartReplacementService.GetReplacementConfig(replacementConfigId);
			}
			return cachedReplacementConfig;
		}
	}

	public WSODef()
	{
	}

	public WSODef(string id)
	{
		base.id = id;
	}
}
