using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UINewBodyNotification : UIBaseNotification
{
	[SerializeField]
	private Image iconImage;

	public ItemDef ItemDef { get; set; }

	public override void Draw()
	{
		iconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(ItemDef.iconId, "i_body");
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
