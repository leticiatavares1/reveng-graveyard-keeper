using UnityEngine;

public static class PlatformUnityShadowPresets
{
	public static readonly PlatformUnityShadowPreset[] DebugPresetOrder = new PlatformUnityShadowPreset[7]
	{
		PlatformUnityShadowPreset.DesktopLike,
		PlatformUnityShadowPreset.Balanced,
		PlatformUnityShadowPreset.Performance,
		PlatformUnityShadowPreset.Low,
		PlatformUnityShadowPreset.Minimal,
		PlatformUnityShadowPreset.Off,
		PlatformUnityShadowPreset.Console
	};

	public static PlatformUnityShadowSettings Get(PlatformUnityShadowPreset preset)
	{
		return preset switch
		{
			PlatformUnityShadowPreset.DesktopLike => new PlatformUnityShadowSettings(ShadowQuality.All, ShadowResolution.High, 3000f, 4, LightShadows.Soft, 8), 
			PlatformUnityShadowPreset.Balanced => new PlatformUnityShadowSettings(ShadowQuality.HardOnly, ShadowResolution.Medium, 120f, 2, LightShadows.Hard, 8), 
			PlatformUnityShadowPreset.Performance => new PlatformUnityShadowSettings(ShadowQuality.HardOnly, ShadowResolution.Low, 80f, 2, LightShadows.Hard, 8), 
			PlatformUnityShadowPreset.Low => new PlatformUnityShadowSettings(ShadowQuality.HardOnly, ShadowResolution.Low, 50f, 0, LightShadows.Hard, 8), 
			PlatformUnityShadowPreset.Console => new PlatformUnityShadowSettings(ShadowQuality.All, ShadowResolution.Low, 50f, 0, LightShadows.Soft, 8, LightShadows.None), 
			PlatformUnityShadowPreset.Minimal => new PlatformUnityShadowSettings(ShadowQuality.HardOnly, ShadowResolution.Low, 30f, 0, LightShadows.Hard, 8), 
			PlatformUnityShadowPreset.Off => new PlatformUnityShadowSettings(ShadowQuality.Disable, ShadowResolution.Low, 0f, 0, LightShadows.None, 8), 
			_ => Get(PlatformUnityShadowPreset.Balanced), 
		};
	}

	public static string GetDisplayName(PlatformUnityShadowPreset preset)
	{
		return preset switch
		{
			PlatformUnityShadowPreset.DesktopLike => "Shadow Preset: Desktop", 
			PlatformUnityShadowPreset.Balanced => "Shadow Preset: Balanced", 
			PlatformUnityShadowPreset.Performance => "Shadow Preset: Performance", 
			PlatformUnityShadowPreset.Low => "Shadow Preset: Low", 
			PlatformUnityShadowPreset.Minimal => "Shadow Preset: Minimal", 
			PlatformUnityShadowPreset.Off => "Shadow Preset: Off", 
			PlatformUnityShadowPreset.Console => "Shadow Preset: Console", 
			_ => preset.ToString(), 
		};
	}

	public static int IndexOf(PlatformUnityShadowPreset preset)
	{
		for (int i = 0; i < DebugPresetOrder.Length; i++)
		{
			if (DebugPresetOrder[i] == preset)
			{
				return i;
			}
		}
		return 0;
	}
}
