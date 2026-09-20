using System;
using System.Collections.Generic;

[Serializable]
public class KnownNPC
{
	[Serializable]
	public class TaskState
	{
		[Serializable]
		public enum State
		{
			Visible,
			Complete,
			Unknown
		}

		public State state;

		public string id;

		public bool is_dlc_stories_task => id.StartsWith("dlc_stories_");

		public bool is_dlc_refugee_task
		{
			get
			{
				if (!id.StartsWith("dlc_refugees"))
				{
					return id.StartsWith("s_ev");
				}
				return true;
			}
		}

		public bool is_dlc_souls_task => id.StartsWith("dlc_souls");

		public string GetTaskText()
		{
			return LocalizedLabel.ColorizeTags(GJL.L("task_" + id), LocalizedLabel.TextColor.SpeechBubble);
		}
	}

	public string npc_id;

	public List<TaskState> tasks = new List<TaskState>();

	public int sort_order
	{
		get
		{
			if (npc_id == "player")
			{
				return 0;
			}
			return GameBalance.me.GetDataOrNull<ObjectDefinition>(npc_id)?.sort_n ?? 999999;
		}
	}

	public void SetQuestState(string task_id, TaskState.State state)
	{
		Stats.DesignEvent("Task:" + task_id + ":" + state);
		foreach (TaskState task in tasks)
		{
			if (task.id == task_id)
			{
				task.state = state;
				return;
			}
		}
		tasks.Add(new TaskState
		{
			id = task_id,
			state = state
		});
	}

	public TaskState.State GetQuestState(string task_id)
	{
		foreach (TaskState task in tasks)
		{
			if (task.id == task_id)
			{
				return task.state;
			}
		}
		return TaskState.State.Unknown;
	}
}
