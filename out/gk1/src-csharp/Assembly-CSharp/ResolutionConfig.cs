using System.Collections.Generic;
using UnityEngine;

public class ResolutionConfig
{
	private static List<ResolutionConfig> _available_resolutions = new List<ResolutionConfig>();

	public int x;

	public int y;

	public int pixel_size = 2;

	public float large_gui_scale = 1f;

	public ResolutionConfig(int w, int h)
	{
		x = w;
		y = h;
	}

	public static ResolutionConfig GetResolutionConfigOrNull(int width, int height)
	{
		ResolutionConfig resolutionConfig = new ResolutionConfig(width, height);
		if (height < 900 || width < 1280)
		{
			resolutionConfig.large_gui_scale = (float)height / 900f;
			return resolutionConfig;
		}
		if (height <= 1440 && width <= 2560)
		{
			return resolutionConfig;
		}
		return null;
	}

	public bool IsHardwareSupported()
	{
		foreach (ResolutionConfig available_resolution in _available_resolutions)
		{
			if (available_resolution.x == x && available_resolution.y == y)
			{
				return true;
			}
		}
		Debug.LogWarning($"Resolution {x}x{y} is not supported.");
		return false;
	}

	public static void InitResolutions()
	{
		Debug.Log("InitResolutions");
		Resolution[] resolutions = Screen.resolutions;
		_available_resolutions.Clear();
		Resolution[] array = resolutions;
		for (int i = 0; i < array.Length; i++)
		{
			Resolution resolution = array[i];
			ResolutionConfig resolutionConfigOrNull = GetResolutionConfigOrNull(resolution.width, resolution.height);
			if (resolutionConfigOrNull == null)
			{
				Debug.Log($"Skipping: {resolution.width}x{resolution.height}");
				continue;
			}
			bool flag = false;
			foreach (ResolutionConfig available_resolution in _available_resolutions)
			{
				if (available_resolution.x == resolutionConfigOrNull.x && available_resolution.y == resolutionConfigOrNull.y)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_available_resolutions.Add(resolutionConfigOrNull);
				Debug.Log($"Available: {resolutionConfigOrNull.x}x{resolutionConfigOrNull.y}, scale = {resolutionConfigOrNull.pixel_size}");
			}
		}
		if (_available_resolutions.Count == 0)
		{
			Debug.LogError("No available resolutions were found!");
		}
	}

	public override string ToString()
	{
		return $"ResolutionConfig [{x}x{y}, pixel_size={pixel_size}]";
	}

	public string GetResolutionName()
	{
		string text = $"{x}x{y}";
		if ((double)large_gui_scale < 0.99)
		{
			text = "[ff5050]" + text + "[/c]";
		}
		return text;
	}

	public static string[] GetResolutionsStringArray()
	{
		List<string> list = new List<string>();
		foreach (ResolutionConfig available_resolution in _available_resolutions)
		{
			list.Add(available_resolution.GetResolutionName());
		}
		return list.ToArray();
	}

	public static ResolutionConfig GetResolutionByIndex(int i)
	{
		return _available_resolutions[i];
	}
}
