using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiInventoryWindow : LazyWindow<UIMultiInventoryWindowData>
{
	[SerializeField]
	private MultiInventoryWidget multiInventoryWidget;

	public override void Redraw()
	{
		base.Redraw();
		multiInventoryWidget.Draw(data.MultiInventoryWidgetData);
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Hide()
	{
		base.Hide();
		multiInventoryWidget.Hide();
	}

	protected override void TestDraw()
	{
		Open(new UIMultiInventoryWindowData(MainGame.PlayerData, null, (Item _) => false));
	}
}
