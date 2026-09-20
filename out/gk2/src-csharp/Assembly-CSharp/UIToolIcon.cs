using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIToolIcon : MonoBehaviour
{
	[SerializeField]
	private Image toolIconImage;

	private float cachedSpacing = -1f;

	private HorizontalLayoutGroup horizontalLayoutGroup;

	public void Draw(ItemType itemType, bool isEquipped, TalentDef talentDef = null)
	{
		string text = (isEquipped ? "equipped" : "not_equipped");
		horizontalLayoutGroup = GetComponentInParent<HorizontalLayoutGroup>();
		if (horizontalLayoutGroup != null)
		{
			cachedSpacing = horizontalLayoutGroup.spacing;
			horizontalLayoutGroup.spacing = 0f;
		}
		toolIconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon_" + itemType.ToString().ToLower() + "_" + text);
		toolIconImage.SetNativeSize();
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (cachedSpacing > -1f && horizontalLayoutGroup != null)
		{
			horizontalLayoutGroup.spacing = cachedSpacing;
			cachedSpacing = -1f;
			horizontalLayoutGroup = null;
		}
	}
}
