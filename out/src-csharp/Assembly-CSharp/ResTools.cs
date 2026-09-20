using System;
using System.Collections.Generic;

public static class ResTools
{
	public static string ParseResForCraft(this string res_srt)
	{
		return res_srt.Replace("[GameRes: ", "").Replace("]", "").Replace("=", " ");
	}

	public static int UsedInventoryCellCount(this GameRes res)
	{
		int num = 0;
		foreach (GameResAtom item in res.ToAtomList())
		{
			num += CellsUsedForRes(item, out var _);
		}
		return num;
	}

	public static List<GameResAtom> ToUnstackedAtomList(this GameRes res)
	{
		List<GameResAtom> list = new List<GameResAtom>();
		foreach (GameResAtom item in res.ToAtomList())
		{
			int stack_count;
			int num = CellsUsedForRes(item, out stack_count);
			float num2 = item.value;
			string type = item.type;
			if (num == 1)
			{
				list.Add(new GameResAtom(type, num2));
			}
			else if (num > 1)
			{
				while (num2 / (float)stack_count > 0f)
				{
					list.Add(new GameResAtom(type, stack_count));
					num2 -= (float)stack_count;
				}
				if (num2 > 0f)
				{
					list.Add(new GameResAtom(type, num2));
				}
			}
		}
		return list;
	}

	public static List<GameResAtom> ListOfResAtomsUntilFullStack(this GameRes res)
	{
		List<GameResAtom> list = new List<GameResAtom>();
		foreach (GameResAtom item in res.ToUnstackedAtomList())
		{
			ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(item.type);
			if (dataOrNull != null)
			{
				float num = (float)dataOrNull.stack_count - item.value;
				if (num > 0f)
				{
					list.Add(new GameResAtom(item.type, num));
				}
			}
		}
		return list;
	}

	private static int CellsUsedForRes(GameResAtom res_atom, out int stack_count)
	{
		stack_count = 0;
		ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(res_atom.type);
		if (dataOrNull == null)
		{
			return 0;
		}
		stack_count = dataOrNull.stack_count;
		return (int)Math.Ceiling(res_atom.value / (float)stack_count);
	}
}
