using System;
using System.Collections.Generic;

[Serializable]
public class LazyTerrainSpriteGroup
{
	public string name;

	public List<LazyTerrainSpriteDefinition> sprites = new List<LazyTerrainSpriteDefinition>();

	[NonSerialized]
	private LazyTerrainSpriteDefinition defaultSprite;

	public LazyTerrainSpriteDefinition GetDefaultSprite()
	{
		if (defaultSprite == null)
		{
			foreach (LazyTerrainSpriteDefinition sprite in sprites)
			{
				if (sprite.type == "C")
				{
					defaultSprite = sprite;
					return sprite;
				}
			}
			defaultSprite = sprites[0];
		}
		return defaultSprite;
	}
}
