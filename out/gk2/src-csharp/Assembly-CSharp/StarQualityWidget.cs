using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarQualityWidget : LazyWidget<StarQualityWidgetData>
{
	[SerializeField]
	private Slider bronzeChanceSlider;

	[SerializeField]
	private Slider silverChanceSlider;

	[SerializeField]
	private Slider goldChanceSlider;

	[SerializeField]
	private TextMeshProUGUI silverChanceField;

	[SerializeField]
	private TextMeshProUGUI goldChanceField;

	public override void Redraw()
	{
		base.Redraw();
		RedrawQualityBar();
	}

	private void RedrawQualityBar()
	{
		CraftParamsData craftParams = data.CraftParams;
		silverChanceField.text = string.Empty;
		goldChanceField.text = string.Empty;
		bronzeChanceSlider.value = 0f;
		silverChanceSlider.value = 0f;
		goldChanceSlider.value = 0f;
		if (craftParams.total <= 1f)
		{
			bronzeChanceSlider.value = 1f;
		}
		else if (craftParams.total > 1f && craftParams.total <= 2f)
		{
			float num = Mathf.RoundToInt(craftParams.total * 100f - 100f);
			silverChanceField.text = num.ToInvariantCultureString() + "%";
			bronzeChanceSlider.value = 1f;
			silverChanceSlider.value = num / 100f;
		}
		else if (craftParams.total > 2f && craftParams.total <= 3f)
		{
			float num = Mathf.RoundToInt(craftParams.total * 100f - 200f);
			silverChanceField.text = 100 + "%";
			goldChanceField.text = num.ToInvariantCultureString() + "%";
			bronzeChanceSlider.value = 1f;
			silverChanceSlider.value = 1f;
			goldChanceSlider.value = num / 100f;
		}
		else if (craftParams.total > 3f)
		{
			silverChanceField.text = 100 + "%";
			goldChanceField.text = 100 + "%";
			bronzeChanceSlider.value = 1f;
			silverChanceSlider.value = 1f;
			goldChanceSlider.value = 1f;
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new StarQualityWidgetData(new CraftParamsData("grape_wine", new WgoData("test_oven", Vector3.zero))));
	}
}
