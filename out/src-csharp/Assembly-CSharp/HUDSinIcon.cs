using UnityEngine;

public class HUDSinIcon : MonoBehaviour
{
	public UI2DSprite spr_back;

	public UI2DSprite spr_active;

	public Color glow_color;

	private Sins.SinType _sin_type;

	public void Init()
	{
	}

	public void Update()
	{
	}

	public void Draw(Sins.SinType sin_type, Sprite back, Sprite active, Color glow)
	{
		spr_back.sprite2D = back;
		spr_active.sprite2D = active;
		glow_color = glow;
		_sin_type = sin_type;
		Redraw();
	}

	public void Redraw()
	{
		spr_active.enabled = MainGame.me.save.GetSinState(_sin_type);
	}
}
