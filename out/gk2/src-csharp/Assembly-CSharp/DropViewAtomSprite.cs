using LazyBearTechnology;
using UnityEngine;

public class DropViewAtomSprite : DropViewAtomBase
{
	private static readonly int replaceBlueColorId = Shader.PropertyToID("_ReplaceBlueColor");

	[SerializeField]
	private SpriteRenderer itemSprite;

	[SerializeField]
	private SpriteText spriteText;

	private MaterialPropertyBlock propertyBlock;

	public override void Activate(string iconId)
	{
		itemSprite.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(iconId);
		base.Activate(iconId);
	}

	public override SpriteText GetSpriteText()
	{
		return spriteText;
	}

	public override void Deactivate()
	{
		SetInteractionState(isUnderInteraction: false);
		base.Deactivate();
		itemSprite.sprite = null;
	}

	public override void SetInteractionState(bool isUnderInteraction)
	{
		if (!(itemSprite == null))
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			itemSprite.GetPropertyBlock(propertyBlock);
			propertyBlock.SetColor(replaceBlueColorId, isUnderInteraction ? Object3DMesh.replaceBlueColor : GetDefaultReplaceBlueColor());
			itemSprite.SetPropertyBlock(propertyBlock);
		}
	}

	private Color GetDefaultReplaceBlueColor()
	{
		Material sharedMaterial = itemSprite.sharedMaterial;
		if (sharedMaterial != null && sharedMaterial.HasProperty(replaceBlueColorId))
		{
			return sharedMaterial.GetColor(replaceBlueColorId);
		}
		return Object3DMesh.replaceBlueTransparent;
	}
}
