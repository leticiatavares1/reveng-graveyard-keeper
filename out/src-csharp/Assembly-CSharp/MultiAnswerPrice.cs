using UnityEngine;

public class MultiAnswerPrice : MonoBehaviour
{
	public UI2DSprite back;

	public UI2DSprite price_icon;

	public UIWidget price_widget;

	public UILabel price_label_n;

	public UI2DSprite price_quality_icon;

	public UIWidget price_lock_available;

	public UIWidget price_lock_locked;

	public Sprite default_back_spr;

	public Sprite second_back_spr;

	public void SetBack(bool default_back)
	{
		back.sprite2D = (default_back ? default_back_spr : second_back_spr);
	}
}
