using UnityEngine;

public class DropCollectItem : MonoBehaviour
{
	public const float LIFETIME = 3.5f;

	public float time;

	public string item_id;

	public int n;

	private ItemDefinition _definition;

	public UILabel txt_num;

	public BaseItemCellGUI item_icon;

	private bool _is_tech_point;

	private bool _is_money;

	public UILabel txt_right;

	public bool show_counter_if_one;

	public bool show_counter_x;

	private void Redraw()
	{
		_is_tech_point = TechDefinition.TECH_POINTS.Contains(item_id);
		_is_money = item_id == "money";
		item_icon.gameObject.SetActive(value: true);
		txt_right.gameObject.SetActive(value: false);
		if (!_is_tech_point && !_is_money)
		{
			_definition = GameBalance.me.GetData<ItemDefinition>(item_id);
			if (_definition == null)
			{
				Debug.LogError("Item definition not found in balance: " + item_id, this);
				return;
			}
		}
		if (!_is_money)
		{
			item_icon.DrawIcon(_definition.GetIcon());
		}
		GJL.EnsureLabelHasCorrectFont(item_icon.item_name, do_cache: false);
		if (_is_tech_point)
		{
			item_icon.item_name.text = GJL.L("tech_point_" + item_id);
		}
		else if (_is_money)
		{
			item_icon.gameObject.SetActive(value: false);
			txt_right.gameObject.SetActive(value: true);
			txt_right.text = Trading.FormatMoney((float)n / 100f, print_zero: true, use_spaces: false);
			item_icon.item_name.text = GJL.L("Money") + "     ";
		}
		else
		{
			item_icon.item_name.text = _definition.GetItemName();
		}
		if (!_is_money)
		{
			_definition.TryDrawQualityOrDisableGameObject(item_icon.quality_icon);
			if (!show_counter_if_one && n == 1)
			{
				txt_num.text = "";
			}
			else
			{
				txt_num.text = (show_counter_x ? "x" : "") + n;
			}
		}
	}

	public void AddMoreItems(int amount)
	{
		time = 0f;
		n += amount;
		Redraw();
	}

	public void Draw(Item i)
	{
		item_id = i.id;
		n = i.value;
		Redraw();
		DropCollectGUI.RedrawGrid(base.transform);
	}

	public void Update()
	{
		time += Time.deltaTime;
		if (time > 3.5f)
		{
			DropCollectGUI.Despawn(this);
		}
	}
}
