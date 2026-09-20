using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[Serializable]
public class QuestSystem
{
	[SerializeField]
	private List<string> _failed_quests = new List<string>();

	[SerializeField]
	private List<string> _succed_quests = new List<string>();

	[SerializeField]
	private List<QuestState> _currnet_quests = new List<QuestState>();

	[SerializeField]
	private List<string> _executed_quests = new List<string>();

	private Dictionary<string, List<QuestDefinition>> _quests_by_staring_key = new Dictionary<string, List<QuestDefinition>>();

	public bool ending_quests_in_progress;

	public bool starting_quests_in_progress;

	private List<QuestState> _ending_quests = new List<QuestState>();

	private List<QuestDefinition> _quest_ready_to_start = new List<QuestDefinition>();

	public bool IsQuestCurrent(string quest_id)
	{
		return _currnet_quests.FindIndex((QuestState p) => p.definition.id == quest_id) != -1;
	}

	public bool IsQuestFaild(string quest_id)
	{
		return _failed_quests.Contains(quest_id);
	}

	public bool IsQuestSucced(string quest_id)
	{
		return _succed_quests.Contains(quest_id);
	}

	public void GiveReward(bool succeed, QuestDefinition quest)
	{
		if (succeed)
		{
			MainGame.me.player.AddToParams(quest.rew_on_success_params);
		}
		else
		{
			MainGame.me.player.AddToParams(quest.rew_on_fail_params);
		}
	}

	public bool CheckKeyQuests(string key)
	{
		MainGame.me.save.achievements.CheckKeyQuests(key);
		_ = string.Empty;
		bool result = false;
		if (_quests_by_staring_key.ContainsKey(key))
		{
			List<QuestDefinition> list = _quests_by_staring_key[key];
			if (list == null)
			{
				return false;
			}
			foreach (QuestDefinition item in list)
			{
				if (!IsQuestInUse(item) && item.IsReadyToStart(this))
				{
					Debug.Log("quest added to starting list id = " + item.id);
					_quest_ready_to_start.Add(item);
					result = true;
				}
			}
			ProcessNextStartingQuest();
		}
		CheckQuestsState();
		return result;
	}

	public void StartQuest(QuestDefinition quest)
	{
		Debug.Log("Start Quest id = " + quest.id);
		QuestState questState = new QuestState();
		questState.definition = quest;
		_currnet_quests.Add(questState);
		OnQuestStart(questState);
	}

	public void ForceQuestEnd(string quest_id, bool succesfull_finish)
	{
		Debug.Log("Force Quest end id = " + quest_id + ", succesfull_finish = " + succesfull_finish);
		for (int i = 0; i < _currnet_quests.Count; i++)
		{
			QuestState questState = _currnet_quests[i];
			if (questState.definition.id == quest_id)
			{
				_currnet_quests.RemoveAt(i);
				questState.state = (succesfull_finish ? QuestState.State.Succeeded : QuestState.State.InProgress);
				EndQuest(questState);
				return;
			}
		}
		Debug.Log("Couldn't find quest to finish");
	}

	private void OnQuestStart(QuestState quest_state)
	{
		quest_state.definition.InitQuestEndTriggers();
		quest_state.definition.StartStartingScripts();
		OnQuestExecuted(quest_state.definition.id);
		OnStartScriptFinished();
		if (GUIElements.me.quest_list != null)
		{
			GUIElements.me.quest_list.Redraw();
		}
	}

	private void OnQuestExecuted(string quest_id)
	{
		if (!_executed_quests.Contains(quest_id))
		{
			_executed_quests.Add(quest_id);
		}
	}

	public bool CheckIfQuestWasExecuted(string quest_id)
	{
		return _executed_quests.Contains(quest_id);
	}

