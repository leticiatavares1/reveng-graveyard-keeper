using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "New Lazy Atlas Commons", menuName = "Lazy/Lazy Atlas Common Sprites", order = 1)]
public class LazyAtlasCommonSprites : ScriptableObject
{
	public List<Object> objects = new List<Object>();
}
