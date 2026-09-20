using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspirationWidgetFinished : LazyWidget<InspirationWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI idLabel;

	[SerializeField]
	private TextMeshProUGUI descriptionLabel;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image iconFrame;

	[SerializeField]
	private Color iconOutlineColor;

	[SerializeField]
	private Sprite[] framesSprites;

	public string IdWithoutLevel => data?.IdWithoutLevel;

	public int Level => data?.CurrentLevel ?? 0;

	public override void Redraw()
	{
		if (data?.InspirationDef != null)
		{
			InspirationDef inspirationDef = data.InspirationDef;
			idLabel.text = LLBase.L(inspirationDef.id);
			descriptionLabel.text = LLBase.L(inspirationDef.id + "_d");
			icon.sprite = inspirationDef.Icon;
			icon.BlueColorReplace(iconOutlineColor);
			int num = data.CurrentLevelFrame - 1;
			if (framesSprites != null && num >= 0 && num < framesSprites.Length)
			{
				iconFrame.sprite = framesSprites[num];
			}
		}
	}

	protected override void TestDraw()
	{
	}
}
