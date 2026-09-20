using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITechRepUnlockedNotification : UIBaseNotification
{
	[SerializeField]
	private Image tabIcon;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public string TechId { get; set; }

	public override void Draw()
	{
		TechDef data = GameBalance.Me.GetData<TechDef>(TechId);
		string spriteName = $"tech_tab_{data.tab}";
		tabIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName, "i_tech_tree_tab_placeholder");
		tabIcon.BlueColorReplace(toReplace);
		label.text = LLBase.L("ui_notification_tech_disponible") + " " + LLBase.L(TechId);
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
