using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

namespace DungeonGenerator;

public class DungeonWalker
{
	public enum Direction
	{
		Left,
		Up,
		Right,
		Down
	}

	[Serializable]
	public class ActionChances
	{
		public enum ActionType
		{
			DoStep,
			TurnLeft,
			TurnRight,
			PlaceRoom,
			Finish
		}

		public static readonly int max_actions = (int)(Enum.GetValues(typeof(ActionType)).Cast<ActionType>().Max() + 1);

		[SerializeField]
		public float[] chances = new float[max_actions];

		public ActionChances()
		{
			chances[0] = 0.5f;
			chances[1] = 0.225f;
			chances[2] = 0.225f;
			chances[3] = 0.05f;
			chances[4] = 0f;
		}

		public ActionChances(float do_step_chance, float turn_left_chance, float turn_right_chance, float place_room_chance)
		{
			chances[0] = do_step_chance;
			chances[1] = turn_left_chance;
			chances[2] = turn_right_chance;
			chances[3] = place_room_chance;
			chances[4] = 0f;
		}

		public ActionType GetTurn()
		{
			ActionType actionType = ((chances[1] >= chances[2]) ? ActionType.TurnLeft : ActionType.TurnRight);
			chances[1] += ((actionType == ActionType.TurnLeft) ? (-0.1f) : 0.1f);
			chances[2] += ((actionType == ActionType.TurnRight) ? (-0.1f) : 0.1f);
			return actionType;
		}

		public ActionType GetAction(bool use_finish = false)
		{
			ActionType actionType = ActionType.Finish;
			float num = 0f;
			for (int i = 0; i < max_actions; i++)
			{
				if (use_finish || i != 4)
				{
					num += chances[i];
				}
			}
			float num2 = Dungeon.RandomRange(0f, num);
			float num3 = 0f;
			for (int j = 0; j < max_actions; j++)
			{
				num3 += chances[j];
				if (num3 > num2)
				{
					actionType = (ActionType)j;
					break;
				}
			}
			switch (actionType)
			{
			case ActionType.DoStep:
				ChangeChance(ActionType.DoStep, -0.05f);
				ChangeChance(ActionType.PlaceRoom, 0.05f);
				break;
			case ActionType.TurnLeft:
				ChangeChance(ActionType.TurnLeft, -0.1f);
				ChangeChance(ActionType.TurnRight, 0.1f);
				break;
			case ActionType.TurnRight:
				ChangeChance(ActionType.TurnLeft, 0.1f);
				ChangeChance(ActionType.TurnRight, -0.1f);
				break;
			case ActionType.PlaceRoom:
				ChangeChance(ActionType.DoStep, chances[3]);
				chances[3] = 0f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case ActionType.Finish:
				break;
			}
			return actionType;
		}

		public void ChangeChance(ActionType type, float delta)
		{
			chances[(int)type] = ((chances[(int)type] + delta < 0f) ? 0f : ((chances[(int)type] + delta > 1f) ? 1f : (chances[(int)type] + delta)));
		}

		public override string ToString()
		{
			string text = string.Empty;
			foreach (ActionType value in Enum.GetValues(typeof(ActionType)))
			{
				text = text + "\n" + value.ToString() + "  \t= " + chances[(int)value];
			}
			return text;
		}

		public ActionChances Copy()
		{
			return new ActionChances(chances[0], chances[1], chances[2], chances[3]);
		}
	}

	public Dungeon dunge;

	public bool is_main_walker = true;

	public bool is_finished;

	public bool is_correct;

	public int min_path_length;

	public int max_path_length;

	public int max_steps_between_rooms;

	public int steps_after_placed_room;

	public int failed_steps_after_placed_room;

	public int cur_path;

	public int thickness;

	public int real_thickness;

	private DungeonPattern _pattern;

	public int step_length;

	public List<DungeonRoom> rooms;

	public List<DungeonRoom> placed_rooms;

	public ActionChances chances;

