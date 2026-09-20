using System.Collections.Generic;
using UnityEngine;

public class EasySpriteCollectionSub : ScriptableObject
{
	public int id = -1;

	public List<Sprite> sprites = new List<Sprite>();

	public List<string> sprite_names = new List<string>();

	protected bool hash_created;

	protected Dictionary<string, Sprite> hash = new Dictionary<string, Sprite>();

	private void CreateHash()
	{
		if (hash_created)
		{
			return;
		}
		hash.Clear();
		for (int i = 0; i < sprite_names.Count; i++)
		{
			if (!hash.ContainsKey(sprite_names[i]))
			{
				hash.Add(sprite_names[i], sprites[i]);
			}
		}
		hash_created = true;
	}

	public Sprite GetSprite(string spr_name)
	{
		if (!hash_created)
		{
			CreateHash();
		}
		if (!hash.ContainsKey(spr_name))
		{
			Debug.LogError("Sprite '" + spr_name + "' is not found in sub-collection!");
			return null;
		}
		return hash[spr_name];
	}
}
