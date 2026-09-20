using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITalentLevelUpRevealedNotification : UIBaseNotification
{
	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public string TalentLevelUpId { get; set; }

	public override void Draw()
	{
		TalentLevelUpDef data = GameBalance.Me.GetData<TalentLevelUpDef>(TalentLevelUpId);
		PerkDef data2 = GameBalance.Me.GetData<PerkDef>(data.linkedPerk);
		perkIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data2.IconId, "i_p-artist");
		perkIcon.BlueColorReplace(toReplace);
		label.text = LLBase.L("ui_perks") + ": " + LLBase.L(data2.id);
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
