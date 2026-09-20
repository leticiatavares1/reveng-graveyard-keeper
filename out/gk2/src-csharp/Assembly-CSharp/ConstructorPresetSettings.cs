using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/ConstructorPresetSettings", fileName = "ConstructorPresetSettings")]
public class ConstructorPresetSettings : ScriptableObject
{
	public string presetId;

	public List<ConstructorPresetLutSetting> lutSettings;
}
