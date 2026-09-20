using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class VendorProgressWidget : LazyWidget<VendorProgressWidgetData>
{
	[SerializeField]
	private Slider progressBar;

	public override void Redraw()
	{
		base.Redraw();
		progressBar.value = data.Progress;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		VendorProgressWidgetData vendorProgressWidgetData = new VendorProgressWidgetData();
		vendorProgressWidgetData.Progress = 0.5f;
		Draw(vendorProgressWidgetData);
	}
}
