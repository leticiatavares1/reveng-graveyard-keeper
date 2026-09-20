using UnityEngine;

public class NPCListQuestText : MonoBehaviour
{
	public UILabel txt;

	public void Draw(KnownNPC.TaskState task_state)
	{
		string text = " ";
		if (GJL.IsEastern())
		{
			text = "";
		}
		if (task_state.is_dlc_stories_task)
		{
			txt.text = "(*2)" + text + task_state.GetTaskText();
		}
		else if (task_state.is_dlc_refugee_task)
		{
			txt.text = "(q_marker_refugee)" + text + task_state.GetTaskText();
		}
		else if (task_state.is_dlc_souls_task)
		{
			txt.text = "(q_souls)" + text + task_state.GetTaskText();
		}
		else
		{
			txt.text = "(*)" + text + task_state.GetTaskText();
		}
	}
}
