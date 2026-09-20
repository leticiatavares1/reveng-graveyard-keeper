using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINpcWidget : LazyWidget<UINpcWidgetData>
{
	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI npcNameLabel;

	[SerializeField]
	private TextMeshProUGUI repLabel;

	[SerializeField]
	private TextMeshProUGUI repIcon;

	[SerializeField]
	private Slider progressBar;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public override void Redraw()
	{
		base.Redraw();
		portrait.sprite = data.WgoDef.Portrait;
		npcNameLabel.text = LLBase.L(data.WgoDef.id);
		int nPCRep = MainGame.Instance.GameSave.playerData.GetNPCRep(data.WgoDef.repResName);
		GameResIconConfig configForRes = GameResDisplayConfig.GetConfigForRes(data.WgoDef.repResName, GameResIconType.Common);
		if (configForRes == null)
		{
			repIcon.text = "icon_smile02".FontIcon();
		}
		else
		{
			repIcon.text = configForRes.iconName.FontIcon();
		}
		repLabel.text = nPCRep.ToString();
		progressBar.value = (float)nPCRep / 100f;
		base.transform.parent.gameObject.SetActive(value: true);
		portrait.BlueColorReplace(toReplace);
	}

	public override void Hide()
	{
		base.transform.parent.gameObject.SetActive(value: false);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		UINpcWidgetData uINpcWidgetData = new UINpcWidgetData();
		uINpcWidgetData.NpcId = "npc_larry";
		Draw(uINpcWidgetData);
	}
}
