using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "GenericFightersPresets", menuName = "GK2/GenericFightersPresets")]
public class GenericFightersPresets : ScriptableObject
{
	[Serializable]
	public class GenericFightersPreset
	{
		public int tier;

		public List<string> weaponsIds = new List<string>();

		public List<string> armorsIds = new List<string>();
	}

	private const string PRESETS_FOLDER = "Fighting";

	public List<GenericFightersPreset> genericFightersPresets = new List<GenericFightersPreset>();

	public static AsyncOperationHandle TryLoad()
	{
		return Addressables.LoadAssetAsync<GenericFightersPresets>("Fighting/GenericFightersPresets.asset");
	}
}
