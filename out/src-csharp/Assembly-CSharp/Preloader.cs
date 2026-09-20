using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{
	public TitleScreenCamera camera;

	public static Preloader me;

	public static bool is_shown;

	public UI2DSprite progress_bar;

	public static bool awake_was_done;

	public GameObject error_go;

	public UILabel error_txt;

	public void Awake()
	{
		PlatformSpecific.FixScreenModeAfterStart();
		Debug.Log("****************************************\n*** Starting Graveyard Keeper, ver. " + LazyConsts.VERSION);
		Debug.Log("Init: device name : " + SystemInfo.graphicsDeviceName + ", type: " + SystemInfo.deviceType.ToString() + ", model: " + SystemInfo.deviceModel + ", ver: " + SystemInfo.graphicsDeviceVersion + ", screen: " + Screen.width + "x" + Screen.height, this);
		Debug.Log("Preloader.Awake, time = " + Time.time, this);
		Debug.Log("Current system language: " + Application.systemLanguage);
		Debug.Log($"Stranger Sins DLC is available: {DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Stories)}");
		Debug.Log($"Game of Crone DLC is available: {DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees)}");
		Debug.Log($"Better Save Soul DLC is available: {DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Souls)}");
		if (error_go != null)
		{
			error_go.SetActive(value: false);
		}
		if (GameSettings.me == null)
		{
			Debug.LogError("Error loading game settings");
			return;
		}
		ResolutionConfig.InitResolutions();
		GameSettings.me.ApplyScreenMode();
		Stats.Init();
		Stats.DesignEvent("Initialize", LazyConsts.VERSION);
		me = this;
		is_shown = true;
		awake_was_done = true;
		SetProgressBar(0f);
		new GameObject("Resolution Helper").AddComponent<ResolutionHelper>();
		new GameObject("MouseCursorAutoHide").AddComponent<MouseCursorAutoHide>();
		UIRoot reloader_root = base.gameObject.GetComponentInChildren<UIRoot>(includeInactive: true);
		reloader_root.gameObject.SetActive(value: false);
		int num = 1 & (CheckTimezoneBug() ? 1 : 0) & (CheckTurkishBug() ? 1 : 0);
		PlatformSpecific.SetDefaultCultureInfo();
		if (num == 0)
		{
			reloader_root.gameObject.SetActive(value: true);
			return;
		}
		GJTimer.AddTimer(0f, delegate
		{
			SceneManager.LoadSceneAsync("scene_main", LoadSceneMode.Additive);
			SceneManager.sceneLoaded += OnSceneLoaded;
			GJTimer.AddTimer(0f, delegate
			{
				reloader_root.gameObject.SetActive(value: true);
			});
		});
	}

	private bool CheckTurkishBug()
	{
		Debug.Log("CheckTurkishBug... " + "i".ToUpper());
		return true;
	}

	private bool CheckTimezoneBug()
	{
		Debug.Log("CheckTimezoneBug...");
		try
		{
			Debug.Log("time now = " + DateTime.Now.ToString());
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Timezone bug detected: " + ex);
			error_go.SetActive(value: true);
			error_txt.text = "Error initializing TimeZone!\n\nPlease, check your current TIME ZONE in your OS settings -- seems that something is wrong with it.\n\nRestart the game after that.";
			return false;
		}
		return true;
	}

	private static void OnSceneLoaded(Scene s, LoadSceneMode mode)
	{
		Debug.Log("Preloader.OnSceneLoaded: " + s.name + ", time = " + Time.time);
		if (s.name == "scene_main")
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	public static void OnMainGameLoaded()
	{
		_ = me == null;
	}

	public static void Hide()
	{
		if (!(me == null))
		{
			is_shown = false;
			NGUITools.Destroy(me.gameObject);
		}
	}

	public static void SetProgressBar(float progress)
	{
		if (is_shown && !(me == null))
		{
			me.progress_bar.fillAmount = progress;
		}
	}
}
