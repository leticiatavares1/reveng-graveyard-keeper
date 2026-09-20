using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeakerIdAliases", menuName = "ScriptableObjects/SpeakerIdAliases")]
public class SpeakerIdAliases : ScriptableObject
{
	[Serializable]
	private class AliasElement
	{
		public string key;

		public string value;
	}

	[SerializeField]
	private List<AliasElement> aliases = new List<AliasElement>();

	private static SpeakerIdAliases cachedInstance;

	public static SpeakerIdAliases Instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = Resources.Load<SpeakerIdAliases>("Locales/SpeakerIdAliases");
			}
			return cachedInstance;
		}
	}

	public static bool TryGetWgoId(string key, out string value)
	{
		value = Instance.aliases.Find((AliasElement x) => x.key == key)?.value;
		if (string.IsNullOrEmpty(value))
		{
			Debug.LogError("[SpeakerIdAliases]: there's no value for key '" + key + "'");
			return false;
		}
		return true;
	}
}
