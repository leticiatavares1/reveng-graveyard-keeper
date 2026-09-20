using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GlobalEventsSystem
{
	[Serializable]
	public class Event
	{
		public enum Type
		{
			None = 0,
			StartNewGame = 1,
			PlayerInsertOverheadToWgoAnItem = 2,
			PlayerTakeFromWgoTheItem = 3,
			PlayerUseItem = 4,
			PlayerTeleport = 5,
			PlayerTeleportAfterFadeOut = 6,
			PlayerEnterGDZone = 7,
			PlayerExitGDZone = 8,
			BuildBuilding = 9,
			CraftStart = 10,
			CraftFinish = 11,
			WgoDead = 12,
			WalkIntoWorldZone = 13,
			AddOverhead = 14,
			MultiAnswerSay = 15,
			SpeechSay = 16,
			SpeechSaid = 17,
			CloseUIWindow = 20,
			OpenUIWindow = 21,
			CustomInteraction = 22,
			WgoGoToFinished = 23,
			PlayerStartMoving = 24,
			PlayerFindWgoToWork = 25,
			Interaction = 26,
			AfterSleep = 27,
			PlayerInsertBodyToAutopsy = 28,
			FightSectorCaptured = 29,
			FightSectorLost = 30,
			FightLineCaptured = 31,
			FightLineLost = 32,
			PlayerDead = 33,
			PlayerHpValueReached = 34,
			WgoCustomTagDead = 35,
			WgoCustomTagHpValueReached = 36,
			PlayerChangeControlByFlow = 37,
			ConveyorChestItemAdded = 38,
			FightWon = 39,
			FightLost = 40,
			DonkeyStart = 41,
			DonkeyMorgue = 42,
			PlayerInsertBodyWithSkulls = 43,
			CustomFlowTrigger = 44,
			FirstCloseCraftWindow = 45,
			RepairTownCluster = 46,
			RemoveWgoDataFromScene = 47
		}

		public Type type;

		public string id;

		public bool hasId;

		public List<IEventTrigerrable> trigerrables = new List<IEventTrigerrable>();
	}

	[NonSerialized]
	public List<Event> checkingEvents = new List<Event>();

	[NonSerialized]
	private Dictionary<Event.Type, Dictionary<string, Event>> eventsCache = new Dictionary<Event.Type, Dictionary<string, Event>>();

	public static GlobalEventsSystem Me => MainGame.Instance.GameSave.globalEventsSystem;

	public void PrepareForGame()
	{
		checkingEvents = new List<Event>();
		eventsCache = new Dictionary<Event.Type, Dictionary<string, Event>>();
	}

	public void AddEvent(IEventTrigerrable trigerrable)
	{
		Event.Type type = trigerrable.Type;
		if (type == Event.Type.None)
		{
			return;
		}
		string triggerableId = trigerrable.TriggerableId;
		if (SGuid.IsNullOrEmpty(trigerrable.UniqueId))
		{
			trigerrable.UniqueId = new SGuid();
		}
		Event @event;
		if (!eventsCache.TryGetValue(type, out var value))
		{
			@event = new Event
			{
				type = type,
				id = triggerableId,
				hasId = !string.IsNullOrEmpty(triggerableId),
				trigerrables = new List<IEventTrigerrable> { trigerrable }
			};
			value = new Dictionary<string, Event> { { triggerableId, @event } };
			eventsCache.Add(@event.type, value);
		}
		else
		{
			if (value.TryGetValue(triggerableId, out @event))
			{
				@event.trigerrables.Add(trigerrable);
				return;
			}
			@event = new Event
			{
				type = type,
				id = triggerableId,
				hasId = !string.IsNullOrEmpty(triggerableId),
				trigerrables = new List<IEventTrigerrable> { trigerrable }
			};
			value.Add(triggerableId, @event);
		}
		checkingEvents.Add(@event);
	}

	public void RemoveEvent(IEventTrigerrable trigerrable)
	{
		Event.Type type = trigerrable.Type;
		string triggerableId = trigerrable.TriggerableId;
		if (!eventsCache.TryGetValue(type, out var value) || !value.TryGetValue(triggerableId, out var value2))
		{
			return;
		}
		Debug.Log($"Remove Event: {type} {triggerableId} {trigerrable.UniqueId}");
		Debug.Log($"Available events: {value2.trigerrables.Count}:");
		foreach (IEventTrigerrable trigerrable2 in value2.trigerrables)
		{
			Debug.Log($"  - {trigerrable2.TriggerableId} {trigerrable2.Type} {trigerrable2.UniqueId}");
		}
		int num = value2.trigerrables.FindIndex((IEventTrigerrable t) => t.TriggerableId == trigerrable.TriggerableId && t.Type == trigerrable.Type && t.UniqueId == trigerrable.UniqueId);
		if (num != -1)
		{
			value2.trigerrables.RemoveAt(num);
		}
		if (value2.trigerrables.Count == 0)
		{
			value.Remove(triggerableId);
			if (value.Count == 0)
			{
				eventsCache.Remove(type);
			}
			checkingEvents.Remove(value2);
		}
	}

	public static void FireTrigger(Event.Type type, string id = "")
	{
		GlobalEventsSystem globalEventsSystem = MainGame.Instance?.GameSave?.globalEventsSystem;
		if (globalEventsSystem?.eventsCache == null || !globalEventsSystem.eventsCache.TryGetValue(type, out var value) || value == null || !value.TryGetValue(id, out var value2))
		{
			return;
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		string value3 = string.Empty;
		string value4 = string.Empty;
		foreach (IEventTrigerrable trigerrable in value2.trigerrables)
		{
			if (trigerrable.OnTriggerPassed())
			{
				if (trigerrable is QuestFinishCheck questFinishCheck)
				{
					if (questFinishCheck.runModificator == QuestCheck.RunModificator.TriggerSolo)
					{
						if (!string.IsNullOrEmpty(value4))
						{
							continue;
						}
						value4 = questFinishCheck.questId;
					}
					list.Add(questFinishCheck.questId);
				}
				else
				{
					if (!(trigerrable is QuestCheck questCheck))
					{
						continue;
					}
					if (questCheck.runModificator == QuestCheck.RunModificator.TriggerSolo)
					{
						if (!string.IsNullOrEmpty(value3))
						{
							continue;
						}
						value3 = questCheck.questId;
					}
					list3.Add(questCheck.questId);
				}
			}
			else if (trigerrable is QuestCheck questCheck2)
			{
				list2.Add(questCheck2.questId);
			}
		}
		foreach (string item in list)
		{
			MainGame.Instance.GameSave.questSystemData.CompleteQuest(item);
		}
		foreach (string item2 in list2)
		{
			MainGame.Instance.GameSave.questSystemData.OnStartFailed(item2);
		}
		foreach (string item3 in list3)
		{
			MainGame.Instance.GameSave.questSystemData.StartQuest(item3);
		}
	}

	public bool GetEvents(Event.Type type, out Dictionary<string, Event> events)
	{
		if (eventsCache == null)
		{
			events = null;
			return false;
		}
		return eventsCache.TryGetValue(type, out events);
	}
}
