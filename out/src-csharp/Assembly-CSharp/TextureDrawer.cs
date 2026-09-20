using System.Collections.Generic;
using System.Linq;
using DungeonGenerator;
using UnityEngine;

public class TextureDrawer : MonoBehaviour
{
	public const int TEXTURE_SIZE_IN_BITS = 8;

	public const int TEXTURE_SIZE = 256;

	public const float DEAD_MOBS_PERCENT_FOR_DUNGEON_COMPLETING = 90f;

	public int random_seed = -1;

	public int mobs_level = 1;

	public Tileset tileset;

	public MeshRenderer mesh_renderer;

	public List<WorldGameObject> spawned_mobs;

	public IntVector2 enter_point = new IntVector2(128, 128);

	public GameObject enter_to_dunge;

	public List<DungeonRoom> main_walker_rooms = new List<DungeonRoom>();

	[HideInInspector]
	public int main_walker_thickness = 3;

	[HideInInspector]
	public int main_walker_step_length = 6;

	[HideInInspector]
	public int main_walker_min_path = 128;

	[HideInInspector]
	public int main_walker_max_path = 256;

	[HideInInspector]
	public int main_walker_max_steps_between_rooms = 3;

	[HideInInspector]
	public int sub_walkers_count = 3;

	[HideInInspector]
	public int room_borders = 2;

	public DungeonPreset cur_dungeon_preset;

	public Dungeon cur_dungeon;

	public SavedDungeon cur_saved_dungeon;

	public DungeonWalker.ActionChances main_walker_chances = new DungeonWalker.ActionChances();

	public bool dungeon_is_loaded_now;

	public DungeonStatistics statistics = new DungeonStatistics();

