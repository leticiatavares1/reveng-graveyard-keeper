using DG.Tweening;
using UnityEngine;

public class IllustrationsGUI : BaseGUI
{
	public Transform illustrations_folder;

	public UILabel text;

	public UI2DSprite dark_back;

	public bool is_open;

	public override void Init()
	{
		base.Init();
	}

	public override void OnClosePressed()
	{
	}

	public new void Open(bool with_dark_back = true)
	{
		base.Open(play_open_sound: false);
		text.alpha = 0f;
		text.text = string.Empty;
		ShowIllustration(string.Empty);
		dark_back.alpha = 0f;
		if (with_dark_back)
		{
			DOTween.To(() => dark_back.alpha, delegate(float x)
			{
				dark_back.alpha = x;
			}, 1f, 0.5f);
		}
		is_open = true;
	}

	public void Hide()
	{
		SetText(string.Empty, out var _);
		ShowIllustration(string.Empty);
		DOTween.To(() => dark_back.alpha, delegate(float x)
		{
			dark_back.alpha = x;
		}, 0f, 0.5f).OnComplete(delegate
		{
			base.Hide(play_hide_sound: false);
		});
		is_open = false;
	}

	public void ShowIllustration(string illustration_name)
	{
		GameObject object_to_hide = null;
		GameObject object_to_show = null;
		for (int i = 0; i < illustrations_folder.childCount; i++)
		{
			Transform child = illustrations_folder.GetChild(i);
			if (child == null)
			{
				break;
			}
			GameObject gameObject = child.gameObject;
			if (gameObject.name == illustration_name && !gameObject.activeInHierarchy)
			{
				object_to_show = gameObject;
			}
			else if (gameObject.name != illustration_name && gameObject.activeInHierarchy)
			{
				object_to_hide = gameObject;
			}
		}
		if (object_to_hide != null)
		{
			UI2DSprite spr_to_hide = object_to_hide.GetComponent<UI2DSprite>();
			if (spr_to_hide == null)
			{
				Debug.LogError("Not found sprite to hide on object " + object_to_hide.name);
				return;
			}
			DOTween.To(() => spr_to_hide.alpha, delegate(float x)
			{
				spr_to_hide.alpha = x;
			}, 0f, 0.5f).OnComplete(delegate
			{
				object_to_hide.SetActive(value: false);
				spr_to_hide.alpha = 1f;
				if (object_to_show != null)
				{
					UI2DSprite spr_to_show2 = object_to_show.GetComponent<UI2DSprite>();
					if (spr_to_show2 == null)
					{
						Debug.LogError("Not found sprite to show on object " + object_to_show.name);
					}
					else
					{
						object_to_show.SetActive(value: true);
						spr_to_show2.alpha = 0f;
						DOTween.To(() => spr_to_show2.alpha, delegate(float x)
						{
							spr_to_show2.alpha = x;
						}, 1f, 0.5f);
					}
				}
			});
		}
		else
		{
			if (!(object_to_show != null))
			{
				return;
			}
			UI2DSprite spr_to_show = object_to_show.GetComponent<UI2DSprite>();
			if (spr_to_show == null)
			{
				Debug.LogError("Not found sprite to show on object " + object_to_show.name);
				return;
			}
			object_to_show.SetActive(value: true);
			spr_to_show.alpha = 0f;
			DOTween.To(() => spr_to_show.alpha, delegate(float x)
			{
				spr_to_show.alpha = x;
			}, 1f, 0.5f);
		}
	}

	public void SetText(string new_text, out float hold_time)
	{
		new_text = GJL.L(new_text);
		hold_time = SpeechBubbleGUI.CalculateWaitTime(new_text.Length);
		if (!string.IsNullOrEmpty(text.text))
		{
			hold_time += 0.5f;
			DOTween.To(() => text.alpha, delegate(float x)
			{
				text.alpha = x;
			}, 0f, 0.5f).OnComplete(delegate
			{
				text.text = new_text;
				DOTween.To(() => text.alpha, delegate(float x)
				{
					text.alpha = x;
				}, 1f, 0.5f);
			});
		}
		else
		{
			text.alpha = 0f;
			text.text = new_text;
			DOTween.To(() => text.alpha, delegate(float x)
			{
				text.alpha = x;
			}, 1f, 0.5f);
		}
	}
}
