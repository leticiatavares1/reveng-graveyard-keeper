using System;
using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

[Serializable]
public class SerializedWalker
{
	[SerializeField]
	public int thickness = 4;

	[SerializeField]
	public int step_length = 4;

	[SerializeField]
	public int max_steps_between_rooms = 3;

	[SerializeField]
	public int min_length = 32;

	[SerializeField]
	public int max_length = 512;

	[SerializeField]
	public List<SerializedRoom> rooms = new List<SerializedRoom>();

	[SerializeField]
	public DungeonWalker.ActionChances action_chances = new DungeonWalker.ActionChances();
}
