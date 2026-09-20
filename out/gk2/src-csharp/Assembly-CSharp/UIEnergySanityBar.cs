using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergySanityBar : LazyWidget<UIEnergySanityBarData>
{
	[SerializeField]
	private Slider currentEnergySlider;

	[SerializeField]
	private Slider currentInsanitySlider;

	[SerializeField]
	private Image energyFillImage;

	[SerializeField]
	private Image insanityFillImage;

	[SerializeField]
	private Sprite defaultEnergyFillSprite;

	[SerializeField]
	private Sprite lowEnergyFillSprite;

	[SerializeField]
	private Sprite defaultInsanityFillSprite;

	[SerializeField]
	private Sprite lowInsanityFillSprite;

	[SerializeField]
	private GameObject energyBlick;

	[SerializeField]
	private GameObject insanityBlick;

	[SerializeField]
	private float lowBorderValue;

	[SerializeField]
	private float disableBorderValue = 0.058f;

	[SerializeField]
	private Transform energyIcon;

	[SerializeField]
	private Transform insanityIcon;

	public override void Init()
	{
		currentEnergySlider.value = 1f;
		currentInsanitySlider.value = 0f;
		AttachHudIconTooltips();
	}

	public void AttachHudIconTooltips()
	{
		if (energyIcon == null)
		{
			energyIcon = base.transform.Find("EnergyIcon");
		}
		if (insanityIcon == null)
		{
			insanityIcon = base.transform.Find("InsanityIcon");
		}
		if (energyIcon != null)
		{
			energyIcon.SetAsLastSibling();
			UIMouseTooltip.Attach(energyIcon.gameObject, "hud_energy", null, addRaycastTarget: true, disableChildRaycasts: false, UIMouseTooltipEdges.All(2f), default(Vector2), "energy");
		}
		if (insanityIcon != null)
		{
			insanityIcon.SetAsLastSibling();
			UIMouseTooltip.Attach(insanityIcon.gameObject, "hud_insanity", null, addRaycastTarget: true, disableChildRaycasts: false, UIMouseTooltipEdges.All(2f), default(Vector2), "insanity");
		}
	}

	protected override void SetData(UIEnergySanityBarData data)
	{
		base.SetData(data);
		data.onFillValueChanged = UpdateEnergyBars;
	}

	public override void Redraw()
	{
		base.Redraw();
		UpdateEnergyBars();
	}

	private void UpdateEnergyBars()
	{
		currentEnergySlider.value = data.EnergyFillValue;
		energyFillImage.sprite = ((currentEnergySlider.value > lowBorderValue) ? defaultEnergyFillSprite : lowEnergyFillSprite);
		currentInsanitySlider.value = data.InsanityFillValue;
		insanityFillImage.sprite = ((currentInsanitySlider.value > lowBorderValue) ? defaultInsanityFillSprite : lowInsanityFillSprite);
		energyBlick.gameObject.SetActive(currentEnergySlider.value > lowBorderValue);
		insanityBlick.gameObject.SetActive(currentInsanitySlider.value > lowBorderValue);
		if (currentEnergySlider.value <= 0f)
		{
			currentEnergySlider.value = 0f;
		}
		else if (currentEnergySlider.value <= disableBorderValue)
		{
			currentEnergySlider.value = disableBorderValue;
		}
		if (currentInsanitySlider.value <= 0f)
		{
			currentInsanitySlider.value = 0f;
		}
		else if (currentInsanitySlider.value <= disableBorderValue)
		{
			currentInsanitySlider.value = disableBorderValue;
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIEnergySanityBarData(MainGame.Instance.GameSave));
	}
}
