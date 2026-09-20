using System.Collections.Generic;
using DungeonGenerator;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonPreset", menuName = "DungeonPreset", order = 3)]
public class DungeonPreset : ScriptableObject
{
	public const string DUNGEON_PRESET_PATH = "Dungeon/DungeonPresets/";

	private const int MAX_ITERATIONS = 50000;

	public int room_borders = 2;

	public Tileset tileset;

	public int dungeon_level = 1;

	public SerializedWalker main_walker = new SerializedWalker();

	public List<SerializedWalker> sub_walkers = new List<SerializedWalker>();

	public EnvironmentPreset environment_preset;

	public Tileset GetTileset()
	{
		if (tileset == null)
		{
			return Resources.Load<Tileset>("Dungeon/Tilesets/DungeonTileset");
		}
		return tileset;
	}

	public Dungeon GenerateDungeon(SavedDungeon dungeon_save, bool need_log = false)
	{
		Dungeon dungeon = null;
		Dungeon.SetRandomSeed(dungeon_save.seed, dungeon_save.random_calls_count);
		Debug.Log("#dgen# Started generating dungeon with {preset=" + base.name + "; seed=" + dungeon_save.seed + "; random_calls_count=" + dungeon_save.random_calls_count + "}");
		int num = 0;
		Dungeon.Init();
		DungeonPattern.InitPatternsCache();
		while (true)
		{
			int randomCallsCount = Dungeon.GetRandomCallsCount();
			Dungeon dungeon2 = new Dungeon(256, 256, new IntVector2(128, 128), room_borders);
			int num2 = 3;
			bool flag = false;
			if (main_walker.thickness % 2 == 1)
			{
				num2 = (main_walker.thickness + 1) / 2;
			}
			else
			{
				num2 = (main_walker.thickness + 2) / 2;
				flag = true;
			}
			List<DungeonRoom> list = new List<DungeonRoom>();
			foreach (SerializedRoom room3 in main_walker.rooms)
			{
				DungeonRoom room = room3.GetRoom(list);
				room.given_biom_type = room3.biom_type;
				room.given_room_type = room3.room_type;
				room.given_room_size = room3.room_size;
				room.given_room_interior_name = room3.preset_name;
				list.Add(room);
			}
			new DungeonWalker(t_is_main: true, dungeon2, list, num2, main_walker.step_length, main_walker.action_chances.Copy(), main_walker.min_length, main_walker.max_length, main_walker.max_steps_between_rooms).real_thickness = main_walker.thickness;
			foreach (SerializedWalker sub_walker in sub_walkers)
			{
				List<DungeonRoom> list2 = new List<DungeonRoom>();
				foreach (SerializedRoom room4 in sub_walker.rooms)
				{
					DungeonRoom room2 = room4.GetRoom(list);
					room2.given_biom_type = room4.biom_type;
					room2.given_room_type = room4.room_type;
					room2.given_room_size = room4.room_size;
					room2.given_room_interior_name = room4.preset_name;
					list2.Add(room2);
				}
				new DungeonWalker(t_is_main: false, dungeon2, list2, num2, sub_walker.step_length, sub_walker.action_chances.Copy(), sub_walker.min_length, sub_walker.max_length, sub_walker.max_steps_between_rooms).real_thickness = main_walker.thickness;
			}
			if (num == 1357)
			{
				Debug.Log("#dgen# I'm Here");
			}
			if (dungeon2.TryGenerateDungeon())
			{
				dungeon = dungeon2;
				if (flag)
				{
					Debug.Log("#dgen# Seems like I need postproduction.");
					dungeon.MakePostproduction();
				}
				dungeon_save.random_calls_count = randomCallsCount;
				Debug.Log("#dgen# Finished Dungeon Generator. Iterations = " + (num + 1));
				break;
			}
			num++;
			if (num > 50000)
			{
				Debug.LogError("Stopped Dungeon Generation By Iterator! (iterations: " + num + ")");
				dungeon = dungeon2;
				break;
			}
		}
		return dungeon;
	}
}
