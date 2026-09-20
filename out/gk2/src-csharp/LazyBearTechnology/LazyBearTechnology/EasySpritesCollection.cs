using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "SpriteCollection", menuName = "Lazy/SpriteCollection")]
public class EasySpritesCollection : LazySingletonSO<EasySpritesCollection>
{
	public const string COLLECTION_SO_NAME = "SpriteCollection";

	public string addressablesSubDirectory = "Addressables";

	[SerializeField]
	private List<SpriteAtlasInfo> atlasInfoList = new List<SpriteAtlasInfo>();

	private Dictionary<string, LoadedSpriteAtlasData> loadedAtlasesDataDictionary = new Dictionary<string, LoadedSpriteAtlasData>();

	private Dictionary<string, string> hash = new Dictionary<string, string>();

	private static bool isInitialized = false;

	public static LazyLogType spriteLogType = LazyLogType.Warning;

	public static LazyLogType fallbackSpriteLogType = LazyLogType.Warning;

	public void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		isInitialized = true;
		foreach (SpriteAtlasInfo atlasInfo in atlasInfoList)
		{
			foreach (string spriteName in atlasInfo.spriteNames)
			{
				if (hash.ContainsKey(spriteName))
				{
					hash.TryGetValue(spriteName, out var value);
					DoLog("#sprites# Sprite " + spriteName + " from atlas " + atlasInfo.atlasName + " has duplicate (" + spriteName + " " + value, LazyLogType.Error);
				}
				else
				{
					hash.Add(spriteName, atlasInfo.atlasName);
				}
			}
		}
	}

	private void ScanAllAtlases()
	{
		atlasInfoList.Clear();
		string[] obj = new string[2] { ".spriteatlas", ".spriteatlasv2" };
		string text = addressablesSubDirectory;
		string text2 = "Assets/" + text;
		DoLog("#sprites# Start scanning \"" + text2 + "\" directory for SpriteAtlases");
		DirectoryInfo directoryInfo = new DirectoryInfo(text2);
		string[] array = obj;
		foreach (string text3 in array)
		{
			FileInfo[] files = directoryInfo.GetFiles("*" + text3, SearchOption.AllDirectories);
			foreach (FileInfo obj2 in files)
			{
				string text4 = obj2.Name.Split('.')[0];
				int num = obj2.Directory.FullName.LastIndexOf(text, StringComparison.Ordinal);
				if (num != -1)
				{
					num += text.Length;
				}
				string text5 = obj2.Directory.FullName.Substring(num) + "/" + text4;
				text5 = text5.Replace('\\', '/');
				if (text5.StartsWith("/"))
				{
					text5 = text5.Substring(1);
				}
				text5 += text3;
				text5 = text2 + "/" + text5;
				SpriteAtlas spriteAtlas = LoadSpriteAtlasForScan(text5);
				if (spriteAtlas == null)
				{
					DoLog("#sprites# Cannot load SpriteAtlas at path: " + text5, LazyLogType.Error);
					continue;
				}
				Sprite[] array2 = new Sprite[spriteAtlas.spriteCount];
				spriteAtlas.GetSprites(array2);
				DoLog($"#sprites# atlas found: {text4}, sprites: {spriteAtlas.spriteCount}");
				SpriteAtlasInfo spriteAtlasInfo = new SpriteAtlasInfo
				{
					atlasName = spriteAtlas.name,
					path = text5
				};
				Sprite[] array3 = array2;
				foreach (Sprite sprite in array3)
				{
					if (!(sprite == null))
					{
						string text6 = sprite.name.Replace("(Clone)", "");
						if (spriteAtlasInfo.spriteNames.Contains(text6))
						{
							DoLog("#sprites# Cannot add sprite [" + text6 + "] to atlasData [" + spriteAtlas.name + "]: name duplicate.", LazyLogType.Error);
						}
						else
						{
							spriteAtlasInfo.spriteNames.Add(text6);
						}
					}
				}
				Addressables.Release(spriteAtlas);
				atlasInfoList.Add(spriteAtlasInfo);
			}
		}
		DoLog($"#sprites# Scanning for SpriteAtlases is done. Total atlases: {atlasInfoList.Count}");
	}

	private SpriteAtlas LoadSpriteAtlasForScan(string path)
	{
		return Addressables.LoadAssetAsync<SpriteAtlas>(path).WaitForCompletion();
	}

	public bool HasSprite(string spriteName)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			return false;
		}
		if (!isInitialized)
		{
			DoLog("#sprites# EasySpritesCollection not initialized yet. Initializing...");
			Initialize();
		}
		return hash.ContainsKey(spriteName);
	}

	public Sprite GetSprite(string spriteName, string fallbackSpriteName = null)
	{
		if (!isInitialized)
		{
			DoLog("#sprites# EasySpritesCollection not initialized yet. Initializing...");
			Initialize();
		}
		if (hash.TryGetValue(spriteName, out var value))
		{
			return EnsureSpriteAtlasWithNameLoaded(value).GetSprite(spriteName);
		}
		if (!string.IsNullOrEmpty(fallbackSpriteName) && hash.TryGetValue(fallbackSpriteName, out value))
		{
			SpriteAtlas spriteAtlas = EnsureSpriteAtlasWithNameLoaded(value);
			if (fallbackSpriteLogType != 0)
			{
				DoLog("#sprites# Cannot find spriteName [" + spriteName + "] in hash, using fallbackSpriteName [" + fallbackSpriteName + "].", fallbackSpriteLogType);
			}
			return spriteAtlas.GetSprite(fallbackSpriteName);
		}
		if (spriteLogType != 0)
		{
			DoLog("#sprites# Cannot find spriteName [" + spriteName + "] in hash.", spriteLogType);
		}
		return null;
	}

	public SpriteAtlas EnsureSpriteAtlasWithNameLoaded(string atlasName)
	{
		if (loadedAtlasesDataDictionary.TryGetValue(atlasName, out var value))
		{
			return value.atlas;
		}
		foreach (SpriteAtlasInfo atlasInfo in atlasInfoList)
		{
			if (atlasInfo.atlasName == atlasName)
			{
				SpriteAtlas spriteAtlas = Addressables.LoadAssetAsync<SpriteAtlas>(atlasInfo.path).WaitForCompletion();
				if (loadedAtlasesDataDictionary.TryGetValue(spriteAtlas.name, out value))
				{
					Addressables.Release(spriteAtlas);
					return value.atlas;
				}
				value = new LoadedSpriteAtlasData
				{
					atlas = spriteAtlas
				};
				loadedAtlasesDataDictionary.Add(value.atlas.name, value);
				return value.atlas;
			}
		}
		DoLog("#sprites# Cannot find meta info about atlas with name: " + atlasName, LazyLogType.Error);
		return null;
	}

	public SpriteAtlas EnsureSpriteAtlasWithSpriteLoaded(string spriteName)
	{
		if (!isInitialized)
		{
			DoLog("#sprites# EasySpritesCollection not initialized yet. Initializing...");
			Initialize();
		}
		if (hash.TryGetValue(spriteName, out var value))
		{
			return EnsureSpriteAtlasWithNameLoaded(value);
		}
		DoLog("#sprites# Cannot find atlas for sprite name: " + spriteName, LazyLogType.Error);
		return null;
	}

	public void UnloadAtlas(string atlasName)
	{
		if (loadedAtlasesDataDictionary.TryGetValue(atlasName, out var value))
		{
			Addressables.Release(value.atlas);
			loadedAtlasesDataDictionary.Remove(atlasName);
		}
		else
		{
			DoLog("#sprites# Cannot unload atlas " + atlasName + ": atlas not loaded.", LazyLogType.Error);
		}
	}

	public void UnloadAtlases(bool includeProtected = false)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, LoadedSpriteAtlasData> item in loadedAtlasesDataDictionary)
		{
			if (!item.Value.isProtected || includeProtected)
			{
				Addressables.Release(item.Value.atlas);
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			loadedAtlasesDataDictionary.Remove(item2);
		}
	}

	public void ProtectAtlases()
	{
		foreach (KeyValuePair<string, LoadedSpriteAtlasData> item in loadedAtlasesDataDictionary)
		{
			item.Value.isProtected = true;
		}
	}

	public void UnprotectAtlases()
	{
		foreach (KeyValuePair<string, LoadedSpriteAtlasData> item in loadedAtlasesDataDictionary)
		{
			item.Value.isProtected = false;
		}
	}

	private void DoLog(string message, LazyLogType type = LazyLogType.Default)
	{
		switch (type)
		{
		case LazyLogType.Error:
			Debug.LogError(message);
			break;
		case LazyLogType.Warning:
			Debug.LogWarning(message);
			break;
		case LazyLogType.Default:
			Debug.Log(message);
			break;
		}
	}
}
