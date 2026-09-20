using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITechRevealedNotification : UIBaseNotification
{
	[SerializeField]
	private Image tabIcon;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public string TechId { get; set; }

	public bool IsTechUnlocked { get; set; }

	public override void Draw()
	{
		TechDef data = GameBalance.Me.GetData<TechDef>(TechId);
		string spriteName = $"tech_tab_{data.tab}";
		tabIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName, "i_tech_tree_tab_placeholder");
		tabIcon.BlueColorReplace(toReplace);
		label.text = (IsTechUnlocked ? (LLBase.L("ui_notification_tech_added") + " " + LLBase.L(TechId)) : (LLBase.L("ui_notification_tech_revealed") + " " + LLBase.L(TechId)));
		if (IsTechUnlocked)
		{
			LazyAudio.PlayAndForget("unlock");
		}
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