	public void CheckQuestsState()
	{
		int num = 0;
		while (num < _currnet_quests.Count)
		{
			QuestState questState = _currnet_quests[num];
			questState.state = questState.CheckQuestProgress();
			if (questState.state == QuestState.State.InProgress)
			{
				num++;
				continue;
			}
			_ending_quests.Add(questState);
			_currnet_quests.RemoveAt(num);
		}
		ProcessNextEndingQuest();
	}

	public void ProcessNextEndingQuest()
	{
		if (_ending_quests.Count != 0 && !QuestScriptInProgress())
		{
			ending_quests_in_progress = true;
			QuestState q_to_end = _ending_quests.Last();
			_ending_quests.RemoveAt(_ending_quests.Count - 1);
			EndQuest(q_to_end);
		}
	}

	private void ProcessNextStartingQuest()
	{
		if (_quest_ready_to_start.Count != 0 && !QuestScriptInProgress())
		{
			starting_quests_in_progress = true;
			QuestDefinition quest = _quest_ready_to_start[0];
			_quest_ready_to_start.RemoveAt(0);
			StartQuest(quest);
		}
	}

	private bool QuestScriptInProgress()
	{
		if (starting_quests_in_progress)
		{
			return ending_quests_in_progress;
		}
		return false;
	}

	private void EndQuest(QuestState q_to_end)
	{
		Debug.Log("EndQuest #" + q_to_end.definition.id + ", success = " + q_to_end.state);
		if (q_to_end.state == QuestState.State.Succeeded)
		{
			OnQuestSucceed(q_to_end);
		}
		else
		{
			OnQuestFailed(q_to_end);
		}
		MainGame.me.save.quests.CheckKeyQuests("quest_finished");
		if (GUIElements.me.quest_list != null)
		{
			GUIElements.me.quest_list.Redraw();
		}
	}

	private void OnQuestSucceed(QuestState q_to_end)
	{
		Debug.Log("OnQuestSucceed " + q_to_end.definition.id);
		_succed_quests.Add(q_to_end.definition.id);
		GiveReward(succeed: true, q_to_end.definition);
		q_to_end.definition.ExpressionsSuccess();
		q_to_end.definition.StartSucceedScript();
		OnEndScriptFinished();
	}

	private void OnQuestFailed(QuestState q_to_end)
	{
		Debug.Log("OnQuestFailed " + q_to_end.definition.id);
		_failed_quests.Add(q_to_end.definition.id);
		GiveReward(succeed: false, q_to_end.definition);
		q_to_end.definition.ExpressionsFail();
		q_to_end.definition.StartFailedScript();
		OnEndScriptFinished();
	}

	public bool IsQuestInUse(QuestDefinition quest)
	{
		if ((!_failed_quests.Contains(quest.id) && !_succed_quests.Contains(quest.id)) || !quest.one_time_quest)
		{
			return IsQuestCurrent(quest.id);
		}
		return true;
	}

	public void InitQuestSystem()
	{
		foreach (QuestDefinition quests_datum in GameBalance.me.quests_data)
		{
			quests_datum.InitQuestStartTriggers();
		}
		FillQuestCheckDic();
	}

	private void FillQuestCheckDic()
	{
		_quests_by_staring_key.Clear();
		foreach (QuestDefinition quests_datum in GameBalance.me.quests_data)
		{
			if (IsQuestInUse(quests_datum))
			{
				continue;
			}
			foreach (string item in quests_datum.start_key)
			{
				if (!_quests_by_staring_key.ContainsKey(item))
				{
					_quests_by_staring_key.Add(item, new List<QuestDefinition>());
				}
				_quests_by_staring_key[item].Add(quests_datum);
			}
		}
	}

	public void OnStartScriptFinished()
	{
		starting_quests_in_progress = false;
		ProcessNextStartingQuest();
	}

	public void OnEndScriptFinished()
	{
		ending_quests_in_progress = false;
		ProcessNextEndingQuest();
	}

	public List<QuestState> GetCurrentQuests()
	{
		return _currnet_quests;
	}
}
