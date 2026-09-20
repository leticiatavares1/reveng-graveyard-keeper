using System;

namespace DungeonGenerator;

[Serializable]
public class DungeonStatistics
{
	public int finish_main_walker_by_iterator;

	public int main_walker_is_not_correct;

	public int finish_sub_walker_by_iterator;

	public int sub_walker_is_not_correct;

	public int is_wrong_direction;

	public int wrong_position_while_plasing_room;

	public int pre_placed_all_rooms;

	public int pre_placing_not_last_room;

	public int pre_no_possible_enters;

	public int pre_not_found_proper_enter;

	public int finish_walker_but_not_placed_all_rooms;

	public int walker_can_not_place_last_room;

	public int not_correctly_placed_sub_walker_after_room_placing;

	public int not_enough_exits_from_room;

	public int can_not_mark_step_pattern_after_room_placing;

	public int walker_is_cycled;

	public int can_not_mark_step_pattern_after_step;

	public int touched_borders;

	public int touched_borders_1;

	public int touched_borders_2;

	public int touched_borders_3;

	public int touched_borders_4;

	public int touched_borders_5;

	public int touched_borders_6;

	public int touched_borders_7;

	public int touched_borders_8;

	public int touched_borders_9;

	public void SetDefault()
	{
		finish_main_walker_by_iterator = 0;
		main_walker_is_not_correct = 0;
		finish_sub_walker_by_iterator = 0;
		sub_walker_is_not_correct = 0;
		is_wrong_direction = 0;
		wrong_position_while_plasing_room = 0;
		pre_placed_all_rooms = 0;
		pre_placing_not_last_room = 0;
		pre_no_possible_enters = 0;
		pre_not_found_proper_enter = 0;
		finish_walker_but_not_placed_all_rooms = 0;
		walker_can_not_place_last_room = 0;
		not_correctly_placed_sub_walker_after_room_placing = 0;
		not_enough_exits_from_room = 0;
		can_not_mark_step_pattern_after_room_placing = 0;
		walker_is_cycled = 0;
		can_not_mark_step_pattern_after_step = 0;
		touched_borders = 0;
		touched_borders_1 = 0;
		touched_borders_2 = 0;
		touched_borders_3 = 0;
		touched_borders_4 = 0;
		touched_borders_5 = 0;
		touched_borders_6 = 0;
		touched_borders_7 = 0;
		touched_borders_8 = 0;
		touched_borders_9 = 0;
	}

	public override string ToString()
	{
		return "#dgen# Dungeon statistics: \n-Global: \n finish_main_walker_by_iterator=" + finish_main_walker_by_iterator + "; \n main_walker_is_not_correct=" + main_walker_is_not_correct + "; \n finish_sub_walker_by_iterator=" + finish_sub_walker_by_iterator + "; \n sub_walker_is_not_correct=" + sub_walker_is_not_correct + "; \n-Local: \n is_wrong_direction=" + is_wrong_direction + "; \n wrong_position_while_plasing_room=" + wrong_position_while_plasing_room + "; \n - pre_placed_all_rooms=" + pre_placed_all_rooms + "; \n - pre_placing_not_last_room=" + pre_placing_not_last_room + "; \n - pre_no_possible_enters=" + pre_no_possible_enters + "; \n - pre_not_found_proper_enter=" + pre_not_found_proper_enter + "; \n finish_walker_but_not_placed_all_rooms=" + finish_walker_but_not_placed_all_rooms + ";\n walker_can_not_place_last_room=" + walker_can_not_place_last_room + ";\n not_correctly_placed_sub_walker_after_room_placing=" + not_correctly_placed_sub_walker_after_room_placing + "; \n not_enough_exits_from_room=" + not_enough_exits_from_room + ";\n can_not_mark_step_pattern_after_room_placing=" + can_not_mark_step_pattern_after_room_placing + ";\n walker_is_cycled=" + walker_is_cycled + ";\n can_not_mark_step_pattern_after_step=" + can_not_mark_step_pattern_after_step + ";\n";
	}
}
