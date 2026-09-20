public class HpBarSimpleWidgetData : HpBarWidgetData
{
	public enum SpriteType
	{
		Ally = 1,
		Enemy
	}

	public const float DEFAULT_WIDTH = 30f;

	public const float DEFAULT_HEIGHT = 5f;

	public float CustomWidth { get; set; } = 30f;


	public float CustomHeight { get; set; } = 5f;


	public SpriteType Sprite { get; set; } = SpriteType.Ally;


	public HpBarSimpleWidgetData(HPComponent hpComponent, float customWidth = 30f, float customHeight = 5f, SpriteType spriteType = SpriteType.Ally)
		: base(hpComponent)
	{
		CustomWidth = customWidth;
		CustomHeight = customHeight;
		Sprite = spriteType;
	}
}
