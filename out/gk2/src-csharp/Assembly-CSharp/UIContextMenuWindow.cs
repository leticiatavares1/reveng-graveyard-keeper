using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIContextMenuWindow : LazyWindow<UIContextMenuWindowData>
{
	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private LazyButton backgroundBtn;

	[SerializeField]
	private UIContextMenuWindowWidget contextMenuWindowWidgetPrefab;

	private Pool pool;

	private List<UIContextMenuWindowWidget> widgets = new List<UIContextMenuWindowWidget>();

	public override void Init()
	{
		base.Init();
		pool = LazyPooler.CreatePool(contextMenuWindowWidgetPrefab);
		contextMenuWindowWidgetPrefab.gameObject.SetActive(value: false);
		backgroundBtn.onClick.AddListener(Close);
	}

	public override void Redraw()
	{
		base.Redraw();
		foreach (UIContextMenuWindowWidget widget in widgets)
		{
			pool.ReleaseObject(widget);
		}
		widgets.Clear();
		for (int i = 0; i < data.Options.Count; i++)
		{
			UIContextMenuWindowWidget orCreateObject = pool.GetOrCreateObject<UIContextMenuWindowWidget>();
			orCreateObject.Draw(data.Options[i]);
			orCreateObject.transform.SetParent(content);
			orCreateObject.gameObject.SetActive(value: true);
			orCreateObject.transform.SetSiblingIndex(i);
			widgets.Add(orCreateObject);
		}
		content.RefreshContentFitter();
		content.position = data.Position;
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
	}

	public override void Close()
	{
		if (base.IsShown)
		{
			base.Close();
		}
	}

	protected override bool OnPressedBack()
	{
		Close();
		return true;
	}

	protected override void TestDraw()
	{
		UIContextMenuWindowData uIContextMenuWindowData = new UIContextMenuWindowData();
		uIContextMenuWindowData.Options = new List<UIContextMenuWindowWidgetData>();
		uIContextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData("test1", delegate
		{
			Debug.Log("test1 pressed");
		}));
		uIContextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData("test2", delegate
		{
			Debug.Log("test2 pressed");
		}));
		Open(uIContextMenuWindowData);
	}
}
