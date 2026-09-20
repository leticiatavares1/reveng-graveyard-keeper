using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "New Lazy Atlas", menuName = "Lazy/Lazy Atlas", order = 1)]
public class LazyAtlas : ScriptableObject
{
	[Tooltip("List of images and folders included in the atlas.")]
	public List<Object> objects = new List<Object>();

	[Tooltip("Common objects that could be included into several atlases.")]
	public LazyAtlasCommonSprites commonObjects;

	public Texture2D texture;

	public TextureFormat textureFormat = TextureFormat.RGBA32;

	public FilterMode filterMode;

	public bool exportToTextMeshPro;

	public TMP_SpriteAsset tmpSpriteAtlas;

	public bool autoScaleNewElements = true;

	public bool autoScaleOldElements = true;

	[SerializeField]
	private List<LazyAtlasItemParams> customItemParams = new List<LazyAtlasItemParams>();

	public int edgeBleed;

	public bool fillFromTop;

	[SerializeField]
	private List<LazyAtlasItem> items = new List<LazyAtlasItem>();

	public List<LazyAtlasItem> Items => items;

	public LazyAtlasItem GetItemByName(string name)
	{
		foreach (LazyAtlasItem item in items)
		{
			if (item.name == name)
			{
				return item;
			}
		}
		return null;
	}

	public LazyAtlasItem GetItemByIndex(int index)
	{
		return items[index];
	}

	public int GetItemIndexByName(string name)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].name == name)
			{
				return i;
			}
		}
		return -1;
	}

	public LazyAtlasItem GetItemByItemNameHash(int itemNameHash)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].name.GetHashCode() == itemNameHash)
			{
				return items[i];
			}
		}
		return null;
	}
}
