using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class GameRes
{
	[SerializeField]
	private List<string> _res_type = new List<string>();

	[SerializeField]
	private List<float> _res_v = new List<float>();

	[SerializeField]
	private float _hp;

	[SerializeField]
	private float _progress;

	[SerializeField]
	private float _money;

	[SerializeField]
	private float _durability;

	public List<string> Types => _res_type;

	public float hp
	{
		get
		{
			return _hp;
		}
		set
		{
			_hp = value;
		}
	}

	public float progress
	{
		get
		{
			return _progress;
		}
		set
		{
			_progress = value;
		}
	}

	public float money
	{
		get
		{
			return _money;
		}
		set
		{
			_money = value;
		}
	}

	public float durability
	{
		get
		{
			return _durability;
		}
		set
		{
			_durability = value;
		}
	}

	public GameRes()
	{
	}

	public GameRes(GameRes r)
	{
		foreach (GameResAtom item in r.ToAtomList())
		{
			Add(item);
		}
	}

	public GameRes(string stype, float value)
	{
		Set(stype, value);
	}

	public float Get(string stype, float default_value = 0f)
	{
		switch (stype)
		{
		case "hp":
			return _hp;
		case "progress":
			return _progress;
		case "money":
			return _money;
		case "durability":
			return _durability;
		default:
		{
			int num = _res_type.IndexOf(stype);
			if (num != -1)
			{
				return _res_v[num];
			}
			return default_value;
		}
		}
	}

	public int GetInt(string stype)
	{
		return (int)Get(stype);
	}

	public bool Has(string stype)
	{
		switch (stype)
		{
		case "hp":
		case "progress":
		case "money":
		case "durability":
			return true;
		default:
			return _res_type.Contains(stype);
		}
	}

	public void Set(string type, float value)
	{
		switch (type)
		{
		case "hp":
			_hp = value;
			return;
		case "progress":
			_progress = value;
			return;
		case "money":
			_money = value;
			return;
		case "durability":
			_durability = value;
			return;
		}
		int num = _res_type.IndexOf(type);
		if (num != -1)
		{
			_res_v[num] = value;
			return;
		}
		_res_type.Add(type);
		_res_v.Add(value);
	}

	public void Clear()
	{
		_res_type.Clear();
		_res_v.Clear();
		_progress = (_hp = (_money = 0f));
		_durability = 1f;
	}

	public void Add(GameResAtom game_res_atom)
	{
		if (!game_res_atom.IsEmpty())
		{
			Add(game_res_atom.type, game_res_atom.value);
		}
	}

	public void Sub(GameResAtom game_res_atom)
	{
		if (!game_res_atom.IsEmpty())
		{
			Add(game_res_atom.type, 0f - game_res_atom.value);
		}
	}

	public void Add(string stype, float value)
	{
		switch (stype)
		{
		case "hp":
			_hp += value;
			return;
		case "progress":
			_progress += value;
			return;
		case "money":
			_money += value;
			return;
		case "durability":
			_durability += value;
			return;
		}
		int num = _res_type.IndexOf(stype);
		if (num != -1)
		{
			_res_v[num] += value;
			return;
		}
		_res_type.Add(stype);
		_res_v.Add(value);
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
			Set(stype, Mathf.RoundToInt(num * value));
		}
	}

	public GameRes Clone()
	{
		GameRes gameRes = new GameRes();
		for (int num = _res_type.Count - 1; num >= 0; num--)
		{
			gameRes.Set(_res_type[num], _res_v[num]);
		}
		gameRes._hp = _hp;
		gameRes._money = _money;
		gameRes._progress = _progress;
		gameRes._durability = _durability;
		return gameRes;
	}

	public void RemoveZeroValues()
	{
		List<int> list = new List<int>();
		for (int num = _res_type.Count - 1; num >= 0; num--)
		{
			if ((double)Mathf.Abs(_res_v[num]) < 0.0001)
			{
				list.Add(num);
			}
		}
		foreach (int item in list)
		{
			_res_type.RemoveAt(item);
			_res_v.RemoveAt(item);
		}
	}

	public static GameRes operator +(GameRes r1, GameRes r2)
	{
		GameRes gameRes = r1.Clone();
		foreach (GameResAtom item in r2.ToAtomList())
		{
			gameRes.Add(item);
		}
		gameRes.RemoveZeroValues();
		return gameRes;
	}

	public static GameRes operator -(GameRes r1, GameRes r2)
	{
		GameRes gameRes = r1.Clone();
		foreach (GameResAtom item in r2.ToAtomList())
		{
			gameRes.Sub(item);
		}
		gameRes.hp = r1.hp - r2.hp;
		gameRes.money = r1.money - r2.money;
		gameRes.durability = r1.durability - r2.durability;
		gameRes.progress = r1.progress - r2.progress;
		gameRes.RemoveZeroValues();
		return gameRes;
	}

	public static GameRes operator *(GameRes r1, float k)
	{
		GameRes gameRes = new GameRes();
		foreach (GameResAtom item in r1.ToAtomList())
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

	public List<GameResAtom> ToAtomList(float tired_k = 1f)
	{
		List<GameResAtom> list = new List<GameResAtom>();
		for (int i = 0; i < _res_type.Count; i++)
		{
			list.Add(new GameResAtom
			{
				type = _res_type[i],
				value = _res_v[i]
			});
		}
		if (!hp.EqualsTo(0f))
		{
			list.Add(new GameResAtom("hp", (int)_hp));
		}
		if (!progress.EqualsTo(0f))
		{
			list.Add(new GameResAtom("progress", _progress));
		}
		if (!money.EqualsTo(0f))
		{
			list.Add(new GameResAtom("money", _money));
		}
		if (!durability.EqualsTo(0f))
		{
			list.Add(new GameResAtom("durability", _durability));
		}
		return list;
	}

	public bool IsEnough(GameRes sub)
	{
		bool result = true;
		foreach (GameResAtom item in sub.ToAtomList())
		{
			if (!IsEnough(item.type, item.value))
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
		if (_res_v.Count == 0 && Mathf.Abs(_hp) < 0.001f && Mathf.Abs(_money) < 0.001f && Mathf.Abs(_progress) < 0.001f)
		{
			return Mathf.Abs(_durability) < 0.001f;
		}
		return false;
	}

	public override string ToString()
	{
		string text = "";
		foreach (string item in _res_type)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text = text + item + "=" + Get(item);
		}
		return "[GameRes: " + text + "]";
	}

	public string ToPrintableString(bool use_colors = false, Color clr_normal = default(Color), Color clr_not_enough = default(Color), bool force_parentheses = false, bool float_format = false, List<string> skip = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = -1;
		foreach (string item in _res_type)
		{
			num++;
			if (skip != null && skip.Contains(item))
			{
				continue;
			}
			float a = Get(item);
			if (!a.EqualsTo(0f))
			{
				int @int = GetInt(item);
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" ");
				}
				string text = item;
				if (TechDefinition.TECH_POINTS.Contains(item) || force_parentheses)
				{
					text = "(" + text + ")";
				}
				stringBuilder.Append(text);
				if (use_colors)
				{
					stringBuilder.Append('[');
					stringBuilder.Append(MainGame.me.player.IsEnough(new GameRes(item, _res_v[num])) ? clr_normal.ToHex(alpha: true) : clr_not_enough.ToHex(alpha: true));
					stringBuilder.Append(']');
				}
				if (float_format)
				{
					stringBuilder.Append(a.ToString("0.0"));
				}
				else
				{
					stringBuilder.Append(@int);
				}
				if (use_colors)
				{
					stringBuilder.Append("[-]");
				}
			}
		}
		return stringBuilder.ToString();
	}

	public string ToFormattedString(bool colorize_values = true, GameRes add_game_res = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = "";
		string text2 = "";
		List<string> list = new List<string> { "hp" };
		list.AddRange(_res_type);
		if (add_game_res != null)
		{
			foreach (GameResAtom item in add_game_res.ToAtomList())
			{
				if (!list.Contains(item.type))
				{
					list.Add(item.type);
				}
			}
		}
		foreach (string item2 in list)
		{
			int num = GetInt(item2);
			if (add_game_res != null)
			{
				num += add_game_res.GetInt(item2);
			}
			if (num != 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" ");
				}
				switch (item2)
				{
				case "hp":
					text = "[c][F4203F]";
					text2 = "(hp)";
					break;
				case "energy":
					text = "[c][3897FF]";
					text2 = "(en)";
					break;
				case "sanity":
					text = "[c][BA00C5]";
					text2 = "(sn)";
					break;
				default:
					text = "[14E549]";
					text2 = "(" + item2 + ")";
					break;
				}
				if (colorize_values)
				{
					stringBuilder.Append(text);
				}
				stringBuilder.Append((num >= 0) ? "+" : "-");
				stringBuilder.Append(text2);
				if (num != 0)
				{
					stringBuilder.Append(Mathf.Abs(num));
				}
				if (colorize_values)
				{
					stringBuilder.Append("[-][/c]");
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string ToFormattedString(List<GameRes> reses, bool colorize_values = true)
	{
		if (reses == null || reses.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = "";
		string text2 = "";
		List<string> list = new List<string> { "hp", "energy" };
		foreach (GameRes rese in reses)
		{
			if (rese == null)
			{
				continue;
			}
			foreach (string item in rese._res_type)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		foreach (string item2 in list)
		{
			int num = int.MaxValue;
			int num2 = int.MinValue;
			foreach (GameRes rese2 in reses)
			{
				if (!(rese2 == null))
				{
					int @int = rese2.GetInt(item2);
					if (@int < num)
					{
						num = @int;
					}
					if (@int > num2)
					{
						num2 = @int;
					}
				}
			}
			if (num != 0 || num2 != 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" ");
				}
				switch (item2)
				{
				case "hp":
					text = "[c][F4203F]";
					text2 = "(hp)";
					break;
				case "energy":
					text = "[c][3897FF]";
					text2 = "(en)";
					break;
				case "sanity":
					text = "[c][BA00C5]";
					text2 = "(sn)";
					break;
				default:
					text = "[14E549]";
					text2 = "(" + item2 + ")";
					break;
				}
				if (colorize_values)
				{
					stringBuilder.Append(text);
				}
				stringBuilder.Append((num >= 0) ? "+" : "-");
				stringBuilder.Append(text2);
				stringBuilder.Append(Mathf.Abs(num));
				if (num < num2)
				{
					stringBuilder.Append(" .. ");
					stringBuilder.Append((num2 >= 0) ? "+" : "-");
					stringBuilder.Append(text2);
					stringBuilder.Append(Mathf.Abs(num2));
				}
				if (colorize_values)
				{
					stringBuilder.Append("[-][/c]");
				}
			}
		}
		return stringBuilder.ToString();
	}

	public void RemoveAllBut(List<string> exceptions)
	{
		for (int i = 0; i < _res_type.Count; i++)
		{
			if (!exceptions.Contains(_res_type[i]))
			{
				_res_v[i] = 0f;
			}
		}
		if (!exceptions.Contains("hp"))
		{
			hp = 0f;
		}
		if (!exceptions.Contains("money"))
		{
			money = 0f;
		}
		if (!exceptions.Contains("progress"))
		{
			progress = 0f;
		}
		if (!exceptions.Contains("durability"))
		{
			durability = 0f;
		}
		RemoveZeroValues();
	}

	public static GameRes ParseSmartExpression(SmartExpression smart_expr)
	{
		GameRes gameRes = new GameRes();
		if (smart_expr == null || smart_expr.HasNoExpresion())
		{
			return gameRes;
		}
		string rawExpressionString = smart_expr.GetRawExpressionString();
		if (rawExpressionString.Contains("energy"))
		{
			string pattern = "(AddPpar\\( *(\"energy\") *, *(.*)\\))";
			Match match = Regex.Match(rawExpressionString, pattern);
			if (match.Success)
			{
				if (match.Groups[2].Captures[0].ToString() != "\"energy\"")
				{
					Debug.LogError("Error while parsing SmartExpression #1. Call Bulat.");
				}
				else
				{
					float value = SmartExpression.ParseExpression(match.Groups[3].Captures[0].ToString()).EvaluateFloat(null, MainGame.me.player);
					gameRes.Add("energy", value);
				}
			}
		}
		if (rawExpressionString.Contains("hp"))
		{
			Match match2 = Regex.Match(rawExpressionString, "(AddPpar\\( *(\"hp\") *, *(.*)\\))");
			if (match2.Success)
			{
				if (match2.Groups[2].Captures[0].ToString() != "\"hp\"")
				{
					Debug.LogError("Error while parsing SmartExpression #2. Call Bulat.");
				}
				else
				{
					float value2 = SmartExpression.ParseExpression(match2.Groups[3].Captures[0].ToString()).EvaluateFloat(null, MainGame.me.player);
					gameRes.Add("hp", value2);
				}
			}
		}
		return gameRes;
	}
}