	public List<ActionChances.ActionType> last_tick_action_type_list = new List<ActionChances.ActionType>();

	public Direction cur_direction = Direction.Up;

	public IntVector2 cur_position = new IntVector2();

	public ActionChances.ActionType last_tick_action_type
	{
		get
		{
			if (last_tick_action_type_list == null || last_tick_action_type_list.Count == 0)
			{
				return ActionChances.ActionType.Finish;
			}
			return last_tick_action_type_list[last_tick_action_type_list.Count - 1];
		}
		set
		{
			if (last_tick_action_type_list == null)
			{
				last_tick_action_type_list = new List<ActionChances.ActionType>();
			}
			last_tick_action_type_list.Add(value);
			if (last_tick_action_type_list.Count > 20)
			{
				last_tick_action_type_list.RemoveAt(0);
			}
		}
	}

	public DungeonWalker(bool t_is_main, Dungeon t_dunge, List<DungeonRoom> t_rooms, int t_thickness = 1, int t_step_length = 1, ActionChances t_chances = null, int t_min_length = 128, int t_max_length = 256, int t_max_steps_between_rooms = 3)
	{
		is_main_walker = t_is_main;
		dunge = t_dunge;
		rooms = t_rooms;
		placed_rooms = new List<DungeonRoom>();
		if (t_chances == null)
		{
			t_chances = new ActionChances();
		}
		chances = t_chances;
		if (is_main_walker)
		{
			cur_position = dunge.enter_to_dunge.Copy();
			dunge.main_walker = this;
		}
		else
		{
			cur_position = new IntVector2(-1, -1);
			dunge.sub_walkers.Add(this);
		}
		min_path_length = t_min_length;
		max_path_length = t_max_length;
		thickness = t_thickness;
		_pattern = DungeonPattern.GetPattern(thickness);
		step_length = t_step_length;
		max_steps_between_rooms = t_max_steps_between_rooms;
	}

	public bool TryMarkStepPattern()
	{
		if (IsWrongPosition())
		{
			return false;
		}
		int num = cur_position.x - thickness + 1;
		int num2 = cur_position.y - thickness + 1;
		for (int i = 0; i <= _pattern.ymax; i++)
		{
			int num3 = i << 8;
			int y = num2 + i;
			int num4 = num;
			for (int j = 0; j <= _pattern.xmax; j++)
			{
				if (_pattern.step_pattern[num3] && dunge.GetCellType(num4, y) != Dungeon.CellType.Room)
				{
					dunge.TrySetCellType(num4, y, Dungeon.CellType.Corridor);
				}
				num3++;
				num4++;
			}
		}
		return true;
	}

