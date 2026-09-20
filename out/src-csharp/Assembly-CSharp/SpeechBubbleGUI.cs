using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SpeechBubbleGUI : BaseBubbleGUI
{
	public enum SpeechBubbleType
	{
		Talk,
		Think,
		InfoBox
	}

	public static Dictionary<long, SpeechBubbleGUI> all = new Dictionary<long, SpeechBubbleGUI>();

	[SerializeField]
	[HideInInspector]
	private UILabel _label;

	private const float LETTER_ANIM_APPEAR_TIME = 0.02f;

	private const float TOTAL_TIME = 1.5f;

	private const float ONE_SYMBOL_TIME = 0.08449999f;

	private const float EASTERN_SYMBOL_K = 4f;

	private static SpeechBubbleGUI _me;

	private bool _animating;

	private bool _disappearing;

	private bool _use_world_cam = true;

	private float _last_time;

	private float _hold_time;

	private int _prev_w;

	private long _speaker_id;

	private SpeechBubbleType _type;

	private SmartSpeechEngine.VoiceID _voice;

	public Sprite spr_corner_talk;

	public Sprite spr_corner_think;

	public Sprite spr_back_say;

	public Sprite spr_back_square;

	public Color color_normal;

	public Color color_player;

	private static int _last_depth = 0;

	public static readonly char[] RANDOM_CHARACTERS = new char[22]
	{
		'╣', '║', '╗', '╝', '╜', '╛', '┐', '└', '┴', '┬',
		'├', '┼', '╞', '╟', '╚', '╔', '╩', '╦', '╠', '╬',
		'╧', '╨'
	};

	private static readonly List<char> NON_CHANGABLE_CHARS = new List<char>
	{
		' ', '\r', '\n', '0', '1', '2', '3', '4', '5', '6',
		'7', '8', '9', '[', ']', ')'
	};

	public override void Init()
	{
		_me = this;
		_label = GetComponentInChildren<UILabel>(includeInactive: true);
		base.Init();
		BaseGUI.on_window_opened += delegate
		{
			if (all.Count == 0)
			{
				return;
			}
			foreach (SpeechBubbleGUI item in new List<SpeechBubbleGUI>(all.Values))
			{
				item.ForceHide(without_anims: true);
			}
		};
	}

	private void ShowMessage(string msg, bool show_to_left = false, Color? color = null, SmartSpeechEngine.VoiceID voice = SmartSpeechEngine.VoiceID.None)
	{
		base.gameObject.SetActive(value: true);
		try_show_to_left = show_to_left;
		_voice = voice;
		string text2 = (_label.text = SpeechText(msg));
		txt = text2;
		GameObject[] array = corners;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponentInChildren<UI2DSprite>().sprite2D = ((_type == SpeechBubbleType.InfoBox) ? null : ((_type == SpeechBubbleType.Talk) ? spr_corner_talk : spr_corner_think));
		}
		if (back_spr != null)
		{
			back_spr.sprite2D = ((_type == SpeechBubbleType.InfoBox) ? spr_back_square : spr_back_say);
		}
		OnContentChanged();
		StartAppear();
		_animating = true;
		_last_time = GJCommons.GetTicksInSeconds();
		_hold_time = CalculateWaitTime(txt.Length);
		if (color.HasValue)
		{
			SetColor(color.Value);
		}
		LateUpdate();
		Update();
	}

	public void Update()
	{
		if (!MainGame.paused)
		{
			DoAnimations();
		}
	}

	private void DoAnimations()
	{
		bool flag = LazyInput.GetKeyDown(GameKey.Back) || LazyInput.GetKeyDown(GameKey.Select) || Input.GetMouseButtonDown(0);
		if (flag)
		{
			LazyInput.ClearAllKeysDown();
		}
		float num = GJCommons.GetTicksInSeconds() - _last_time;
		if (flag && num.EqualsTo(0f))
		{
			flag = false;
		}
		if (!_animating)
		{
			if (_hold_time > 0f)
			{
				_hold_time -= RealTime.deltaTime;
			}
			if (flag)
			{
				_hold_time = -1f;
			}
			if (!_disappearing && !(_hold_time > 0f))
			{
				_disappearing = true;
				StartDisappear();
			}
			return;
		}
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num2;
		do
		{
			num2 = Mathf.FloorToInt(num / 0.02f);
			flag4 = false;
			if (flag)
			{
				num2 = txt.Length;
			}
			else if (_prev_w < num2 - 1)
			{
				num2 = ++_prev_w;
				flag4 = true;
			}
			if (num2 >= txt.Length)
			{
				num2 = txt.Length;
				_animating = false;
				break;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num2 > 0)
			{
				char num3 = txt[num2 - 1];
				if (num3 == '(')
				{
					flag2 = true;
				}
				if (num3 == '[')
				{
					flag3 = true;
				}
				bool flag5 = false;
				if (num3 == ')')
				{
					flag2 = false;
					flag5 = true;
				}
				if (num3 == ']')
				{
					flag3 = false;
					flag5 = true;
				}
				if (flag5 && num2 < txt.Length && txt[num2] == '[')
				{
					flag3 = true;
				}
			}
			if (flag2 || flag3)
			{
				_last_time -= 0.02f;
				num += 0.02f;
			}
		}
		while (flag2 || flag3 || flag4);
		_prev_w = num2;
		string text = txt.Substring(0, num2);
		string text2 = txt.Substring(num2);
		text2 = text2.Replace("[-][/c]", "");
		_label.text = text + "[c][00000000]" + text2 + "[-][/c]";
		float volume = 1f;
		if (linked_tf != null)
		{
			volume = Sounds.CalcSoundVolume(linked_tf.position);
		}
		SmartSpeechEngine.me.PlayVoiceSound(_voice, volume);
	}

	public void ForceHide(bool without_anims = false)
	{
		_animating = false;
		_hold_time = -1f;
		if (!_disappearing)
		{
			_disappearing = true;
			if (without_anims)
			{
				alpha_anim_time = 0f;
			}
			StartDisappear();
		}
	}

	public override void DestroyBubble()
	{
		base.DestroyBubble();
		if (all.ContainsKey(_speaker_id) && all[_speaker_id] == this)
		{
			all.Remove(_speaker_id);
		}
	}

	public static float CalculateWaitTime(int message_len)
	{
		float num = (float)message_len * 0.08449999f;
		if (GJL.IsEastern())
		{
			num *= 4f;
		}
		return 1.5f + num;
	}

	public override void LateUpdate()
	{
		if (linked_tf != null)
		{
			UpdateBubble(linked_tf.position, _use_world_cam);
		}
	}

	public static void ShowMessage(long speaker_id, string txt, Transform link, GJCommons.VoidDelegate on_disappeared = null, bool show_to_left = false, bool use_world_cam = true, SpeechBubbleType type = SpeechBubbleType.Talk, bool is_player = false, SmartSpeechEngine.VoiceID voice = SmartSpeechEngine.VoiceID.None)
	{
		if (_me == null)
		{
			return;
		}
		if (all.ContainsKey(speaker_id))
		{
			all[speaker_id].ForceHide();
			all.Remove(speaker_id);
		}
		SpeechBubbleGUI speechBubbleGUI = _me.Copy();
		_last_depth += 3;
		GJL.EnsureChildLabelsHasCorrectFont(speechBubbleGUI.gameObject, do_cache: false);
		GJL.ApplyCustomFontSettings(speechBubbleGUI.gameObject);
		speechBubbleGUI.linked_tf = link;
		speechBubbleGUI._speaker_id = speaker_id;
		speechBubbleGUI._use_world_cam = use_world_cam;
		speechBubbleGUI._type = type;
		if (type == SpeechBubbleType.InfoBox)
		{
			speechBubbleGUI.try_bottom_center = true;
		}
		speechBubbleGUI.ShowMessage(txt, show_to_left, is_player ? _me.color_player : _me.color_normal, voice);
		speechBubbleGUI.on_disappeared = on_disappeared;
		if (speechBubbleGUI._label != null)
		{
			speechBubbleGUI._label.depth = _last_depth + 2;
		}
		if (speechBubbleGUI.back_spr != null)
		{
			speechBubbleGUI.back_spr.depth = _last_depth;
		}
		GameObject[] array = speechBubbleGUI.corners;
		for (int i = 0; i < array.Length; i++)
		{
			UIWidget component = array[i].GetComponent<UIWidget>();
			if (component != null)
			{
				component.depth = _last_depth + 1;
			}
		}
		all.Add(speaker_id, speechBubbleGUI);
	}

	public static string SpeechText(string s)
	{
		s = LocalizedLabel.ColorizeTags(GJL.L(s), LocalizedLabel.TextColor.SpeechBubble);
		StringBuilder stringBuilder = new StringBuilder();
		float num = PlayerComponent.GetTextObfuscationChance() * 100f;
		if (num < 5f)
		{
			return s;
		}
		bool flag = false;
		string text = s;
		foreach (char c in text)
		{
			if (c == '(')
			{
				flag = true;
			}
			if (c == ')')
			{
				flag = false;
			}
			if (flag || NON_CHANGABLE_CHARS.Contains(c) || (float)Random.Range(0, 100) > num)
			{
				stringBuilder.Append(c);
			}
			else
			{
				stringBuilder.Append(RANDOM_CHARACTERS[NGUITools.RandomRange(0, RANDOM_CHARACTERS.Length - 1)]);
			}
		}
		return stringBuilder.ToString();
	}

	public void SetColor(Color c)
	{
		back_spr.color = c;
		GameObject[] array = corners;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponentInChildren<UI2DSprite>().color = c;
		}
	}
}
