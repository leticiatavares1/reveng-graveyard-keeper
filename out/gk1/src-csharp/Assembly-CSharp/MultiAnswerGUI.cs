using System.Collections.Generic;
using UnityEngine;

public class MultiAnswerGUI : BaseBubbleGUI
{
	public delegate void MultiAnswerResult(string chosen);

	private const char LOCKABLE_PHRASE_FIRST_CHAR = '@';

	[Range(0f, 1f)]
	public float anim_time = 0.3f;

	[Range(0f, 0.3f)]
	public float anim_delay = 0.1f;

	[HideInInspector]
	public MultiAnswerOptionGUI answer_prefab;

	[HideInInspector]
	public SimpleUITable table;

	[HideInInspector]
	public GamepadNavigationController gamepad_controller;

	private static MultiAnswerGUI _me;

	private MultiAnswerResult _on_chosen;

	private List<MultiAnswerOptionGUI> _answers;

	private bool _all_anims_finished;

	private bool _need_reposition;

	public static WorldGameObject talker_wgo;

	private static MultiAnswerGUI _current;

	public override void Init()
	{
		_me = this;
		answer_prefab = GetComponentInChildren<MultiAnswerOptionGUI>(includeInactive: true);
		answer_prefab.Init();
		answer_prefab.gameObject.SetActive(value: false);
		table = GetComponent<SimpleUITable>();
		gamepad_controller = GetComponent<GamepadNavigationController>();
		base.Init();
	}

	protected void ShowAnswers(List<AnswerVisualData> answers, bool show_to_left)
	{
		base.gameObject.SetActive(value: true);
		BaseGUI.for_gamepad = LazyInput.gamepad_active;
		LazyInput.ClearKeyDown(GameKey.Select);
		LazyInput.ClearKey(GameKey.Select);
		List<string> list = new List<string>();
		List<AnswerVisualData> list2 = new List<AnswerVisualData>();
		bool flag = false;
		table?.ClearHashes();
		bool flag2 = false;
		foreach (AnswerVisualData answer in answers)
		{
			if (!string.IsNullOrEmpty(answer.id) && (answer.id[0] != '@' || MainGame.me.save.unlocked_phrases.Contains(answer.id)) && !MainGame.me.save.black_list_of_phrases.Contains(answer.id))
			{
				answer.translation = SpeechBubbleGUI.SpeechText(answer.id);
				list.Add(answer.translation);
				list2.Add(answer);
				if (answer.IsDetailed())
				{
					flag = true;
				}
				Debug.Log("Draw answer " + answer.id + ", " + answer.translation);
			}
		}
		if (list2.Count == 0)
		{
			Debug.LogError("No answers");
			_on_chosen("error");
			return;
		}
		Debug.Log("Showing multi-answer dialog with options count = " + list2.Count);
		int num = 0;
		foreach (string item in list)
		{
			int num2 = Mathf.RoundToInt(LabelSizeCalculator.Calc(answer_prefab.label, item).x);
			if (num2 > num)
			{
				num = num2;
			}
		}
		_answers = new List<MultiAnswerOptionGUI>();
		for (int i = 0; i < list2.Count; i++)
		{
			MultiAnswerOptionGUI multiAnswerOptionGUI = answer_prefab.Copy(null, activate: true, "#" + i + ": " + list2[i].id);
			GJL.EnsureChildLabelsHasCorrectFont(multiAnswerOptionGUI.gameObject, do_cache: false);
			GJL.ApplyCustomFontSettings(multiAnswerOptionGUI.gameObject);
			multiAnswerOptionGUI.gameObject.SetActive(value: true);
			_answers.Add(multiAnswerOptionGUI);
		}
		if (flag)
		{
			num = 150;
		}
		else if (num % 2 != 0)
		{
			num++;
		}
		float num3 = 0f;
		for (int j = 0; j < list2.Count; j++)
		{
			float num4 = anim_delay * (float)(list2.Count - j - 1);
			if (num3.EqualsTo(0f))
			{
				num3 = num4 + anim_time + 0.02f;
			}
			if (_answers[j] == null)
			{
				Debug.LogError("answer #" + j + " is null");
			}
			else
			{
				_answers[j].Show(list2[j], j == 0, j == list2.Count - 1, num, this, anim_time, num4);
			}
		}
		_all_anims_finished = num3.EqualsTo(0f);
		corners[0] = _answers.LastElement().ld.gameObject;
		corners[1] = _answers.LastElement().rd.gameObject;
		corners[2] = _answers[0].lu.gameObject;
		corners[3] = _answers[0].ru.gameObject;
		try_show_to_left = show_to_left;
		current_corner_index = -1;
		base.Init();
		base.gameObject.SetActive(value: true);
		GJL.ApplyCustomFontSettings(base.gameObject);
		table.Reposition();
		OnContentChanged();
		LateUpdate();
		Update();
		_need_reposition = true;
		if (BaseGUI.for_gamepad)
		{
			gamepad_controller.ReinitItems(focus_on_first_active: false);
		}
		GJTimer.AddTimer(num3 - anim_time, delegate
		{
			if (!_all_anims_finished)
			{
				_all_anims_finished = true;
				if (BaseGUI.for_gamepad)
				{
					gamepad_controller.FocusOnFirstActive();
				}
			}
		});
	}

