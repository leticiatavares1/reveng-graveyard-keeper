using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class GameRes
{
	[SerializeField]
	private List<GameResAtom> resValues = new List<GameResAtom>();

	[SerializeField]
	private List<string> resType = new List<string>();

	private Dictionary<string, GameResSystemBase> gameResSystems;

	public Dictionary<string, GameResSystemBase> GameResSystems => gameResSystems ?? new Dictionary<string, GameResSystemBase>();

	public List<GameResAtom> List => resValues;

	public List<string> TypesList => resType;

	public GameRes()
	{
	}

	public GameRes(GameRes gameRes)
	{
		int count = gameRes.List.Count;
		for (int i = 0; i < count; i++)
		{
			Add(gameRes.List[i]);
		}
	}

	public GameRes(string stype, float value)
	{
		Set(stype, value);
	}

	public GameRes(List<GameResAtom> atoms)
	{
		if (atoms != null)
		{
			for (int i = 0; i < atoms.Count; i++)
			{
				Add(atoms[i]);
			}
		}
	}

	public void SetSystems(Dictionary<string, GameResSystemBase> gameResAtomSystems)
	{
		gameResSystems = gameResAtomSystems;
	}

	public GameResSystemBase GetSystem(string stype)
	{
		return GameResSystems[stype];
	}

	public float Get(string stype, float defaultValue = 0f)
	{
		if (GameResSystems.TryGetValue(stype, out var value))
		{
			return value.Get();
		}
		return GetWithoutSystemsCheck(stype, defaultValue);
	}

	public float GetWithoutSystemsCheck(string stype, float defaultValue = 0f)
	{
		int num = resType.IndexOf(stype);
		if (num != -1)
		{
			return resValues[num].value;
		}
		return defaultValue;
	}

	public int GetInt(string stype)
	{
		return (int)Get(stype);
	}

	public bool Has(string stype)
	{
		return resType.Contains(stype);
	}

	public void Clear()
	{
		resType.Clear();
		resValues.Clear();
	}

	public void Set(string stype, float value)
	{
		if (GameResSystems.TryGetValue(stype, out var value2))
		{
			value2.Set(value);
		}
		else
		{
			SetWithoutSystemsCheck(stype, value);
		}
	}

	public void SetWithoutSystemsCheck(string stype, float value)
	{
		int num = resType.IndexOf(stype);
		if (num != -1)
		{
			resValues[num].value = value;
			return;
		}
		resType.Add(stype);
		resValues.Add(new GameResAtom(stype, value));
	}

	public void Set(GameRes gameRes)
	{
		int count = gameRes.List.Count;
		for (int i = 0; i < count; i++)
		{
			Set(gameRes.List[i].type, gameRes.List[i].value);
		}
	}

	public void Add(GameRes gameRes)
	{
		int count = gameRes.List.Count;
		for (int i = 0; i < count; i++)
		{
			Add(gameRes.List[i]);
		}
	}

	public void Add(GameResAtom gameResAtom)
	{
		if (!gameResAtom.IsEmpty())
		{
			Add(gameResAtom.type, gameResAtom.value);
		}
	}

	public void Add(string stype, float value)
	{
		if (GameResSystems.TryGetValue(stype, out var value2))
		{
			value2.Add(value);
		}
		else
		{
			AddWithoutSystemsCheck(stype, value);
		}
	}

	public void AddWithoutSystemsCheck(string stype, float value)
	{
		int num = resType.IndexOf(stype);
		if (num != -1)
		{
			resValues[num].value += value;
			return;
		}
		resType.Add(stype);
		resValues.Add(new GameResAtom(stype, value));
	}

	public void Sub(GameRes gameRes)
	{
		int count = gameRes.List.Count;
		for (int i = 0; i < count; i++)
		{
			Sub(gameRes.List[i]);
		}
	}

	public void Sub(GameResAtom gameResAtom)
	{
		if (!gameResAtom.IsEmpty())
		{
			Add(gameResAtom.type, 0f - gameResAtom.value);
		}
	}

	public void Sub(string stype, float value)
	{
		Add(stype, 0f - value);
	}

	public void Multiply(string stype, float value)
	{
		float num = Get(stype);
		if ((double)Mathf.Abs(num) > 0.0001)
		{
			Set(stype, num * value);
		}
	}

	public GameRes Clone()
	{
		GameRes gameRes = new GameRes();
		for (int num = resType.Count - 1; num >= 0; num--)
		{
			gameRes.Set(resType[num], resValues[num].value);
		}
		return gameRes;
	}

	public void RemoveZeroValues()
	{
		List<int> list = new List<int>();
		for (int num = resType.Count - 1; num >= 0; num--)
		{
			if ((double)Mathf.Abs(resValues[num].value) < 0.0001)
			{
				list.Add(num);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			resType.RemoveAt(list[i]);
			resValues.RemoveAt(list[i]);
		}
	}

	public void Sort(Comparison<GameResAtom> comparison)
	{
		resValues.Sort(comparison);
		List<string> list = new List<string>(resType);
		foreach (string item in resType)
		{
			int index = 0;
			for (int i = 0; i < resValues.Count; i++)
			{
				if (resValues[i].type == item)
				{
					index = i;
					break;
				}
			}
			list[index] = item;
		}
		resType = new List<string>(list);
	}

	public static GameRes operator +(GameRes r1, GameRes r2)
	{
		GameRes gameRes = r1.Clone();
		int count = r2.List.Count;
		for (int i = 0; i < count; i++)
		{
			gameRes.Add(r2.List[i]);
		}
		gameRes.RemoveZeroValues();
		return gameRes;
	}

	public static GameRes operator -(GameRes r1, GameRes r2)
	{
		GameRes gameRes = r1.Clone();
		int count = r2.List.Count;
		for (int i = 0; i < count; i++)
		{
			gameRes.Sub(r2.List[i]);
		}
		gameRes.RemoveZeroValues();
		return gameRes;
	}

	public bool IsEnough(GameRes sub)
	{
		bool result = true;
		List<GameResAtom> list = sub.List;
		for (int i = 0; i < list.Count; i++)
		{
			GameResAtom gameResAtom = list[i];
			if (!IsEnough(gameResAtom.type, gameResAtom.value))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool IsEnough(GameResAtom r)
	{
		return IsEnough(r.type, r.value);
	}

	public bool IsEnough(string type, float value)
	{
		bool result = true;
		if (Get(type) < value)
		{
			result = false;
		}
		return result;
	}

	public static GameRes operator *(GameRes r1, float k)
	{
		GameRes gameRes = new GameRes();
		foreach (GameResAtom item in r1.List)
		{
			gameRes.Set(item.type, r1.Get(item.type) * k);
		}
		gameRes.RemoveZeroValues();
		return gameRes;
	}

	public static GameRes operator /(GameRes r1, float k)
	{
		return r1 * (1f / k);
	}

	public static bool operator <(GameRes r1, GameRes r2)
	{
		return r2.IsEnough(r1);
	}

	public static bool operator <=(GameRes r1, GameRes r2)
	{
		if (!(r2 - r1).IsEmpty())
		{
			return r1 < r2;
		}
		return true;
	}

	public static bool operator >(GameRes r1, GameRes r2)
	{
		return r1.IsEnough(r2);
	}

	public static bool operator >=(GameRes r1, GameRes r2)
	{
		if (!(r1 - r2).IsEmpty())
		{
			return r1 > r2;
		}
		return true;
	}

	public static bool operator ==(GameRes r1, GameRes r2)
	{
		if ((object)r1 == r2)
		{
			return true;
		}
		if ((object)r1 == null || (object)r2 == null)
		{
			return false;
		}
		return (r1 - r2).IsEmpty();
	}

	public static bool operator !=(GameRes r1, GameRes r2)
	{
		return !(r1 == r2);
	}

	public bool IsEmpty()
	{
		RemoveZeroValues();
		return resType.Count == 0;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[GameRes: ");
		for (int i = 0; i < resType.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			string text = resType[i];
			stringBuilder.Append(text);
			stringBuilder.Append("=");
			stringBuilder.Append(Get(text));
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public void RemoveAllBut(List<string> exceptions)
	{
		for (int i = 0; i < resType.Count; i++)
		{
			if (!exceptions.Contains(resType[i]))
			{
				resValues[i].value = 0f;
			}
		}
		RemoveZeroValues();
	}

	public override bool Equals(object obj)
	{
		GameRes gameRes = obj as GameRes;
		if (gameRes != null && EqualityComparer<List<GameResAtom>>.Default.Equals(resValues, gameRes.resValues) && EqualityComparer<List<string>>.Default.Equals(resType, gameRes.resType))
		{
			return EqualityComparer<List<GameResAtom>>.Default.Equals(List, gameRes.List);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((-1419608871 * -1521134295 + EqualityComparer<List<GameResAtom>>.Default.GetHashCode(resValues)) * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(resType)) * -1521134295 + EqualityComparer<List<GameResAtom>>.Default.GetHashCode(List);
	}
}
