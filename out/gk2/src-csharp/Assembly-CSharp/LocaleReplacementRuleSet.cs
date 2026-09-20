using LazyBearTechnology;
using UnityEngine;

public class LocaleReplacementRuleSet : BaseReplacementRuleSet
{
	private const string GAME_KEY_PREFIX = "GameKey";

	public override string Replace(string input)
	{
		if (input.StartsWith("GameKey"))
		{
			GameKey byStaticFieldName = Enumeration.GetByStaticFieldName<GameKey>(input.Replace("GameKey", ""));
			if (byStaticFieldName == null)
			{
				Debug.LogError("#icon# Can't parse LocaleReplacementRuleSet value:[" + input + "]");
				return string.Empty;
			}
			return ControllerIconLibrary.GetIconId(byStaticFieldName, null, trailingSpace: false);
		}
		return base.Replace(input);
	}
}
