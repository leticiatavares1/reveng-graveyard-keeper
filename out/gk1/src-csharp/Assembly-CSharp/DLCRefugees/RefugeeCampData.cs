using System;
using System.Collections.Generic;

namespace DLCRefugees;

[Serializable]
public class RefugeeCampData
{
	public bool is_camp_living;

	public bool camp_was_started_at_once;

	public List<Refugee> active_refugee_list = new List<Refugee>();

	public RefugeeCampMusic camp_music;

	public RefugeeCampMusicAlarich camp_music_alarich;

	public List<RefugeeInfo> refugee_already_spawned_list = new List<RefugeeInfo>();

	public List<RefugeeInfo> refugee_to_spawn_ordered_list = new List<RefugeeInfo>
	{
		new RefugeeInfo("npc_master_alarich", "npc_master_alarich"),
		new RefugeeInfo("npc_marquis_teodoro_jr", "npc_refugee_1"),
		new RefugeeInfo("npc_refugee_1", "npc_refugee_2"),
		new RefugeeInfo("npc_refugee_2", "npc_refugee_3"),
		new RefugeeInfo("npc_refugee_cook", "npc_refugee_8"),
		new RefugeeInfo("npc_refugee_3", "npc_refugee_4"),
		new RefugeeInfo("npc_refugee_moneylender", "npc_refugee_6"),
		new RefugeeInfo("npc_refugee_4", "npc_refugee_7"),
		new RefugeeInfo("npc_refugee_coffin_maker", "npc_refugee_5"),
		new RefugeeInfo("npc_refugee_5", "npc_refugee_9"),
		new RefugeeInfo("npc_refugee_tanner", "npc_refugee_10")
	};

	public void Init()
	{
		for (int i = 0; i < active_refugee_list.Count; i++)
		{
			active_refugee_list[i].Init();
		}
	}
}
