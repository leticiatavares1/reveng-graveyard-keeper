using System;

namespace LazyBearTechnology;

[Serializable]
public class GameResConditionAtom
{
	public string type;

	public float value;

	public GameResCondition.Condition condition;

	public GameResConditionAtom()
	{
	}

	public GameResConditionAtom(GameResConditionAtom source)
	{
		type = source.type;
		value = source.value;
		condition = source.condition;
	}

	public GameResConditionAtom(string type, int value, GameResCondition.Condition condition)
	{
		this.type = type;
		this.value = value;
		this.condition = condition;
	}

	public GameResConditionAtom(string type, float value, GameResCondition.Condition condition)
	{
		this.type = type;
		this.value = value;
		this.condition = condition;
	}

	public override string ToString()
	{
		return $"[{type} {GameResCondition.ConditionToString(condition)} {value}]";
	}
}
