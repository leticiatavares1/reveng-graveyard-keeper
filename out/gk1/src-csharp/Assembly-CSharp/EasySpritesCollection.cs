using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "sprite_collection", menuName = "Sprite collection")]
public class EasySpritesCollection : ScriptableObject
{
	[HideInInspector]
	public List<Sprite> sprites = new List<Sprite>();

	[HideInInspector]
	public List<string> sprite_names = new List<string>();

	public ESCFolderSplitter folder_splitter;

	protected static Dictionary<string, Sprite> hash = new Dictionary<string, Sprite>();

	protected static Dictionary<string, int> hash_sub = new Dictionary<string, int>();

	protected static Dictionary<string, string> hash_lower_case = new Dictionary<string, string>();

	protected static bool hash_created = false;

	[Space(10f)]
	public List<string> folders = new List<string>();

	private static List<EasySpritesCollection> _all_collections = new List<EasySpritesCollection>();

	private static Dictionary<int, EasySpriteCollectionSub> _loaded_sub_collections = new Dictionary<int, EasySpriteCollectionSub>();

	[Space(10f)]
	public List<string> sub_sprite_names = new List<string>();

	[Space(10f)]
	public List<int> sub_group_ids = new List<int>();

	private int _total_sprites_for_inspector;

	private static bool _all_atlases_started_loading = false;

	public static int total_collections => _all_collections.Count;

	public static void Load(string filename = "sprite_collection")
	{
		EasySpritesCollection easySpritesCollection = Resources.Load<EasySpritesCollection>(filename);
		if (easySpritesCollection == null)
		{
			Debug.LogError("Error loading sprite collection: " + filename);
			return;
		}
		if (!_all_collections.Contains(easySpritesCollection))
		{
			_all_collections.Add(easySpritesCollection);
		}
		hash_created = false;
		CreateHash();
	}

	private static void CreateHash()
	{
		if (hash_created)
		{
			return;
		}
		Debug.Log("CreateHash");
		hash.Clear();
		foreach (EasySpritesCollection all_collection in _all_collections)
		{
			for (int i = 0; i < all_collection.sprite_names.Count; i++)
			{
				if (!hash.ContainsKey(all_collection.sprite_names[i]))
				{
					hash.Add(all_collection.sprite_names[i], all_collection.sprites[i]);
				}
			}
			for (int j = 0; j < all_collection.sub_sprite_names.Count; j++)
			{
				if (!hash_sub.ContainsKey(all_collection.sub_sprite_names[j]))
				{
					hash_sub.Add(all_collection.sub_sprite_names[j], all_collection.sub_group_ids[j]);
				}
			}
		}
		hash_created = true;
	}

	public static Sprite GetSprite(string sprite_name, bool not_found_is_valid = false, string sprite_if_not_found = "")
	{
		if (!hash_created)
		{
			CreateHash();
		}
		string value;
		if (string.IsNullOrEmpty(sprite_name))
		{
			value = string.Empty;
		}
		else if (!hash_lower_case.TryGetValue(sprite_name, out value))
		{
			value = sprite_name.ToLower();
			hash_lower_case.Add(sprite_name, value);
		}
		if (!hash.ContainsKey(value))
		{
			if (!hash_sub.ContainsKey(value))
			{
				if (not_found_is_valid)
				{
					if (!string.IsNullOrEmpty(sprite_if_not_found))
					{
						return GetSprite(sprite_if_not_found);
					}
					return null;
				}
				Debug.LogError("Sprite '" + value + "(" + sprite_name + ")' is not found in collections! (collections: " + _all_collections.Count + ", total size: " + hash.Count + ")");
				return null;
			}
			int key = hash_sub[value];
			if (!_loaded_sub_collections.ContainsKey(key))
			{
				Debug.Log("Loading sprite sub-collection id = " + key);
				EasySpriteCollectionSub easySpriteCollectionSub = Resources.Load<EasySpriteCollectionSub>("SpriteSubCollections/esc_subc_" + key);
				if (easySpriteCollectionSub == null)
				{
					Debug.LogError("Couldn't load sprite sub-collection, id = " + key);
					return null;
				}
				_loaded_sub_collections.Add(key, easySpriteCollectionSub);
				return easySpriteCollectionSub.GetSprite(value);
			}
			return _loaded_sub_collections[key].GetSprite(value);
		}
		return hash[value];
	}

	public static bool SetSpriteOrDisableGameObject(UI2DSprite ngui_spr, string sprite_name)
	{
		if (string.IsNullOrEmpty(sprite_name))
		{
			ngui_spr.gameObject.SetActive(value: false);
			return false;
		}
		ngui_spr.gameObject.SetActive(value: true);
		ngui_spr.sprite2D = GetSprite(sprite_name);
		return true;
	}

	public static void LoadAllAtlasesAsync(string atlases_list_name = "sprite_collection_atlases")
	{
		if (_all_atlases_started_loading)
		{
			return;
		}
		_all_atlases_started_loading = true;
		if (_all_collections.Count == 0)
		{
			Debug.LogError("_all_collections is zero length");
			return;
		}
		Debug.Log("LoadAllAtlasesAsync " + atlases_list_name);
		EasySpriteCollectionAtlases easySpriteCollectionAtlases = Resources.Load<EasySpriteCollectionAtlases>(atlases_list_name);
		if (easySpriteCollectionAtlases == null)
		{
			Debug.LogError("Couldn't load atlases: " + atlases_list_name);
			return;
		}
		foreach (string atlases_name in easySpriteCollectionAtlases.atlases_names)
		{
			ResourceRequest rq = Resources.LoadAsync<SpriteAtlas>(atlases_name);
			EasySpriteCollectionManager.StartTrackingResourceRequest(atlases_name, rq, _all_collections[0].OnAtlasLoaded);
		}
	}

	private void OnAtlasLoaded(Object atlas)
	{
		SpriteAtlas spriteAtlas = atlas as SpriteAtlas;
		if (spriteAtlas == null)
		{
			Debug.LogError("OnAtlasLoaded: atlas is null");
			return;
		}
		Debug.Log("OnAtlasLoaded " + spriteAtlas.name + ", sprites: " + spriteAtlas.spriteCount, spriteAtlas);
		Sprite[] array = new Sprite[spriteAtlas.spriteCount];
		spriteAtlas.GetSprites(array);
		Sprite[] array2 = array;
		foreach (Sprite sprite in array2)
		{
			string text = sprite.name.Replace("(Clone)", "").ToLower();
			if (hash.ContainsKey(text))
			{
				Debug.LogWarning("Error adding sprite to a library - duplicate name: " + text);
			}
			else
			{
				hash.Add(text, sprite);
			}
		}
	}
}
