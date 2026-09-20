using System;
using UnityEngine;

public class HUDTaskItemGUI : MonoBehaviour
{
	public UILabel txt;

	public Transform marker_point;

	public UILabel quest_marker;

	[NonSerialized]
	public KnownNPC.TaskState linked_task;
}
