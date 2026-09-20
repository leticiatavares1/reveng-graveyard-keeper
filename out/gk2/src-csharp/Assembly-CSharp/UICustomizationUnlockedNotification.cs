using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICustomizationUnlockedNotification : UIBaseNotification
{
	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	public Item SourceItem { get; set; }

	public override void Draw()
	{
		itemIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(SourceItem.Definition.iconId);
		itemIcon.BlueColorReplace(toReplace);
		label.text = LLBase.L("tech_customization") + " " + LLBase.L(SourceItem.id);
		LazyAudio.PlayAndForget("unlock");
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
