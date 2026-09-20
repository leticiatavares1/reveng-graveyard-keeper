using UnityEngine;

public class FishingDistanceChoosingBarItemGUI : MonoBehaviour
{
	public UI2DSprite icon;

	public UILabel label;

	public void SetFishItem(string sprite_name, int chance)
	{
		icon.sprite2D = EasySpritesCollection.GetSprite(sprite_name);
		label.text = chance + "%";
	}
}
