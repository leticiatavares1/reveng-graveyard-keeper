using System;
using DG.Tweening;
using DarkTonic.MasterAudio;
using UnityEngine;

public class Intro : MonoBehaviour
{
	public TitleScreenCamera intro_camera;

	public UILabel subtitle_text;

	public Animator animator;

	private static Intro _me;

	private Action _on_finished;

	public static bool need_show_first_intro;

	public void ShowSubtitleText(string lng_id)
	{
		GJL.EnsureLabelHasCorrectFont(subtitle_text, do_cache: false);
		subtitle_text.text = GJL.L(lng_id);
		subtitle_text.gameObject.SetActive(value: true);
		subtitle_text.GetComponent<TypewriterEffect>().enabled = false;
		subtitle_text.alpha = 0f;
		DOTween.To(() => subtitle_text.alpha, delegate(float v)
		{
			subtitle_text.alpha = v;
		}, 1f, 0.3f);
	}

	public void SubtitleTextDisappear()
	{
		DOTween.To(() => subtitle_text.alpha, delegate(float v)
		{
			subtitle_text.alpha = v;
		}, 0f, 1f);
	}

	private void Awake()
	{
		Debug.Log("Intro.Awake", this);
		if (MainGame.me != null)
		{
			Debug.Log("Loaded from the MainGame");
			base.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(intro_camera.GetComponent<AudioListener>());
		}
		_me = this;
	}

	public static void ShowIntro(Action on_finished, bool no_words = false, bool stop_all_playlist = true)
	{
		if (!need_show_first_intro)
		{
			on_finished.TryInvoke();
			return;
		}
		if (_me == null)
		{
			_me = UnityEngine.Object.FindObjectOfType<Intro>();
			if (_me == null)
			{
				Debug.Log("ShowIntro, me is null");
				on_finished.TryInvoke();
				return;
			}
		}
		if (stop_all_playlist)
		{
			MasterAudio.StopAllPlaylists();
		}
		_me._on_finished = on_finished;
		_me.gameObject.SetActive(value: true);
		if (no_words)
		{
			_me.animator.SetTrigger("start_no_words");
		}
		else
		{
			_me.animator.SetTrigger("start");
		}
		SmartAudioEngine.me.TransitionToSnapshot("DisabledSoundFXLowPass");
	}

	public static void OnIntroAnimationFinished()
	{
		Debug.Log("OnIntroAnimationFinished");
		if (!(MainGame.me == null))
		{
			_me.gameObject.SetActive(value: false);
			_me._on_finished.TryInvoke();
		}
	}
}
