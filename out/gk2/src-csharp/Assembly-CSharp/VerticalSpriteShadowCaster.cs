public class VerticalSpriteShadowCaster : VerticalSprite
{
	protected override void Awake()
	{
		invisibleSprite = true;
		castShadows = true;
		base.Awake();
		ApplyMaterial();
	}
}
