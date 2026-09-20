using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeformingGrassAtlas", menuName = "HP/Deforming Grass Atlas", order = 1)]
public class DeformingGrassAtlas : ScriptableObject
{
	public Texture2D texture;

	[HideInInspector]
	public List<int> variations = new List<int>();
}
