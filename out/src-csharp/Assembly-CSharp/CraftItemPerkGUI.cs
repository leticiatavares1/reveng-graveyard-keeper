using UnityEngine;

public class CraftItemPerkGUI : MonoBehaviour
{
	public UI2DSprite icon;

	public Tooltip tooltip;

	public void DrawPerk(string perk_id)
	{
		bool flag = !string.IsNullOrEmpty(perk_id);
		base.gameObject.SetActive(flag);
		if (flag)
		{
			PerkDefinition data = GameBalance.me.GetData<PerkDefinition>(perk_id);
			icon.sprite2D = EasySpritesCollection.GetSprite(data.GetIcon());
			icon.alpha = (MainGame.me.save.unlocked_perks.Contains(perk_id) ? 1f : 0.25f);
			icon.MakePixelPerfect();
			tooltip.SetData(new BubbleWidgetTextData(GJL.L(perk_id), UITextStyles.TextStyle.HintTitle, NGUIText.Alignment.Left));
			string descriptionIfExists = data.GetDescriptionIfExists();
			if (!string.IsNullOrEmpty(descriptionIfExists))
			{
				tooltip.AddData(new BubbleWidgetTextData(descriptionIfExists, UITextStyles.TextStyle.TinyDescription, NGUIText.Alignment.Left));
			}
		}
	}

	public void DrawBuff(string buff_id)
	{
		BuffDefinition data = GameBalance.me.GetData<BuffDefinition>(buff_id);
		if (data == null)
		{
			Debug.LogError("Couldn't draw a null buff: " + buff_id);
			return;
		}
		base.gameObject.SetActive(value: true);
		icon.sprite2D = EasySpritesCollection.GetSprite(data.GetIconName());
		icon.alpha = 1f;
		icon.MakePixelPerfect();
		tooltip.SetData(new BubbleWidgetTextData(data.GetLocalizedName(), UITextStyles.TextStyle.HintTitle, NGUIText.Alignment.Left));
		string descriptionIfExists = data.GetDescriptionIfExists();
		if (!string.IsNullOrEmpty(descriptionIfExists))
		{
			tooltip.AddData(new BubbleWidgetTextData(descriptionIfExists, UITextStyles.TextStyle.TinyDescription, NGUIText.Alignment.Left));
		}
	}
}
