using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldZoneWidget : LazyWidget<WorldZoneWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI worldZoneLabel;

	[SerializeField]
	private TextMeshProUGUI townSubZoneLabel;

	[SerializeField]
	private RectTransform townSubZoneParent;

	[SerializeField]
	private Image subTownImage;

	[SerializeField]
	private Sprite subTownSpr1;

	[SerializeField]
	private Sprite subTownSpr2;

	[SerializeField]
	private float subTownImageSizeBorder = 20f;

	[SerializeField]
	private TextStyle qualityEnoughStyle;

	[SerializeField]
	private TextStyle qualityNotEnoughStyle;

	public override void Redraw()
	{
		bool flag = data.WorldZoneData != null;
		bool insideTown = data.InsideTown;
		townSubZoneParent.gameObject.SetActive(value: false);
		worldZoneLabel.text = "";
		worldZoneLabel.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		if (insideTown)
		{
			bool num = data.TownSubZone != null;
			base.gameObject.SetActive(value: true);
			string text = "reputation-citizens".FontIcon() + "\u2060" + qualityEnoughStyle.ApplyStyleToString($"{MainGame.Instance.GameSave.townSystem.Quality}");
			worldZoneLabel.SetText(LLBase.L("town_zone") + " " + text);
			worldZoneLabel.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
			if (num)
			{
				townSubZoneParent.gameObject.SetActive(value: true);
				townSubZoneLabel.text = LLBase.L(data.TownSubZone.id);
			}
			((RectTransform)base.transform).RefreshContentFitter();
			if (num)
			{
				if (townSubZoneParent.sizeDelta.y > subTownImageSizeBorder)
				{
					subTownImage.sprite = subTownSpr2;
				}
				else
				{
					subTownImage.sprite = subTownSpr1;
				}
			}
			return;
		}
		WorldZoneDef.DisplayType displayType = data.WorldZoneData?.Definition.displayType ?? WorldZoneDef.DisplayType.None;
		if (flag && displayType != WorldZoneDef.DisplayType.Hidden)
		{
			base.gameObject.SetActive(value: true);
			string text2 = LLBase.L("wz_" + data.WorldZoneData.id);
			if (data.WorldZoneData.IsContainer && (!(data.WorldZoneData.id == "resurrection") || MainGame.PlayerData.GetResInt("zombies_limit_mechanic") != 0) && displayType != 0)
			{
				text2 += " ";
				text2 = ((!(data.WorldZoneData.GetTotalQuality() >= 0f)) ? (text2 + data.WorldZoneData.GetQualityString(qualityNotEnoughStyle)) : (text2 + data.WorldZoneData.GetQualityString(qualityEnoughStyle)));
			}
			worldZoneLabel.SetText(text2);
			worldZoneLabel.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new WorldZoneWidgetData(new WorldZoneData("493_dev_playground_test", "PortArea", Vector3.zero, default(Rect))));
	}
}
