using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KnownNPCList
{
	public List<KnownNPC> npcs = new List<KnownNPC>();

	public KnownNPC GetOrCreateNPC(string npc_id)
	{
		ObjectDefinition dataOrNull = GameBalance.me.GetDataOrNull<ObjectDefinition>(npc_id);
		if (dataOrNull != null && !string.IsNullOrEmpty(dataOrNull.npc_alias))
		{
			return GetOrCreateNPC(dataOrNull.npc_alias);
		}
		if (dataOrNull == null && npc_id != "player")
		{
			return new KnownNPC();
		}
		foreach (KnownNPC npc in npcs)
		{
			if (npc.npc_id == npc_id)
			{
				return npc;
			}
		}
		Debug.Log("Adding new known npc: " + npc_id);
		KnownNPC knownNPC = new KnownNPC
		{
			npc_id = npc_id
		};
		npcs.Add(knownNPC);
		MainGame.me.save.achievements.CheckKeyQuests("meet_" + npc_id.Replace(" ", "_"));
		return knownNPC;
	}

	public void Sort()
	{
		npcs.Sort((KnownNPC a, KnownNPC b) => a.sort_order.CompareTo(b.sort_order));
	}

	public void RemoveNPC(string npc_id)
	{
		if (!string.IsNullOrEmpty(npc_id))
		{
			KnownNPC knownNPC = npcs.Find((KnownNPC p) => p.npc_id == npc_id);
			if (knownNPC != null)
			{
				npcs.Remove(knownNPC);
				Debug.Log("Removing NPC " + npc_id + " from Known NPC List");
			}
			else
			{
				Debug.Log("Can't find NPC " + npc_id + " in Known NPC List");
			}
		}
	}

	public KnownNPC GetNPC(string npc_id)
	{
		foreach (KnownNPC npc in npcs)
		{
			if (npc.npc_id == npc_id)
			{
				return npc;
			}
		}
		return null;
	}
}
