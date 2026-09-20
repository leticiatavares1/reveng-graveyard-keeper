using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ResolutionConfig
{
	public const int MIN_SCREEN_WIDTH = 1280;

	public const int MIN_SCREEN_HEIGHT = 720;

	public const int MAX_SCREEN_WIDTH = 5120;

	public static ResolutionConfig currentResolution;

	[SerializeField]
	private int width;

	[SerializeField]
	private int height;

	[SerializeField]
	private int pixelSize;

	[SerializeField]
	private string customAdditinalString;

	[SerializeField]
	private UIWindowSizeType windowSizeType;

	[SerializeField]
	private bool useMainMenuScaleX2;

	[SerializeField]
	private bool isFakeResolution;

	[SerializeField]
	private int fakeWidth;

	[SerializeField]
	private int fakeHeight;

	private static List<ResolutionConfig> availableResolutions = new List<ResolutionConfig>();

	private static List<ResolutionConfig> hardcodedResolutions = new List<ResolutionConfig>
	{
		new ResolutionConfig(1920, 1080, 2),
		new ResolutionConfig(2560, 1440, 2, UIWindowSizeType.Big, "(x2)", useMainMenuScaleX2: true),
		new ResolutionConfig(2560, 1440, 2, UIWindowSizeType.Big, "(x3)", useMainMenuScaleX2: true, isFakeResolution: true, 1920, 1080),
		new ResolutionConfig(3840, 2160, 4),
		new ResolutionConfig(1280, 720, 2, UIWindowSizeType.Small),
		new ResolutionConfig(1280, 800, 2, UIWindowSizeType.Small),
		new ResolutionConfig(2560, 1600, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1366, 768, 2, UIWindowSizeType.Small),
		new ResolutionConfig(3440, 1440, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1920, 1200, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1600, 900, 2),
		new ResolutionConfig(1360, 768, 2),
		new ResolutionConfig(1440, 900, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1680, 1050, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(2880, 1800, 3, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1280, 1024, 2, UIWindowSizeType.Small, "", useMainMenuScaleX2: true),
		new ResolutionConfig(5120, 1440, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1920, 1440, 2, UIWindowSizeType.Big, "(x2)", useMainMenuScaleX2: true),
		new ResolutionConfig(1920, 1440, 2, UIWindowSizeType.Big, "(x3)", useMainMenuScaleX2: true, isFakeResolution: true, 1440, 1080),
		new ResolutionConfig(1280, 768, 2, UIWindowSizeType.Small, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1280, 960, 2, UIWindowSizeType.Small, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1440, 1080, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1440, 1080, 2, UIWindowSizeType.Big, "", useMainMenuScaleX2: true),
		new ResolutionConfig(1600, 1024, 2),
		new ResolutionConfig(1600, 1200, 2)
	};

	private static bool isInitialized;

	public static int Height
	{
		get
		{
			if (currentResolution == null)
			{
				return Screen.height / DevUtils.PixelSize;
			}
			return currentResolution.AppliedHeight / currentResolution.pixelSize;
		}
	}

	public static int Width
	{
		get
		{
			if (currentResolution == null)
			{
				return Screen.width / DevUtils.PixelSize;
			}
			return currentResolution.AppliedWidth / currentResolution.pixelSize;
		}
	}

	public static int PixelSize => currentResolution?.pixelSize ?? DevUtils.PixelSize;

	public bool IsValid
	{
		get
		{
			if (width > 0)
			{
				return height > 0;
			}
			return false;
		}
	}

	public int ListedWidth => width;

	public int ListedHeight => height;

	public int AppliedWidth
	{
		get
		{
			if (!HasFakeResolution)
			{
				return width;
			}
			return fakeWidth;
		}
	}

	public int AppliedHeight
	{
		get
		{
			if (!HasFakeResolution)
			{
				return height;
			}
			return fakeHeight;
		}
	}

	public UIWindowSizeType WindowSizeType => windowSizeType;

	public bool UseMainMenuScaleX2 => useMainMenuScaleX2;

	private bool HasFakeResolution
	{
		get
		{
			if (isFakeResolution && fakeWidth > 0)
			{
				return fakeHeight > 0;
			}
			return false;
		}
	}

	private static List<ResolutionConfig> Resolutions
	{
		get
		{
			if (availableResolutions.Count <= 0)
			{
				return hardcodedResolutions;
			}
			return availableResolutions;
		}
	}

	public static bool IsDisplayBelowMinimum()
	{
		if (!TryGetNativeDisplaySize(out var num, out var num2))
		{
			return false;
		}
		Debug.Log(string.Format("{0}: native display [{1}x{2}], minimum [{3}x{4}]", "ResolutionConfig", num, num2, 1280, 720));
		return num2 < 720;
	}

	public static bool TryGetNativeDisplaySize(out int width, out int height)
	{
		width = 0;
		height = 0;
		if (Display.main != null)
		{
			ConsiderNativeDisplaySize(Display.main.systemWidth, Display.main.systemHeight, ref width, ref height);
		}
		ConsiderNativeDisplaySize(Screen.currentResolution.width, Screen.currentResolution.height, ref width, ref height);
		if (width > 0)
		{
			return height > 0;
		}
		return false;
	}

	private static void ConsiderNativeDisplaySize(int candidateWidth, int candidateHeight, ref int width, ref int height)
	{
		if (candidateWidth > 0 && candidateHeight > 0 && (height <= 0 || candidateHeight < height))
		{
			width = candidateWidth;
			height = candidateHeight;
		}
	}

	public static float GetUiScaleFactor()
	{
		int num = PixelSize;
		ResolutionConfig resolutionConfig = currentResolution;
		if (resolutionConfig != null)
		{
			_ = resolutionConfig.AppliedHeight;
			_ = 0;
		}
		return num;
	}

	public ResolutionConfig Copy()
	{
		return new ResolutionConfig(width, height, pixelSize, windowSizeType, customAdditinalString, useMainMenuScaleX2, isFakeResolution, fakeWidth, fakeHeight);
	}

	public ResolutionConfig WithSize(int newWidth, int newHeight)
	{
		return new ResolutionConfig(newWidth, newHeight, pixelSize, windowSizeType, customAdditinalString, useMainMenuScaleX2);
	}

	public int FindIndexForScreenSize(int screenWidth, int screenHeight)
	{
		return FindResolutionConfigIndex(new ResolutionConfig(screenWidth, screenHeight, pixelSize, windowSizeType, customAdditinalString, useMainMenuScaleX2));
	}

	public ResolutionConfig(int width, int height)
	{
		this.width = width;
		this.height = height;
		pixelSize = GetPixelSize(width, height);
		UpdateUIModeType();
		customAdditinalString = string.Empty;
		useMainMenuScaleX2 = false;
	}

	public ResolutionConfig(int width, int height, int pixelSize, UIWindowSizeType windowSizeType = UIWindowSizeType.Big, string customAdditinalString = "", bool useMainMenuScaleX2 = false, bool isFakeResolution = false, int fakeWidth = 0, int fakeHeight = 0)
	{
		this.width = width;
		this.height = height;
		this.pixelSize = pixelSize;
		this.windowSizeType = windowSizeType;
		this.customAdditinalString = customAdditinalString;
		this.useMainMenuScaleX2 = useMainMenuScaleX2;
		this.isFakeResolution = isFakeResolution;
		this.fakeWidth = fakeWidth;
		this.fakeHeight = fakeHeight;
	}

	public static string[] GetResolutionsStringArray()
	{
		List<string> list = new List<string>();
		foreach (ResolutionConfig resolution in Resolutions)
		{
			list.Add(resolution.GetResolutionName());
		}
		return list.ToArray();
	}

	public static ResolutionConfig GetResolutionConfigByIndex(int index)
	{
		if (index < 0 || index > Resolutions.Count - 1)
		{
			Debug.LogError(string.Format("Cannot find {0} by index [{1}]", "ResolutionConfig", index));
			return Resolutions[0];
		}
		return Resolutions[index];
	}

	public static ResolutionConfig GetOptimalResolution()
	{
		int num = Screen.currentResolution.width;
		int num2 = Screen.currentResolution.height;
		if (num <= 0 || num2 <= 0)
		{
			num = Screen.width;
			num2 = Screen.height;
		}
		ResolutionConfig resolutionConfig = null;
		int num3 = -1;
		for (int i = 0; i < Resolutions.Count; i++)
		{
			ResolutionConfig resolutionConfig2 = Resolutions[i];
			if (resolutionConfig2.HasFakeResolution)
			{
				continue;
			}
			if (resolutionConfig2.width == num && resolutionConfig2.height == num2)
			{
				return resolutionConfig2.Copy();
			}
			if (resolutionConfig2.width <= num && resolutionConfig2.height <= num2)
			{
				int num4 = resolutionConfig2.width * resolutionConfig2.height;
				if (num4 > num3)
				{
					num3 = num4;
					resolutionConfig = resolutionConfig2;
				}
			}
		}
		if (resolutionConfig != null)
		{
			return resolutionConfig.Copy();
		}
		return new ResolutionConfig(num, num2);
	}

	public static int FindResolutionConfigIndex(int width, int height)
	{
		Debug.Log(string.Format("call [FindResolutionConfigIndex] in [{0}] width:[{1}] height:[{2}]", "ResolutionConfig", width, height));
		for (int i = 0; i < Resolutions.Count; i++)
		{
			Debug.Log(string.Format("{0}: trying to find res [{1}x{2}]. Checking: {3}", "ResolutionConfig", width, height, Resolutions[i].GetResolutionName()));
			if (Resolutions[i].width == width && Resolutions[i].height == height)
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindResolutionConfigIndex(ResolutionConfig resolutionConfig)
	{
		if (resolutionConfig == null)
		{
			return -1;
		}
		for (int i = 0; i < Resolutions.Count; i++)
		{
			ResolutionConfig resolutionConfig2 = Resolutions[i];
			if (resolutionConfig2.width == resolutionConfig.width && resolutionConfig2.height == resolutionConfig.height && resolutionConfig2.pixelSize == resolutionConfig.pixelSize && resolutionConfig2.windowSizeType == resolutionConfig.windowSizeType && resolutionConfig2.customAdditinalString == resolutionConfig.customAdditinalString)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool TryAddAvailableResolution(ResolutionConfig resolutionConfig)
	{
		foreach (ResolutionConfig availableResolution in availableResolutions)
		{
			if (availableResolution.width == resolutionConfig.width && availableResolution.height == resolutionConfig.height && availableResolution.windowSizeType == resolutionConfig.windowSizeType && availableResolution.customAdditinalString == resolutionConfig.customAdditinalString)
			{
				Debug.LogWarning("Resolution with parameters " + resolutionConfig.GetInfo() + " already exist");
				return false;
			}
		}
		Debug.Log($"AddResolution [{resolutionConfig.width}x{resolutionConfig.height}] size:[{resolutionConfig.windowSizeType}]");
		availableResolutions.Add(resolutionConfig);
		return true;
	}

	public static void InitAvailableResolutions()
	{
		if (isInitialized)
		{
			return;
		}
		isInitialized = true;
		currentResolution = null;
		availableResolutions.Clear();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			Debug.Log($"  Processing resolution [{resolution.width}x{resolution.height}]");
			if (IsUltraWide(resolution) || resolution.width < 1280 || resolution.height < 720 || resolution.width > 5120)
			{
				continue;
			}
			List<ResolutionConfig> list = hardcodedResolutions.FindAll((ResolutionConfig r) => r.width == resolution.width && r.height == resolution.height);
			if (list.Count > 0)
			{
				foreach (ResolutionConfig item in list)
				{
					TryAddAvailableResolution(item);
				}
			}
			else
			{
				TryAddAvailableResolution(new ResolutionConfig(resolution.width, resolution.height));
			}
		}
	}

	public static void SetResolution(IntVector2 resolution)
	{
		int num = FindResolutionConfigIndex(resolution.x, resolution.y);
		if (num != -1)
		{
			currentResolution = Resolutions[num].Copy();
			Debug.Log("ResolutionConfig: Set resolution [" + currentResolution.GetInfo() + "]");
			return;
		}
		currentResolution = Resolutions[FindClosestResolutionConfigIndex(resolution.x, resolution.y)];
		Debug.LogWarning(string.Format("{0}:cannot find resolution [{1}] in {2}. Applied closest one [{3}]", "ResolutionConfig", resolution, "Resolutions", currentResolution));
	}

	public static void SetResolution(ResolutionConfig resolutionConfig)
	{
		if (resolutionConfig == null)
		{
			currentResolution = null;
			return;
		}
		int num = FindResolutionConfigIndex(resolutionConfig);
		if (num != -1)
		{
			currentResolution = Resolutions[num].Copy();
			Debug.Log("ResolutionConfig: Set resolution by config [" + currentResolution.GetInfo() + "]");
			return;
		}
		int num2 = FindResolutionConfigIndex(resolutionConfig.width, resolutionConfig.height);
		if (num2 != -1)
		{
			currentResolution = Resolutions[num2].Copy();
			Debug.Log("ResolutionConfig: Set resolution by size [" + currentResolution.GetInfo() + "]");
			return;
		}
		int num3 = ((resolutionConfig.height < 720) ? GetPixelSize(resolutionConfig.width, resolutionConfig.height) : ((resolutionConfig.pixelSize > 0) ? resolutionConfig.pixelSize : GetPixelSize(resolutionConfig.width, resolutionConfig.height)));
		ResolutionConfig resolutionConfig2 = new ResolutionConfig(resolutionConfig.width, resolutionConfig.height, num3, resolutionConfig.windowSizeType, resolutionConfig.customAdditinalString, resolutionConfig.useMainMenuScaleX2, resolutionConfig.isFakeResolution, resolutionConfig.fakeWidth, resolutionConfig.fakeHeight);
		if (resolutionConfig2.windowSizeType != UIWindowSizeType.Small && resolutionConfig2.windowSizeType != 0)
		{
			resolutionConfig2.UpdateUIModeType();
		}
		else if (resolutionConfig2.windowSizeType == UIWindowSizeType.Big && (resolutionConfig2.height < 720 || resolutionConfig2.height / resolutionConfig2.pixelSize <= 360))
		{
			resolutionConfig2.UpdateUIModeType();
		}
		currentResolution = resolutionConfig2;
		Debug.Log("ResolutionConfig: Set normalized custom resolution [" + currentResolution.GetInfo() + "]");
	}

	public string GetResolutionName()
	{
		return $"{width}x{height}{customAdditinalString}";
	}

	private static bool IsUltraWide(Resolution resolution)
	{
		return (float)resolution.width / (float)resolution.height > 2f;
	}

	private static int FindClosestResolutionConfigIndex(int width, int height)
	{
		int result = 0;
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		for (int i = 0; i < Resolutions.Count; i++)
		{
			int num3 = Mathf.Abs(Resolutions[i].width - width);
			int num4 = Mathf.Abs(Resolutions[i].height - height);
			if (num3 <= num && num4 <= num2)
			{
				num = num3;
				num2 = num4;
				result = i;
			}
		}
		return result;
	}

	public static ResolutionConfig CreateSwitchResolutionConfig()
	{
		if (SwitchDisplayResolution.TryGet(out var num, out var num2) && num2 > 0)
		{
			if (num2 > 720)
			{
				return new ResolutionConfig(1280, 720);
			}
			return new ResolutionConfig(num, num2);
		}
		return new ResolutionConfig(1280, 720);
	}

	public static int GetPixelSize(int width, int height)
	{
		if (height < 720)
		{
			return 1;
		}
		int result = 2;
		if (height > 1440)
		{
			result = Mathf.CeilToInt((float)height / 540f);
		}
		return result;
	}

	private void UpdateUIModeType()
	{
		windowSizeType = ((height < 720 || height / pixelSize <= 360) ? UIWindowSizeType.Small : UIWindowSizeType.Big);
	}

	private string GetInfo()
	{
		string text = $"Resolution: [{width}x{height}] PixelSize: [{pixelSize}] windowSizeType:[{windowSizeType}] customAdditinalString:[{customAdditinalString}] useMainMenuScaleX2:[{useMainMenuScaleX2}]";
		if (HasFakeResolution)
		{
			text += $" fakeResolution:[{fakeWidth}x{fakeHeight}]";
		}
		return text;
	}

	public static void LogCurrentResolutionConfig()
	{
		if (currentResolution == null)
		{
			Debug.LogWarning("ResolutionConfig: currentResolution is null");
		}
		else
		{
			Debug.Log(string.Format("{0} current config: {1}", "ResolutionConfig", currentResolution));
		}
	}

	public override string ToString()
	{
		return "[" + GetInfo() + "]";
	}
}