	public override void LateUpdate()
	{
		if (linked_tf != null)
		{
			UpdateBubble(linked_tf.position, use_world_cam: true);
		}
	}

	private void Update()
	{
		if (_need_reposition)
		{
			table.Reposition();
			_need_reposition = true;
		}
		OnContentChanged();
		if (!BaseGUI.for_gamepad)
		{
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Up))
		{
			LazyInput.ClearKeyDown(GameKey.Up);
			if (_all_anims_finished)
			{
				gamepad_controller.Navigate(Direction.Up);
			}
		}
		if (LazyInput.GetKeyDown(GameKey.Down))
		{
			LazyInput.ClearKeyDown(GameKey.Down);
			if (_all_anims_finished)
			{
				gamepad_controller.Navigate(Direction.Down);
			}
		}
		if (LazyInput.GetKeyDown(GameKey.Select))
		{
			LazyInput.ClearAllKeysDown();
			if (_all_anims_finished)
			{
				gamepad_controller.SelectFocusedItem();
			}
			else
			{
				SkipAnimations();
			}
		}
	}

	private void SkipAnimations()
	{
		if (_all_anims_finished)
		{
			return;
		}
		_all_anims_finished = true;
		foreach (MultiAnswerOptionGUI answer in _answers)
		{
			answer.FinishAnimation();
		}
		if (BaseGUI.for_gamepad)
		{
			gamepad_controller.FocusOnFirstActive();
		}
	}

	public void OnChosen(string answer)
	{
		Debug.Log("MultiAnswer OnChosen: " + answer);
		_on_chosen(answer);
		StartDisappear();
	}

	public static void ShowAnswers(List<AnswerVisualData> answers, Transform link, MultiAnswerResult on_chosen, bool show_to_left = false, GJCommons.VoidDelegate on_disappeared = null, WorldGameObject talker = null)
	{
		if (_me == null)
		{
			Debug.LogError("MultiAnswer.ShowAnswers error: _me is null");
			return;
		}
		MultiAnswerGUI multiAnswerGUI = (_current = _me.Copy());
		talker_wgo = talker;
		multiAnswerGUI.linked_tf = link;
		multiAnswerGUI.ShowAnswers(answers, show_to_left);
		multiAnswerGUI._on_chosen = on_chosen;
		multiAnswerGUI.on_disappeared = on_disappeared;
	}

	public override void DestroyBubble()
	{
		if (this == _me)
		{
			Debug.LogError("Error: trying to destroy _me MultiAnswerGUI");
			return;
		}
		base.DestroyBubble();
		_current = null;
	}

	public static void HideAnyctive()
	{
		if (_current != null)
		{
			_current.DestroyBubble();
		}
	}
}
