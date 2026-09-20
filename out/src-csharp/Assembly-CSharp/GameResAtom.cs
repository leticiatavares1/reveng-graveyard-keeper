using System;

[Serializable]
public class GameResAtom
{
	public string type;

	public float value;

	public static GameResAtom empty => new GameResAtom("empty", -1);

	public GameResAtom()
	{
	}

	public GameResAtom(GameResAtom source)
	{
		type = source.type;
		value = source.value;
	}

	public GameResAtom(string t, int v)
	{
		type = t;
		value = v;
	}

	public GameResAtom(string t, float v)
	{
		type = t;
		value = v;
	}

	public bool IsEmpty()
	{
		if (!(type == "empty"))
		{
			return value.EqualsTo(0f);
		}
		return true;
	}

	public bool IsNotEmpty()
	{
		return !IsEmpty();
	}

	public override string ToString()
	{
		return $"[t={type}, v={value}]";
	}
}
