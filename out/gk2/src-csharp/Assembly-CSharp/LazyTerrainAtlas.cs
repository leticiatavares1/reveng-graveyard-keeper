using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Lazy Terrain Atlas")]
public class LazyTerrainAtlas : ScriptableObject
{
	public const int GRID_SIZE = 48;

	public LazyAtlas[] atlases;

	public Texture2D[] textures;

	public List<LazyTerrainSpriteGroup> spriteGroups = new List<LazyTerrainSpriteGroup>();
}
