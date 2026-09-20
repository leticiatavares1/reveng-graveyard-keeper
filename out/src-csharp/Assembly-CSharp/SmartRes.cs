using System;
using UnityEngine;

[Serializable]
public class SmartRes
{
	public enum ResType
	{
		Empty,
		Item,
		GameRes
	}

	public ResType res_type;

	public Item item;

	[SerializeField]
	private GameResAtom _res;

	private WorldGameObject _linked_wgo;

	public GameResAtom res
	{
		get
		{
			if (_res.type == "_rel" && _linked_wgo != null)
			{
				string text = (string.IsNullOrEmpty(_linked_wgo.obj_def.npc_alias) ? _linked_wgo.obj_id : _linked_wgo.obj_def.npc_alias);
				return new GameResAtom("_rel_" + text, _res.value);
			}
			return _res;
		}
		set
		{
			_res = value;
		}
	}

	public void FillVisualData(out string icon_name, out int n, out string quality_icon, WorldGameObject linked_wgo = null)
	{
		quality_icon = (icon_name = "");
		n = 0;
		_linked_wgo = linked_wgo;
		switch (res_type)
		{
		case ResType.Empty:
			icon_name = "";
			n = 0;
			break;
		case ResType.Item:
		{
			ItemDefinition data = GameBalance.me.GetData<ItemDefinition>(item.id);
			icon_name = ((data == null) ? ("i_" + item.id) : data.GetIcon());
			if (data != null)
			{
				quality_icon = data.GetQualityIconName();
			}
			n = item.value;
			break;
		}
		case ResType.GameRes:
			icon_name = "res_" + _res.type;
			n = Mathf.RoundToInt(_res.value);
			if (_res.type == "_rel")
			{
				icon_name = ((n > 0) ? ":+(positive)" : ":-(negative)") + Mathf.Abs(n);
			}
			if (_res.type == "money")
			{
				icon_name = ":+" + Trading.FormatMoney(_res.value, print_zero: false, use_spaces: false);
			}
			if (_res.type == "r" || _res.type == "b" || _res.type == "g")
			{
				icon_name = ":+(" + _res.type + ")" + _res.value;
			}
			break;
		}
	}

	public override string ToString()
	{
		return $"[SmartRes {res_type}, item={item}, res={_res}]";
	}
}
