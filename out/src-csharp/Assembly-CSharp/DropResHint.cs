using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DropResHint : MonoBehaviour
{
	[HideInInspector]
	public UILabel label;

	private DropResGameObject _drop;

	private bool _enabled;

	private int _update_text_counter = 5;

	private bool _show_durability;

	private StringBuilder _sb = new StringBuilder();

	private static List<DropResHint> _list = new List<DropResHint>();

	public void Init()
	{
		label = GetComponentInChildren<UILabel>();
		base.gameObject.SetActive(value: false);
	}

	public static DropResHint Show(DropResGameObject drop_res, bool show_durability)
	{
		DropResHint dropResHint = GUIElements.me.drop_res_hint.Copy();
		dropResHint._drop = drop_res;
		dropResHint.gameObject.SetActive(value: true);
		dropResHint._enabled = true;
		dropResHint._show_durability = show_durability;
		dropResHint.UpdateText();
		_list.Add(dropResHint);
		return dropResHint;
	}

	public void LateUpdate()
	{
		if (_enabled)
		{
			base.transform.SetGUIPosToWorldPos(_drop.object_transform.position + new Vector3(0f, 30f, 0f), MainGame.me.world_cam, MainGame.me.gui_cam);
			if (--_update_text_counter <= 0)
			{
				_update_text_counter = 5;
				UpdateText();
			}
		}
	}

	private void UpdateText()
	{
		if (_enabled)
		{
			_sb.Length = 0;
			if (_show_durability)
			{
				_sb.Append("(hp)");
				_sb.Append(Item.FloatNumberToPercentString(_drop.res.durability));
			}
			label.text = _sb.ToString();
			if (!string.IsNullOrEmpty(_drop.res.sub_name))
			{
				label.text = _drop.res.sub_name + "\n" + label.text;
			}
		}
	}

	public void DestroyMe()
	{
		Object.Destroy(base.gameObject);
		_list.Remove(this);
		_enabled = false;
	}

	public static void DestroyAll()
	{
		while (_list.Count > 0)
		{
			_list[0].DestroyMe();
		}
	}
}