	public void CalculateTick()
	{
		if (IsWrongPosition())
		{
			is_finished = true;
			return;
		}
		ActionChances.ActionType actionType = ActionChances.ActionType.Finish;
		bool use_finish = cur_path > min_path_length;
		if (is_main_walker && placed_rooms.Count == 0)
		{
			actionType = ActionChances.ActionType.PlaceRoom;
		}
		else if (is_main_walker || placed_rooms.Count != 0 || steps_after_placed_room != 0)
		{
			actionType = ((cur_path > max_path_length || IsDeadlock()) ? ActionChances.ActionType.Finish : (IsWrongDirection() ? chances.GetTurn() : ((placed_rooms.Count < rooms.Count && steps_after_placed_room >= max_steps_between_rooms) ? ((placed_rooms.Count == rooms.Count - 1) ? ActionChances.ActionType.Finish : ActionChances.ActionType.PlaceRoom) : ((last_tick_action_type == ActionChances.ActionType.DoStep) ? chances.GetAction(use_finish) : ActionChances.ActionType.DoStep))));
		}
		else
		{
			if (IsWrongDirection())
			{
				MainGame.me.dungeon_root.statistics.is_wrong_direction++;
				is_correct = false;
				is_finished = true;
				return;
			}
			actionType = ActionChances.ActionType.DoStep;
		}
		switch (actionType)
		{
		case ActionChances.ActionType.DoStep:
			if (!TryDoStep())
			{
				Debug.LogError("Can not do step!");
				break;
			}
			cur_path++;
			steps_after_placed_room++;
			break;
		case ActionChances.ActionType.TurnLeft:
			TurnLeft();
			if (IsWrongDirection())
			{
				TurnRight();
			}
			else
			{
				if (!(Dungeon.RandomRange(0f, 1f) < 0.25f))
				{
					break;
				}
				foreach (DungeonWalker sub_walker in dunge.sub_walkers)
				{
					if (sub_walker.cur_position.x == -1 || sub_walker.cur_position.y == -1)
					{
						sub_walker.cur_position = cur_position.Copy();
						break;
					}
				}
			}
			break;
		case ActionChances.ActionType.TurnRight:
			TurnRight();
			if (IsWrongDirection())
			{
				TurnLeft();
			}
			else
			{
				if (!(Dungeon.RandomRange(0f, 1f) < 0.25f))
				{
					break;
				}
				foreach (DungeonWalker sub_walker2 in dunge.sub_walkers)
				{
					if (sub_walker2.cur_position.x == -1 || sub_walker2.cur_position.y == -1)
					{
						sub_walker2.cur_position = cur_position.Copy();
						break;
					}
				}
			}
			break;
		case ActionChances.ActionType.PlaceRoom:
		{
			bool flag = false;
			for (int i = 0; i < 3; i++)
			{
				if (TryPlaceRoom())
				{
					flag = false;
					break;
				}
				flag = true;
				if (!rooms[placed_rooms.Count].TryChangeRoomInterior(cur_direction, 2 * thickness - 1, placed_rooms))
				{
					break;
				}
			}
			if (flag)
			{
				if (failed_steps_after_placed_room > max_steps_between_rooms / 2)
				{
					MainGame.me.dungeon_root.statistics.wrong_position_while_plasing_room++;
					is_finished = true;
					is_correct = false;
				}
				else
				{
					steps_after_placed_room--;
					failed_steps_after_placed_room++;
				}
			}
			break;
		}
		case ActionChances.ActionType.Finish:
			is_finished = true;
			if (placed_rooms.Count != rooms.Count - 1)
			{
				MainGame.me.dungeon_root.statistics.finish_walker_but_not_placed_all_rooms++;
				is_correct = false;
				break;
			}
			if (TryPlaceRoom(last_room: true))
			{
				is_correct = true;
				break;
			}
			TurnLeft();
			if (TryPlaceRoom(last_room: true))
			{
				is_correct = true;
				break;
			}
			TurnRight();
			TurnRight();
			is_correct = TryPlaceRoom(last_room: true);
			if (!is_correct)
			{
				MainGame.me.dungeon_root.statistics.walker_can_not_place_last_room++;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		last_tick_action_type = actionType;
	}

	public bool TryPlaceRoom(bool last_room = false)
	{
		if (placed_rooms.Count >= rooms.Count)
		{
			MainGame.me.dungeon_root.statistics.pre_placed_all_rooms++;
			return false;
		}
		bool flag = is_main_walker && placed_rooms.Count == 0;
		DungeonRoom dungeonRoom = rooms[placed_rooms.Count];
		IntVector2 intVector = new IntVector2(-1, -1);
		int num = cur_position.y - thickness + 1;
		int num2 = cur_position.x - thickness + 1;
		if (placed_rooms.Count == rooms.Count - 1 && !last_room)
		{
			MainGame.me.dungeon_root.statistics.pre_placing_not_last_room++;
			return false;
		}
		Direction t_direction = ((cur_direction > Direction.Up) ? (cur_direction - 2) : (cur_direction + 2));
		List<IntVector2> possibleEnters = dungeonRoom.room_interior.GetPossibleEnters(t_direction, 2 * thickness - 1);
		if (possibleEnters.Count == 0)
		{
			MainGame.me.dungeon_root.statistics.pre_no_possible_enters++;
			return false;
		}
		foreach (IntVector2 item in possibleEnters)
		{
			intVector = cur_direction switch
			{
				Direction.Left => new IntVector2(cur_position.x - dungeonRoom.room_width, num - item.y), 
				Direction.Up => new IntVector2(num2 - item.x, cur_position.y + 1), 
				Direction.Right => new IntVector2(cur_position.x + 1, num - item.y), 
				Direction.Down => new IntVector2(num2 - item.x, cur_position.y - dungeonRoom.room_height), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			if (RoomCanBePlaced(dungeonRoom, intVector, cur_direction))
			{
				if (!flag)
				{
					dungeonRoom.enters_coords.Add(item.Copy());
				}
				break;
			}
			if (possibleEnters.IndexOf(item) == possibleEnters.Count - 1)
			{
				MainGame.me.dungeon_root.statistics.pre_not_found_proper_enter++;
				return false;
			}
		}
		if (intVector.x <= 1 || intVector.y <= 1 || intVector.x + dungeonRoom.room_width >= dunge.dungeon_width || intVector.y + dungeonRoom.room_height >= dunge.dungeon_height)
		{
			MainGame.me.dungeon_root.statistics.touched_borders_4++;
			return false;
		}
		steps_after_placed_room = 0;
		failed_steps_after_placed_room = 0;
		dungeonRoom.is_placed = true;
		dungeonRoom.coords = intVector;
		for (int i = 0; i < dungeonRoom.room_width; i++)
		{
			for (int j = 0; j < dungeonRoom.room_height; j++)
			{
				dunge.TrySetCellType(intVector.x + i, intVector.y + j, Dungeon.CellType.Room);
			}
		}
		if (Dungeon.RandomRange(0f, 1f) < 0.15f)
		{
			foreach (DungeonWalker sub_walker in dunge.sub_walkers)
			{
				if (sub_walker.cur_position.x == -1 || sub_walker.cur_position.y == -1)
				{
					if (sub_walker.SetNewPositionAfterRoomPlaced(dungeonRoom, intVector) && !sub_walker.TryMarkStepPattern())
					{
						MainGame.me.dungeon_root.statistics.not_correctly_placed_sub_walker_after_room_placing++;
						is_correct = false;
						is_finished = true;
						Debug.LogError("Wrong placed sub walker: sub_walker=" + sub_walker.cur_position?.ToString() + "; place_for_room=" + intVector);
						return false;
					}
					break;
				}
			}
		}
		if (!last_room && !SetNewPositionAfterRoomPlaced(dungeonRoom, intVector))
		{
			MainGame.me.dungeon_root.statistics.not_enough_exits_from_room++;
			return false;
		}
		if (!TryMarkStepPattern())
		{
			MainGame.me.dungeon_root.statistics.can_not_mark_step_pattern_after_room_placing++;
			is_correct = false;
			is_finished = true;
			return false;
		}
		placed_rooms.Add(dungeonRoom);
		if (is_main_walker)
		{
			_ = placed_rooms.Count;
			_ = 1;
			return true;
		}
		return true;
	}

	public bool SetNewPositionAfterRoomPlaced(DungeonRoom placed_room, IntVector2 room_place)
	{
		List<IntVector2>[] array = new List<IntVector2>[4];
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			array[i] = GetEntersWithoutIntersections(placed_room, (Direction)i);
			num += array[i].Count;
		}
		if (num == 0)
		{
			return false;
		}
		int num2 = Dungeon.RandomRange(0, num);
		IntVector2 intVector = null;
		int num3 = -1;
		for (int j = 0; j < 4; j++)
		{
			if (num2 >= array[j].Count)
			{
				num2 -= array[j].Count;
				continue;
			}
			intVector = array[j][num2].Copy();
			num3 = j;
		}
		if (intVector == null)
		{
			Debug.LogError("FATAL ERROR! exit_coords == null!");
			return false;
		}
		if (num3 == -1)
		{
			Debug.LogError("FATAL ERROR! exit_direction == -1");
			return false;
		}
		IntVector2 intVector2 = intVector + room_place;
		cur_direction = (Direction)num3;
		int num4 = -1;
		int num5 = -1;
		switch (cur_direction)
		{
		case Direction.Left:
			num4 = intVector2.x - 1;
			num5 = intVector2.y + thickness - 1;
			break;
		case Direction.Up:
			num4 = intVector2.x + thickness - 1;
			num5 = intVector2.y + 1;
			break;
		case Direction.Right:
			num4 = intVector2.x + 1;
			num5 = intVector2.y + thickness - 1;
			break;
		case Direction.Down:
			num4 = intVector2.x + thickness - 1;
			num5 = intVector2.y - 1;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		cur_position.x = num4;
		cur_position.y = num5;
		placed_room.enters_coords.Add(intVector);
		return true;
	}

	private List<IntVector2> GetEntersWithoutIntersections(DungeonRoom placed_room, Direction t_direction)
	{
		int num = 2 * thickness - 1;
		List<IntVector2> possibleEnters = placed_room.room_interior.GetPossibleEnters(t_direction, num);
		List<IntVector2> enters_coords = placed_room.enters_coords;
		List<IntVector2> list = new List<IntVector2>();
		for (int i = 0; i < possibleEnters.Count; i++)
		{
			IntVector2 intVector = possibleEnters[i];
			bool flag = false;
			foreach (IntVector2 item in enters_coords)
			{
				if (intVector.x == item.x)
				{
					if (Mathf.Abs(intVector.y - item.y) <= num)
					{
						flag = true;
						break;
					}
				}
				else if (intVector.y == item.y && Mathf.Abs(intVector.x - item.x) <= num)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(intVector);
			}
		}
		return list;
	}

	public bool RoomCanBePlaced(DungeonRoom room, IntVector2 place, Direction direction)
	{
		if (room.room_width < 2 * thickness - 1 || room.room_height < 2 * thickness - 1)
		{
			Debug.LogError("Wrong thickness! room.name=\"" + room.room_interior.name + "\"; [width, height]=[" + room.room_width + ", " + room.room_height + "]; thickness=" + (2 * thickness - 1));
			return false;
		}
		for (int i = 0; i < dunge.room_borders; i++)
		{
			if (!CheckBorders(room.room_width + 2 * i, room.room_height + 2 * i, new IntVector2(place.x - i, place.y - i), direction))
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckBorders(int room_width, int room_height, IntVector2 place, Direction direction)
	{
		if (place.x < 1 || place.y < 1 || place.x + room_width >= dunge.dungeon_width - 1 || place.y + room_height >= dunge.dungeon_height - 1)
		{
			MainGame.me.dungeon_root.statistics.touched_borders_5++;
			return false;
		}
		int num = cur_position.y + thickness - 1;
		int num2 = cur_position.y - thickness + 1;
		int num3 = cur_position.x + thickness - 1;
		int num4 = cur_position.x - thickness + 1;
		switch (direction)
		{
		case Direction.Left:
		{
			for (int m = 0; m < room_height + 2; m++)
			{
				if (!dunge.IsEmptyCell(place.x - 1, place.y - 1 + m))
				{
					return false;
				}
				if ((place.y - 1 + m > num || place.y - 1 + m < num2) && !dunge.IsEmptyCell(place.x + room_height, place.y - 1 + m))
				{
					return false;
				}
			}
			for (int n = 0; n < room_width + 1; n++)
			{
				if (!dunge.IsEmptyCell(place.x + n, place.y - 1))
				{
					return false;
				}
				if (!dunge.IsEmptyCell(place.x + n, place.y + room_height))
				{
					return false;
				}
			}
			break;
		}
		case Direction.Up:
		{
			for (int num5 = 0; num5 < room_width + 2; num5++)
			{
				if (!dunge.IsEmptyCell(place.x - 1 + num5, place.y + room_height))
				{
					return false;
				}
				if ((place.x - 1 + num5 > num3 || place.x - 1 + num5 < num4) && !dunge.IsEmptyCell(place.x - 1 + num5, place.y - 1))
				{
					return false;
				}
			}
			for (int num6 = -1; num6 < room_height; num6++)
			{
				if (!dunge.IsEmptyCell(place.x - 1, place.y + num6))
				{
					return false;
				}
				if (!dunge.IsEmptyCell(place.x + room_width, place.y + num6))
				{
					return false;
				}
			}
			break;
		}
		case Direction.Right:
		{
			for (int k = 0; k < room_height + 2; k++)
			{
				if (!dunge.IsEmptyCell(place.x + room_width, place.y - 1 + k))
				{
					return false;
				}
				if ((place.y - 1 + k > num || place.y - 1 + k < num2) && !dunge.IsEmptyCell(place.x - 1, place.y - 1 + k))
				{
					return false;
				}
			}
			for (int l = -1; l < room_width; l++)
			{
				if (!dunge.IsEmptyCell(place.x + l, place.y - 1))
				{
					return false;
				}
				if (!dunge.IsEmptyCell(place.x + l, place.y + room_height))
				{
					return false;
				}
			}
			break;
		}
		case Direction.Down:
		{
			for (int i = 0; i < room_width + 2; i++)
			{
				if (!dunge.IsEmptyCell(place.x - 1 + i, place.y - 1))
				{
					return false;
				}
				if ((place.x - 1 + i > num3 || place.x - 1 + i < num4) && !dunge.IsEmptyCell(place.x - 1 + i, place.y + room_height))
				{
					return false;
				}
			}
			for (int j = 0; j < room_height + 1; j++)
			{
				if (!dunge.IsEmptyCell(place.x - 1, place.y + j))
				{
					return false;
				}
				if (!dunge.IsEmptyCell(place.x + room_width, place.y + j))
				{
					return false;
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("direction", direction, null);
		}
		return true;
	}

	public bool IsDeadlock()
	{
		if (last_tick_action_type_list.Count >= 20)
		{
			int num = 0;
			foreach (ActionChances.ActionType item in last_tick_action_type_list)
			{
				if (item == ActionChances.ActionType.TurnLeft || item == ActionChances.ActionType.TurnRight)
				{
					num++;
				}
			}
			if (num > 10)
			{
				MainGame.me.dungeon_root.statistics.walker_is_cycled++;
				is_correct = false;
				is_finished = true;
				return true;
			}
		}
		bool flag = true;
		for (int i = 0; i < 4; i++)
		{
			TurnLeft();
			flag = flag && IsWrongDirection();
		}
		return flag;
	}

	public bool TryDoStep()
	{
		if (IsWrongPosition())
		{
			return false;
		}
		if (IsWrongDirection())
		{
			return false;
		}
		IntVector2 intVector = null;
		intVector = cur_direction switch
		{
			Direction.Left => new IntVector2(-1), 
			Direction.Up => new IntVector2(0, 1), 
			Direction.Right => new IntVector2(1), 
			Direction.Down => new IntVector2(0, -1), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		for (int i = 0; i < step_length; i++)
		{
			cur_position += intVector;
			if (!TryMarkStepPattern())
			{
				MainGame.me.dungeon_root.statistics.can_not_mark_step_pattern_after_step++;
				is_correct = false;
				is_finished = true;
				return false;
			}
		}
		return true;
	}

	public void TurnRight()
	{
		switch (cur_direction)
		{
		case Direction.Left:
		case Direction.Up:
		case Direction.Right:
			cur_direction++;
			break;
		case Direction.Down:
			cur_direction = Direction.Left;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void TurnLeft()
	{
		switch (cur_direction)
		{
		case Direction.Left:
			cur_direction = Direction.Down;
			break;
		case Direction.Up:
		case Direction.Right:
		case Direction.Down:
			cur_direction--;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool IsWrongPosition()
	{
		if (dunge == null)
		{
			return true;
		}
		if (thickness < 1)
		{
			return true;
		}
		if (cur_position.x < thickness - 1 || cur_position.x > dunge.dungeon_width - thickness || cur_position.y < thickness - 1 || cur_position.y > dunge.dungeon_height - thickness)
		{
			MainGame.me.dungeon_root.statistics.touched_borders_6++;
			return true;
		}
		return false;
	}

	public bool IsWrongDirection()
	{
		if (dunge == null)
		{
			return false;
		}
		if (cur_direction == Direction.Left)
		{
			if (cur_position.x - thickness - step_length < 0)
			{
				MainGame.me.dungeon_root.statistics.touched_borders_7++;
				return true;
			}
			if (!dunge.IsEmptyCell(cur_position.x - thickness - step_length + 1, cur_position.y))
			{
				return true;
			}
			for (int i = 1; i <= step_length; i++)
			{
				for (int j = 0; j < 2 * thickness + 1; j++)
				{
					for (int k = 0; k < 2 * thickness + 1; k++)
					{
						if (_pattern.hit_pattern[k + (j << 8)])
						{
							if (dunge.IsEmptyCell(cur_position.x - i - thickness + k, cur_position.y - thickness + j))
							{
								break;
							}
							return true;
						}
					}
				}
			}
		}
		if (cur_direction == Direction.Up)
		{
			if (cur_position.y + thickness + step_length >= dunge.dungeon_height)
			{
				MainGame.me.dungeon_root.statistics.touched_borders_7++;
				return true;
			}
			if (!dunge.IsEmptyCell(cur_position.x, cur_position.y + thickness + step_length - 1))
			{
				return true;
			}
			for (int l = 1; l <= step_length; l++)
			{
				for (int m = 0; m < 2 * thickness + 1; m++)
				{
					for (int num = 2 * thickness; num > 0; num--)
					{
						if (_pattern.hit_pattern[m + (num << 8)])
						{
							if (dunge.IsEmptyCell(cur_position.x - thickness + m, cur_position.y + l - thickness + num))
							{
								break;
							}
							return true;
						}
					}
				}
			}
		}
		if (cur_direction == Direction.Right)
		{
			if (cur_position.x + thickness + step_length >= dunge.dungeon_width)
			{
				MainGame.me.dungeon_root.statistics.touched_borders_7++;
				return true;
			}
			if (!dunge.IsEmptyCell(cur_position.x + thickness + step_length - 1, cur_position.y))
			{
				return true;
			}
			for (int n = 1; n <= step_length; n++)
			{
				for (int num2 = 0; num2 < 2 * thickness + 1; num2++)
				{
					for (int num3 = 2 * thickness; num3 > 0; num3--)
					{
						if (_pattern.hit_pattern[num3 + (num2 << 8)])
						{
							if (dunge.IsEmptyCell(cur_position.x + n - thickness + num3, cur_position.y - thickness + num2))
							{
								break;
							}
							return true;
						}
					}
				}
			}
		}
		if (cur_direction == Direction.Down)
		{
			if (cur_position.y - thickness - step_length + 1 < 1)
			{
				MainGame.me.dungeon_root.statistics.touched_borders_7++;
				return true;
			}
			if (!dunge.IsEmptyCell(cur_position.x, cur_position.y - thickness - step_length + 1))
			{
				return true;
			}
			for (int num4 = 1; num4 <= step_length; num4++)
			{
				for (int num5 = 0; num5 < 2 * thickness + 1; num5++)
				{
					for (int num6 = 0; num6 < 2 * thickness + 1; num6++)
					{
						if (_pattern.hit_pattern[num5 + (num6 << 8)])
						{
							if (dunge.IsEmptyCell(cur_position.x - thickness + num5, cur_position.y - num4 - thickness + num6))
							{
								break;
							}
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool IsWrongWalker()
	{
		foreach (DungeonRoom room in rooms)
		{
			if (room.room_width < 2 * thickness || room.room_height < 2 * thickness)
			{
				return true;
			}
		}
		return false;
	}
}
