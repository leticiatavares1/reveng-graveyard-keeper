using UnityEngine;

public class SinItem : MonoBehaviour
{
	public enum ItemType
	{
		Greed,
		Sloth,
		Lust,
		Pride,
		Wrath,
		Envy,
		Gluttony
	}

	private const string GREED_KEY = "is_greed_unlocked";

	private const string WRATH_KEY = "is_wrath_unlocked";

	private const string ENVY_KEY = "is_envy_unlocked";

	private const string PRIDE_KEY = "is_pride_unlocked";

	private const string GLUTTONY_KEY = "is_gluttony_unlocked";

	private const string SLOTH_KEY = "is_sloth_unlocked";

	private const string LUST_KEY = "is_lust_unlocked";

	[SerializeField]
	private ItemType _item_type;

	[SerializeField]
	private BaseItemCellGUI _item_cell_gui;

	[SerializeField]
	[Space]
	private SoulPanelSkullBarGUI _skull_bar;

	public ItemType item_type => _item_type;

	public BaseItemCellGUI item_cell_gui => _item_cell_gui;

	public static string GetOrganIdBySin(SinItem sin_item)
	{
		string result = string.Empty;
		switch (sin_item.item_type)
		{
		case ItemType.Envy:
			result = "intestine";
			break;
		case ItemType.Gluttony:
			result = "fat";
			break;
		case ItemType.Greed:
			result = "flesh";
			break;
		case ItemType.Lust:
			result = "heart";
			break;
		case ItemType.Pride:
			result = "skin";
			break;
		case ItemType.Sloth:
			result = "brain";
			break;
		case ItemType.Wrath:
			result = "blood";
			break;
		}
		return result;
	}

	public static Item GetSinItemByTypeFromItem(ItemType item_type, Item from_item)
	{
		string text = string.Empty;
		switch (item_type)
		{
		case ItemType.Envy:
			text = "sin_envy";
			break;
		case ItemType.Gluttony:
			text = "sin_gluttony";
			break;
		case ItemType.Greed:
			text = "sin_greed";
			break;
		case ItemType.Lust:
			text = "sin_lust";
			break;
		case ItemType.Pride:
			text = "sin_pride";
			break;
		case ItemType.Sloth:
			text = "sin_sloth";
			break;
		case ItemType.Wrath:
			text = "sin_wrath";
			break;
		}
		foreach (Item item in from_item.inventory)
		{
			string text2 = item.id;
			if (item.id.Contains(":"))
			{
				text2 = text2.Split(':')[0];
			}
			if (text == text2)
			{
				return item;
			}
		}
		return null;
	}

	public void SetSinValuesInSkulls(bool is_sin_item_set = false, int red_skulls_sin = 0, int white_skulls_sin = 0, int red_skulls_organ = 0, int white_skulls_organ = 0)
	{
		_skull_bar.SetSkullValues(red_skulls_sin, white_skulls_sin, red_skulls_organ, white_skulls_organ);
	}

	public float GetHealRate()
	{
		return _skull_bar.GetSkullsFillRate();
	}

	public bool IsSinUnlocked(WorldGameObject wgo)
	{
		string param_name = string.Empty;
		switch (item_type)
		{
		case ItemType.Greed:
			param_name = "is_greed_unlocked";
			break;
		case ItemType.Wrath:
			param_name = "is_wrath_unlocked";
			break;
		case ItemType.Envy:
			param_name = "is_envy_unlocked";
			break;
		case ItemType.Pride:
			param_name = "is_pride_unlocked";
			break;
		case ItemType.Gluttony:
			param_name = "is_gluttony_unlocked";
			break;
		case ItemType.Sloth:
			param_name = "is_sloth_unlocked";
			break;
		case ItemType.Lust:
			param_name = "is_lust_unlocked";
			break;
		}
		return wgo.GetParam(param_name).EqualsTo(1f);
	}
}