	public void DrawTexture(DungeonPreset dungeon_preset = null)
	{
		if (mesh_renderer == null)
		{
			Debug.LogError("Mesh renderer is null!");
			return;
		}
		Texture2D texture2D = mesh_renderer.GetComponent<Texture2D>();
		if (texture2D == null || texture2D.width != 256)
		{
			texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, mipChain: false);
			texture2D.filterMode = FilterMode.Point;
		}
		Color[] array = new Color[65536];
		DestroyTiles();
		if (dungeon_preset == null)
		{
			Debug.LogError("Can not generate dungeon: dungeon preset is null!");
			return;
		}
		cur_dungeon_preset = dungeon_preset;
		tileset = cur_dungeon_preset.GetTileset();
		if (Application.isPlaying)
		{
			cur_saved_dungeon = MainGame.me.save.dungeons.GetSavedDungeon(cur_dungeon_preset.dungeon_level);
		}
		else
		{
			cur_saved_dungeon = new SavedDungeon();
		}
		if (cur_saved_dungeon == null)
		{
			Debug.LogError("Dungeon save is null!");
			return;
		}
		if (cur_saved_dungeon.seed == -1)
		{
			if (Application.isPlaying)
			{
				cur_saved_dungeon.seed = MainGame.me.save.dungeons.GetDungeonSeed(cur_dungeon_preset.dungeon_level);
			}
			else
			{
				cur_saved_dungeon.seed = ((random_seed == -1) ? Random.Range(0, 10000) : random_seed);
			}
		}
		if (statistics == null)
		{
			statistics = new DungeonStatistics();
		}
		statistics.SetDefault();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Dungeon dungeon = cur_dungeon_preset.GenerateDungeon(cur_saved_dungeon, need_log: true);
		Debug.Log("#dgen# Dungeon generation time = " + (Time.realtimeSinceStartup - realtimeSinceStartup));
		Debug.Log(statistics.ToString());
		int[,] matrix = dungeon.GetMatrix();
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				Color color = (((i + j) % 2 == 1) ? Color.gray : Color.black);
				if (matrix[i, j] == 1)
				{
					color = Color.green;
				}
				else if (matrix[i, j] == 2)
				{
					color = Color.blue;
				}
				array[i + j * 256] = color;
			}
		}
		ArrangeTiles(matrix);
		PlaceRooms(dungeon, out var mob_spawners, cur_saved_dungeon.objects);
		spawned_mobs = ActivateMobSpawners(mob_spawners, cur_dungeon_preset.dungeon_level, cur_saved_dungeon.objects);
		InitAllOptimizedColliders();
		cur_dungeon = dungeon;
		texture2D.SetPixels(array);
		texture2D.Apply();
		if (!dungeon_is_loaded_now)
		{
			GJTimer.AddTimer(0.1f, ImportGDPoints);
		}
		mesh_renderer.material.mainTexture = texture2D;
		dungeon_is_loaded_now = true;
		ChunkedGameObject[] componentsInChildren = base.gameObject.GetComponentsInChildren<ChunkedGameObject>(includeInactive: true);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].is_temp = true;
		}
	}

	private void ArrangeTiles(int[,] dunge_matrix)
	{
		if (tileset == null)
		{
			return;
		}
		Debug.Log("#dgen# Started arranging tiles.");
		for (int i = 1; i < 255; i++)
		{
			for (int j = 1; j < 255; j++)
			{
				if (dunge_matrix[i, j] != 1)
				{
					continue;
				}
				bool need_mirror = false;
				WorldSimpleObject worldSimpleObject = null;
				if (dunge_matrix[i - 1, j] != 0 && dunge_matrix[i + 1, j] != 0 && dunge_matrix[i, j - 1] != 0 && dunge_matrix[i, j + 1] != 0)
				{
					if (dunge_matrix[i + 1, j + 1] == 0)
					{
						worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerInDownLeft, out need_mirror);
					}
					else if (dunge_matrix[i + 1, j - 1] == 0)
					{
						worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerInUpLeft, out need_mirror);
					}
					else if (dunge_matrix[i - 1, j + 1] == 0)
					{
						worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerInDownRight, out need_mirror);
					}
					else if (dunge_matrix[i - 1, j - 1] == 0)
					{
						worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerInUpRight, out need_mirror);
					}
				}
				else if (dunge_matrix[i - 1, j] != 0 && dunge_matrix[i + 1, j] != 0 && dunge_matrix[i, j - 1] != 0 && dunge_matrix[i, j + 1] == 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.Up, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] != 0 && dunge_matrix[i + 1, j] != 0 && dunge_matrix[i, j - 1] == 0 && dunge_matrix[i, j + 1] != 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.Down, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] == 0 && dunge_matrix[i + 1, j] != 0 && dunge_matrix[i, j - 1] != 0 && dunge_matrix[i, j + 1] != 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.Left, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] != 0 && dunge_matrix[i + 1, j] == 0 && dunge_matrix[i, j - 1] != 0 && dunge_matrix[i, j + 1] != 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.Right, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] == 0 && dunge_matrix[i + 1, j] != 0 && dunge_matrix[i, j - 1] != 0 && dunge_matrix[i, j + 1] == 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerOutUpLeft, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] != 0 && dunge_matrix[i + 1, j] == 0 && dunge_matrix[i, j - 1] != 0 && dunge_matrix[i, j + 1] == 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerOutUpRight, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] == 0 && dunge_matrix[i + 1, j] != 0 && dunge_matrix[i, j - 1] == 0 && dunge_matrix[i, j + 1] != 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerOutDownLeft, out need_mirror);
				}
				else if (dunge_matrix[i - 1, j] != 0 && dunge_matrix[i + 1, j] == 0 && dunge_matrix[i, j - 1] == 0 && dunge_matrix[i, j + 1] != 0)
				{
					worldSimpleObject = tileset.GetTilePrefab(Tileset.WallType.CornerOutDownRight, out need_mirror);
				}
				if (worldSimpleObject != null)
				{
					WorldSimpleObject worldSimpleObject2 = Object.Instantiate(worldSimpleObject, base.gameObject.transform, worldPositionStays: false);
					if (worldSimpleObject2 == null)
					{
						Debug.LogError("Can not Instantiate tile: " + worldSimpleObject.name);
						continue;
					}
					worldSimpleObject2.transform.localPosition = new Vector2(i, j);
					if (need_mirror)
					{
						Vector3 localScale = worldSimpleObject2.transform.localScale;
						localScale.x *= -1f;
						worldSimpleObject2.transform.localScale = localScale;
					}
				}
				Object.Instantiate(tileset.GetTilePrefab(Tileset.WallType.Floor, out var _), base.gameObject.transform, worldPositionStays: false).transform.localPosition = new Vector2(i, j);
			}
		}
	}

	public void PlaceRooms(Dungeon generated_dungeon, out List<MobSpawner> mob_spawners, List<SavedDungeonObject> saved_objs)
	{
		mob_spawners = new List<MobSpawner>();
		if (tileset == null)
		{
			return;
		}
		mob_spawners = new List<MobSpawner>();
		foreach (DungeonRoom placed_room in generated_dungeon.main_walker.placed_rooms)
		{
			placed_room.room_interior.DrawRoom(base.gameObject.transform, placed_room.coords.ToVector2(), placed_room.enters_coords, generated_dungeon.main_walker.real_thickness, tileset, out var spawners, saved_objs, generated_dungeon.main_walker.placed_rooms.IndexOf(placed_room) == 0);
			mob_spawners.AddRange(spawners);
		}
		foreach (DungeonWalker sub_walker in generated_dungeon.sub_walkers)
		{
			foreach (DungeonRoom placed_room2 in sub_walker.placed_rooms)
			{
				placed_room2.room_interior.DrawRoom(base.gameObject.transform, placed_room2.coords.ToVector2(), placed_room2.enters_coords, sub_walker.real_thickness, tileset, out var spawners2, saved_objs, sub_walker.placed_rooms.IndexOf(placed_room2) == 0);
				mob_spawners.AddRange(spawners2);
			}
		}
	}

	public void DestroyTiles()
	{
		WorldGameObject[] componentsInChildren = base.gameObject.GetComponentsInChildren<WorldGameObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.Destroy();
		}
		WorldSimpleObject[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<WorldSimpleObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].gameObject.Destroy();
		}
		DropResGameObject[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<DropResGameObject>(includeInactive: true);
		foreach (DropResGameObject obj in componentsInChildren3)
		{
			WorldMap.OnDropItemRemoved(obj.res);
			obj.is_collected = true;
			obj.DestroyLinkedHint();
		}
		if (dungeon_is_loaded_now)
		{
			ExportGDPoints();
		}
		dungeon_is_loaded_now = false;
		cur_dungeon_preset = null;
		cur_saved_dungeon = null;
		GJTimer.AddTimer(0.05f, ChunkManager.RemovePedingTempObjects);
	}

	public List<WorldGameObject> ActivateMobSpawners(List<MobSpawner> mob_spawners, int dungeon_level, List<SavedDungeonObject> saved_objs = null)
	{
		if (saved_objs == null)
		{
			saved_objs = new List<SavedDungeonObject>();
		}
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (MobSpawner mob_spawner in mob_spawners)
		{
			mob_spawner.ActivateSpawner(dungeon_level, saved_objs);
			list.AddRange(mob_spawner.spawned_mobs);
		}
		return list;
	}

	public bool TrySaveDungeon()
	{
		if (cur_dungeon_preset == null)
		{
			Debug.LogError("Current dungeon preset is null!");
			return false;
		}
		if (cur_saved_dungeon == null)
		{
			Debug.LogError("cur_saved_dungeon is null! Loading cur_saved_dungeon #" + cur_dungeon_preset.dungeon_level);
			cur_saved_dungeon = MainGame.me.save.dungeons.GetSavedDungeon(cur_dungeon_preset.dungeon_level);
		}
		if (cur_saved_dungeon == null)
		{
			Debug.LogError("Saved dungeon is null!");
			return false;
		}
		cur_saved_dungeon.dungeon_preset_name = cur_dungeon_preset.name;
		if (cur_saved_dungeon.seed == -1)
		{
			cur_saved_dungeon.seed = MainGame.me.save.dungeons.GetDungeonSeed(cur_dungeon_preset.dungeon_level);
		}
		cur_saved_dungeon.objects = new List<SavedDungeonObject>();
		WorldGameObject[] componentsInChildren = GetComponentsInChildren<WorldGameObject>(includeInactive: true);
		List<BaseCharacterComponent> list = new List<BaseCharacterComponent>();
		WorldGameObject[] array = componentsInChildren;
		foreach (WorldGameObject worldGameObject in array)
		{
			if (!(worldGameObject == null) && !(worldGameObject.obj_id == "0") && !(worldGameObject.obj_id == "empty") && worldGameObject.obj_def != null && worldGameObject.obj_def.type != 0 && worldGameObject.components.character.enabled)
			{
				list.Add(worldGameObject.components.character);
			}
		}
		if (list.Count == 0)
		{
			Debug.Log("Not found any alive mob in dungeon.");
		}
		else
		{
			cur_saved_dungeon.objects = new List<SavedDungeonObject>();
			foreach (BaseCharacterComponent item in list)
			{
				cur_saved_dungeon.objects.Add(new SavedDungeonObject
				{
					name = item.wgo.name,
					local_position = item.spawner_coords,
					type = SavedDungeonObject.SavedDungeonObjectType.Mob,
					mob_is_alive = true
				});
			}
		}
		array = componentsInChildren;
		foreach (WorldGameObject worldGameObject2 in array)
		{
			if (!worldGameObject2.components.character.enabled)
			{
				cur_saved_dungeon.objects.Add(new SavedDungeonObject
				{
					name = worldGameObject2.obj_id,
					local_position = worldGameObject2.transform.localPosition,
					type = SavedDungeonObject.SavedDungeonObjectType.WGO
				});
			}
		}
		cur_saved_dungeon.is_empty = false;
		UpdateDungeonState();
		spawned_mobs = null;
		return true;
	}

	public void InitAllOptimizedColliders()
	{
		OptimizedCollider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			OptimizedCollider2D[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Init();
			}
		}
	}

	public float GetDeadMobsPercent()
	{
		if (spawned_mobs == null || spawned_mobs.Count == 0)
		{
			return 100f;
		}
		int num = 0;
		foreach (WorldGameObject spawned_mob in spawned_mobs)
		{
			if (spawned_mob == null)
			{
				num++;
			}
			else if (spawned_mob.is_dead)
			{
				num++;
			}
		}
		return (float)num / (float)spawned_mobs.Count * 100f;
	}

	public void UpdateDungeonState()
	{
		if (dungeon_is_loaded_now && Application.isPlaying && !(cur_dungeon_preset == null) && cur_saved_dungeon != null && !cur_saved_dungeon.is_completed && GetDeadMobsPercent() > 90f)
		{
			cur_saved_dungeon.is_completed = true;
			Stats.DesignEvent("Dungeon:" + cur_dungeon_preset.dungeon_level + ":Complete");
			MainGame.me.save.quests.CheckKeyQuests("dungeon_completed_" + cur_dungeon_preset.dungeon_level);
		}
	}

	private void ImportGDPoints()
	{
		List<GDPoint> list = MainGame.me.dungeon_root.GetComponentsInChildren<GDPoint>(includeInactive: true).ToList();
		if (list != null && list.Count > 0)
		{
			Debug.Log("Import GDPoints on dungeon, count: " + list.Count);
			WorldMap.ImportGDPointsOnLoadedScene(list);
		}
	}

	private void ExportGDPoints()
	{
		List<GDPoint> list = MainGame.me.dungeon_root.GetComponentsInChildren<GDPoint>(includeInactive: true).ToList();
		if (list != null && list.Count > 0)
		{
			Debug.Log("Export GDPoints on dungeon, count: " + list.Count);
			WorldMap.ExportGDPointsOnUnloadedScene(list);
		}
	}
}
