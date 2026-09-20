using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class PerkWidgetFull : PerkWidget
{
	[SerializeField]
	private TextMeshProUGUI perkName;

	[SerializeField]
	private TextMeshProUGUI perkDescription;

	[SerializeField]
	private Vector2 durationPosWhenBuff;

	[SerializeField]
	private Vector2 durationPosWhenDefault;

	public PerksWidgetSeparator NextItemSeparator { get; set; }

	public override void Redraw()
	{
		base.Redraw();
		perkName.text = LLBase.L(data.PerkData.id);
		perkDescription.text = LLBase.L(data.PerkData.id + "_d");
		if (data.PerkData.Definition.perkType == PerkType.Buff)
		{
			durationLabel.rectTransform.anchoredPosition = durationPosWhenBuff;
		}
		else
		{
			durationLabel.rectTransform.anchoredPosition = durationPosWhenDefault;
		}
	}
}
