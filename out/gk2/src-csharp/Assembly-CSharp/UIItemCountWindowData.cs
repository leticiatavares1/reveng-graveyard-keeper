using System;
using LazyBearTechnology;

public class UIItemCountWindowData : LazyWidgetDataBase
{
	public delegate int PriceCalculateDelegate(int amount);

	public Item Item { get; set; }

	public int Min { get; set; }

	public int Max { get; set; }

	public int SnapStep { get; set; } = 1;


	public Action<int> OnConfirm { get; set; }

	public PriceCalculateDelegate PriceCalculateDel { get; set; }

	public bool IsForVendor { get; set; }

	public UIDialogWindowData.ButtonData OkBtnData { get; set; }

	public UIDialogWindowData.ButtonData BackBtnData { get; set; }
}
