using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarWiget_Simplified : LazyWidget<ProgressBarWidget_SimplifiedData>
{
	private const int CellPixelWidth = 7;

	[SerializeField]
	private Slider greenBarSlider;

	[SerializeField]
	private LayoutElement greenBarLayoutElement;

	[SerializeField]
	private Slider redBarSlider;

	[SerializeField]
	private Image bgImage;

	public RectTransform GreenFillRect
	{
		get
		{
			if (!(greenBarSlider != null))
			{
				return null;
			}
			return greenBarSlider.fillRect;
		}
	}

	public void Apply(int cellCount, int greenValue, int redValue = 0)
	{
		if (data == null || data.CellCount != cellCount || data.GreenValue != greenValue || data.RedValue != redValue || !base.gameObject.activeSelf)
		{
			Draw(new ProgressBarWidget_SimplifiedData(cellCount, greenValue, redValue));
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		int num = Mathf.Max(0, data.CellCount);
		greenBarLayoutElement.preferredWidth = 7 * num;
		int num2 = Mathf.Max(1, num);
		greenBarSlider.maxValue = num2;
		greenBarSlider.value = Mathf.Clamp(data.GreenValue, 0, num);
		redBarSlider.maxValue = num2;
		redBarSlider.value = Mathf.Clamp(data.RedValue, 0, num);
		if (bgImage.TryGetComponent<ProgressCellAtlasSize>(out var component))
		{
			component.SetCellCount(num);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)greenBarLayoutElement.transform.parent);
	}

	protected override void TestDraw()
	{
		Draw(new ProgressBarWidget_SimplifiedData(6, 3, 2));
	}
}
