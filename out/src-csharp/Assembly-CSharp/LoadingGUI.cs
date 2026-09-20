using UnityEngine;

public class LoadingGUI : MonoBehaviour
{
	public float anim_time = 0.3f;

	private static LoadingGUI _me;

	private static UIWidget _widget;

	private static bool _shown;

	public GameObject progress_bar_go;

	public UI2DSprite progress_bar;

	private static LocalizedLabel _localized_label;

	private bool _inited;

	private AsyncOperation _async;

	public static bool is_shown => _shown;

	public void Init()
	{
		if (!_inited)
		{
			_inited = true;
			_widget = GetComponentInChildren<UIWidget>();
			_localized_label = GetComponentInChildren<LocalizedLabel>();
			_me = this;
			_me.gameObject.SetActive(value: false);
		}
	}

	public static void Show(GJCommons.VoidDelegate on_anim_played = null)
	{
		Debug.Log("Loading GUI: Show");
		if (_shown)
		{
			Debug.LogWarning("loading gui already shown");
			return;
		}
		if (_me == null)
		{
			_me = GUIElements.me.loading;
			_me.Init();
		}
		_me.progress_bar_go.SetActive(value: false);
		_shown = true;
		_localized_label.Localize();
		_me.gameObject.SetActive(value: true);
		_me.GetComponent<UIPanel>().ChangeAlpha(0f, 1f, _me.anim_time, delegate
		{
			TitleScreen.Hide();
			if (on_anim_played != null)
			{
				on_anim_played();
			}
		});
	}

	public static void ShowWithProgressBar()
	{
		Show();
		_me._async = null;
		ShowProgressBar();
	}

	public static void ShowProgressBar()
	{
		_me.progress_bar_go.SetActive(value: true);
		SetProgressBar(0f);
	}

	public static void SetProgressBar(float progress)
	{
		Debug.Log("Loading: SetProgressBar = " + progress);
		_me.progress_bar.fillAmount = progress;
		Preloader.SetProgressBar(progress);
	}

	public static void IncreaseProgressBar(float amount = 0.05f)
	{
		SetProgressBar(_me.progress_bar.fillAmount + amount);
	}

	public static void LinkAsyncProcess(AsyncOperation async)
	{
		_me._async = async;
	}

	public void Update()
	{
		if (progress_bar_go.activeSelf && _async != null)
		{
			float progress = _async.progress;
			progress_bar.fillAmount = progress;
			Preloader.SetProgressBar(progress);
		}
	}

	public static void Hide(GJCommons.VoidDelegate on_anim_played = null)
	{
		Debug.Log("Loading GUI: Hide");
		if (!_shown)
		{
			Debug.LogError("loading gui is not shown");
			return;
		}
		if (Preloader.is_shown)
		{
			HideImmediate();
			on_anim_played.TryInvoke();
			return;
		}
		GJTimer.AddTimer(0.1f, delegate
		{
			_me.GetComponent<UIPanel>().ChangeAlpha(_me.GetComponent<UIPanel>().alpha, 0f, _me.anim_time, delegate
			{
				_shown = false;
				on_anim_played.TryInvoke();
				_me.gameObject.SetActive(value: false);
			});
		});
	}

	public static void HideImmediate()
	{
		_shown = false;
		_me.gameObject.SetActive(value: false);
	}

	public static void ShowBlackBackground(bool vis, bool animated = false)
	{
		if (!animated)
		{
			GUIElements.me.black_background.gameObject.SetActive(vis);
			if (vis)
			{
				GUIElements.me.black_background.alpha = 1f;
			}
			return;
		}
		GUIElements.me.black_background.gameObject.SetActive(value: true);
		GUIElements.me.black_background.alpha = ((!vis) ? 1 : 0);
		GUIElements.me.black_background.ChangeAlpha(GUIElements.me.black_background.alpha, vis ? 1 : 0, _me.anim_time, delegate
		{
			if (!vis)
			{
				GUIElements.me.black_background.gameObject.SetActive(value: false);
			}
		});
	}
}
