using System;
using UnityEngine;

public class NewsItemGUI : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private SimpleUITable _table;

	[HideInInspector]
	[SerializeField]
	private UIWidget[] _widgets;

	public UILabel version;

	public UILabel version_upcoming;

	public UILabel eta;

	public UILabel items;

	public UILabel release_date;

	public UI2DSprite upcoming_progress;

	public GameObject upcoming_go;

	public GameObject version_go;

	public void Init()
	{
		_table = GetComponent<SimpleUITable>();
		_widgets = GetComponentsInChildren<UIWidget>(includeInactive: true);
	}

	public void Draw(NewsItemData data)
	{
		upcoming_go.SetActive(data.is_upcoming);
		version_go.SetActive(!data.is_upcoming);
		if (data.is_upcoming)
		{
			version_upcoming.text = data.version;
			upcoming_progress.fillAmount = (float)data.progress / 100f;
			int num = (data.date - DateTime.UtcNow).Days;
			if (num < 1)
			{
				num = 1;
			}
			eta.text = "ETA: " + num + " d";
		}
		else
		{
			version.text = data.version;
			release_date.text = data.date.ToString("dd MMM yyyy");
		}
		items.text = data.items;
		_table.Reposition();
		UIWidget[] widgets = _widgets;
		foreach (UIWidget obj in widgets)
		{
			obj.Update();
			obj.UpdateAnchors();
		}
	}
}
