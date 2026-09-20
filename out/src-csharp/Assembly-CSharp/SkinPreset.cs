using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Skin Preset")]
public class SkinPreset : ScriptableObject
{
	public int head = -1;

	public int body = 300;

	public int mid;

	public int bot;

	public int backpack;

	public static SkinPreset Load(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		SkinPreset skinPreset = Resources.Load<SkinPreset>(id);
		if (skinPreset == null)
		{
			Debug.LogError("Couldn't load skin preset = " + id);
			return null;
		}
		return skinPreset;
	}
}
