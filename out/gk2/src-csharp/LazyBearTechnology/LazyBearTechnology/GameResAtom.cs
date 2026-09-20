using System;

namespace LazyBearTechnology;

[Serializable]
public class GameResAtom : IStringIntSet
{
	public string type;

	public float value;

	public static GameResAtom Empty => new GameResAtom(string.Empty, 0);

	public GameResAtom()
	{
	}

	public GameResAtom(GameResAtom source)
	{
		type = source.type;
		value = source.value;
	}

	public GameResAtom(string type, int value)
	{
		this.type = type;
		this.value = value;
	}

	public GameResAtom(string type, float value)
	{
		this.type = type;
		this.value = value;
	}

	public bool IsEmpty()
	{
		if (!(type == string.Empty))
		{
			return value == 0f;
		}
		return true;
	}

	public override string ToString()
	{
		return $"[t={type}, v={value}]";
	}

	public void Set(string id, int v)
	{
		type = id;
		value = v;
	}
}
