using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionHintRow : LazyWidget<UIInteractionHintRowWidgetData>
{
	public TextMeshProUGUI label;

	public LayoutElement labelLayoutElement;

	public float heightGamepad = 16f;

	public float heightKeyboard = 12f;

	public UIToolIcon toolIcon;

	public UITalentIcon talentIcon;

	public Image customIcon;

	public override void Redraw()
	{
		InteractionInfo interactionInfo = data.InteractionInfo;
		customIcon.gameObject.SetActive(value: false);
		label.text = ((interactionInfo.isEnoughMastery && interactionInfo.isItemEquipped) ? interactionInfo.text : "icon_lock".FontIcon());
		if (interactionInfo.assignedTalent != null)
		{
			if (!interactionInfo.isItemEquipped)
			{
				toolIcon.Draw(interactionInfo.equippedItemType, interactionInfo.isItemEquipped, interactionInfo.assignedTalent);
				talentIcon.Hide();
			}
			else
			{
				talentIcon.Draw(interactionInfo.assignedTalent, interactionInfo.masteryLock, interactionInfo.isEnoughMastery);
				toolIcon.Hide();
			}
		}
		else
		{
			talentIcon.Hide();
			if (interactionInfo.equippedItemType != 0)
			{
				toolIcon.Draw(interactionInfo.equippedItemType, interactionInfo.isItemEquipped, interactionInfo.assignedTalent);
			}
			else
			{
				toolIcon.Hide();
			}
		}
		if (!string.IsNullOrEmpty(interactionInfo.customIconId))
		{
			Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(interactionInfo.customIconId);
			if (sprite != null)
			{
				customIcon.sprite = sprite;
				customIcon.SetNativeSize();
				customIcon.gameObject.SetActive(value: true);
			}
		}
		if (labelLayoutElement != null)
		{
			if (LazyInput.IsGamepadActive)
			{
				labelLayoutElement.preferredHeight = heightGamepad;
			}
			else
			{
				labelLayoutElement.preferredHeight = heightKeyboard;
			}
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIInteractionHintRowWidgetData(new InteractionInfo(LLBase.L("hint_interaction"))));
	}
}
