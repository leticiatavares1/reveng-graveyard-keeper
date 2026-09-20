using System;
using System.Collections.Generic;
using UnityEngine;

public static class DevUtils
{
	public static List<string> MeshUnlitMaterialPostfixes = new List<string> { "_ut", "-ut" };

	public const string MESH_BLACKOUT_MATERIAL_POSTFIX = "-blackout";

	public const string TREE_MESH_PREFIX = "tree_";

	public const string TREE_LEAVES_MESH_PART = "_fm";

	public const float GFX_SIZE_MULTIPLIER = 100f;

	public const string KEY_AUTOLOADER_SCENE = "scene_autoloader";

	public const string KEY_SKIP_MAIN_SCENE = "main_scene_skip";

	public const string KEY_AUTO_SELECT_WGO = "auto_select_wgo";

	public const string KEY_AUTO_SELECT_CONSTRUCTOR_PART = "auto_select_constructor_part";

	public const string KEY_ENABLE_ANALYZER = "enable_analyzer";

	public const string SKIP_QUEST_AUTO_START = "skip_quest_auto_start";

	public const string KEY_SKIP_CAMERA_ANIMATION = "skip_camera_animation";

	public const string KEY_NATIVE_CAMERA_DISABLED = "native_camera_disable";

	public const string KEY_ZOMBIE_LOG = "zombie_log";

	public const string KEY_LOG_CHANNELS = "log_channels";

	public const string KEY_SIMULATE_DEMO_BITSUMMIT = "simulate_demo_bitsummit";

	public const string CONSTRUCTOR_ROOT_PATH = "Assets/_WorldAssets/Constructor";

	public const string MODELS_ROOT_PATH = "Assets/_WorldAssets";

	public const string TEXTURE_DIR_NAME = "Textures";

	public const string MESHES_DIR_NAME = "Meshes";

	public const string MODEL_PREFAB_DIR_NAME = "Prefabs";

	public const string LUTS_DIR_NAME = "LUTs";

	public const string MESH_PIVOT_POSTFIX = "-pivot";

	public const string MESH_COLLISION_POSTFIX = "-collision";

	public const string PREFAB_EXTENSION = ".prefab";

	public const string MESH_MERGE_TECH_POSTFIX = "-MERGE";

	public const string MESH_SHADOW_POSTFIX_0 = "_sh";

	public const string MESH_SHADOW_POSTFIX_1 = "-sh";

	public const string MESH_SHADOW_POSTFIX_2 = "_sh2";

	public const string MESH_SHADOW_POSTFIX_3 = "-sh2";

	public const string MESH_WIND_CLOTH_POSTFIX = "-wind";

	public static bool isMainSceneSkipped = false;

	public const string GND_TEXTURE_SUFFIX = "gnd";

	public const string SH_TEXTURE_SUFFIX = "sh";

	private static VerticalFogDisableZone[] verticalFogDisableZones;

	public static HashSet<string> GameplayScenes => new HashSet<string> { "RuinedTemple", "NorthRuinedTemple", "Prison", "IntroScoutBuilding", "IntroScoutSewer", "PalaceSewer" };

	public static int PixelSize
	{
		get
		{
			if (!IsNativeCameraInUse)
			{
				return 2;
			}
			return 1;
		}
	}

	public static bool IsNativeCameraInUse
	{
		get
		{
			return PlayerPrefs.GetInt("native_camera_disable") == 0;
		}
		private set
		{
			PlayerPrefs.SetInt("native_camera_disable", (!value) ? 1 : 0);
		}
	}

	public static bool SkipMainScene
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("main_scene_skip", value.ToInt());
		}
	}

	public static bool AutoSelectWgo
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("auto_select_wgo", value.ToInt());
		}
	}

	public static bool AutoSelectConstructorPart
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("auto_select_constructor_part", value.ToInt());
		}
	}

	public static bool EnableAnalyzer
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("enable_analyzer", value.ToInt());
		}
	}

	public static bool SkipQuestAutoStart
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("skip_quest_auto_start", value.ToInt());
		}
	}

	public static bool SkipCameraAnimation
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("skip_camera_animation", value.ToInt());
		}
	}

	public static bool SimulateDemoBitsummit
	{
		get
		{
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("simulate_demo_bitsummit", value.ToInt());
		}
	}

	public static bool IsDemoBitsummitActive => false;

	public static bool ZombieLog
	{
		get
		{
			return IsLogChannelEnabled(LogChannel.Zombie);
		}
		set
		{
			SetLogChannelEnabled(LogChannel.Zombie, value);
		}
	}

	public static LogChannel EnabledLogChannels
	{
		get
		{
			return LogChannel.None;
		}
		set
		{
			PlayerPrefs.SetInt("log_channels", (int)value);
		}
	}

	public static bool IsDebugFogActive => verticalFogDisableZones != null;

	public static bool IsDevWindValueSet { get; set; }

	public static float DevWindValue { get; set; }

	public static void SetCameraRenderType(bool isNativeCameraInUse)
	{
	}

	public static bool IsLogChannelEnabled(LogChannel channel)
	{
		return (EnabledLogChannels & channel) != 0;
	}

	public static void SetLogChannelEnabled(LogChannel channel, bool enabled)
	{
		EnabledLogChannels = (enabled ? (EnabledLogChannels | channel) : (EnabledLogChannels & ~channel));
	}

	public static void ToggleLogChannel(LogChannel channel)
	{
		EnabledLogChannels ^= channel;
	}

	private static LogChannel GetAllLogChannelsMask()
	{
		LogChannel logChannel = LogChannel.None;
		foreach (LogChannel value in Enum.GetValues(typeof(LogChannel)))
		{
			if (value != 0)
			{
				logChannel |= value;
			}
		}
		return logChannel;
	}

	public static string GetTextureGndPostfix(bool isConstructor)
	{
		if (!isConstructor)
		{
			return "_gnd";
		}
		return "-gnd";
	}

	public static string GetTextureShPostfix(bool isConstructor)
	{
		if (!isConstructor)
		{
			return "_sh";
		}
		return "-sh";
	}

	public static void SetDebugFogState(bool isActive)
	{
		if (isActive)
		{
			verticalFogDisableZones = UnityEngine.Object.FindObjectsByType<VerticalFogDisableZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
			VerticalFogDisableZone[] array = verticalFogDisableZones;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
		else
		{
			VerticalFogDisableZone[] array = verticalFogDisableZones;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
			verticalFogDisableZones = null;
		}
		WeatherSystem.Instance.SetWeatherComponent("DebugFogOpacity", isActive);
	}
}
