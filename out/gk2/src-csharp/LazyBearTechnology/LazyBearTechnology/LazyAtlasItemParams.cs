using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class LazyAtlasItemParams
{
	[LazyAtlasTableCell]
	public string name;

	[LazyAtlasTableCell]
	public int offsetX;

	[LazyAtlasTableCell]
	public int offsetY;

	[Tooltip("Extra Advance")]
	[LazyAtlasTableCell]
	public int extraAdvance;
}
