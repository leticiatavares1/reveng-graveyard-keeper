using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISimpleTextWithIconNotification : UIBaseNotification
{
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI label;

	public string Text { get; set; }

	public string LocalizationKey { get; set; }

	public string IconId { get; set; }

	public override void Draw()
	{
		label.text = Text;
		iconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(IconId);
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
