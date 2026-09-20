using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class QuestListGUI : MonoBehaviour
{
	public UITable table;

	private readonly List<QuestListItemGUI> _quests = new List<QuestListItemGUI>();

	private QuestListItemGUI _item_prefab;

	private const float REDRAW_RATE = 1f;

	private float _refresh_time;

	private readonly List<string> _shown_quests = new List<string>();

	public GameObject new_quest_fx_prefab;

	private StringBuilder _cur_quests_ids = new StringBuilder();

	public void Init()
	{
		table = GetComponentInChildren<UITable>(includeInactive: true);
		_item_prefab = GetComponentInChildren<QuestListItemGUI>(includeInactive: true);
		_item_prefab.gameObject.SetActive(value: false);
		if (new_quest_fx_prefab != null)
		{
			new_quest_fx_prefab.SetActive(value: false);
		}
	}

	public void ResetAtGameStart()
	{
		_shown_quests.Clear();
	}

	public void Redraw()
	{
		foreach (QuestListItemGUI quest in _quests)
		{
			quest.transform.SetParent(null, worldPositionStays: false);
			Object.Destroy(quest.gameObject);
		}
		_quests.Clear();
		WorldGameObject worldGameObject = null;
		_cur_quests_ids.Length = 0;
		foreach (QuestState currentQuest in MainGame.me.save.quests.GetCurrentQuests())
		{
			if (!currentQuest.definition.quest_visible)
			{
				continue;
			}
			_cur_quests_ids.Append(currentQuest.definition.id);
			_cur_quests_ids.Append(';');
			bool is_new = false;
			if (base.gameObject.activeInHierarchy && !_shown_quests.Contains(currentQuest.definition.id))
			{
				_shown_quests.Add(currentQuest.definition.id);
				is_new = true;
			}
			QuestListItemGUI questListItemGUI = _item_prefab.Copy();
			questListItemGUI.gameObject.SetActive(value: true);
			_quests.Add(questListItemGUI);
			questListItemGUI.Draw(currentQuest, is_new);
			if (!string.IsNullOrEmpty(currentQuest.definition.arrow_wgo_custom_tag) && worldGameObject == null)
			{
				worldGameObject = WorldMap.GetWorldGameObjectByCustomTag(currentQuest.definition.arrow_wgo_custom_tag);
			}
			if (string.IsNullOrEmpty(currentQuest.definition.arrow_wgo_obj_id) || !(worldGameObject == null))
			{
				continue;
			}
			List<WorldGameObject> worldGameObjectsByObjId = WorldMap.GetWorldGameObjectsByObjId(currentQuest.definition.arrow_wgo_obj_id);
			float num = float.MaxValue;
			foreach (WorldGameObject item in worldGameObjectsByObjId)
			{
				float sqrMagnitude = (MainGame.me.player.pos - item.pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					worldGameObject = item;
				}
			}
		}
		table.Reposition();
		table.repositionNow = true;
		UIRect[] componentsInChildren = GetComponentsInChildren<UIRect>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateAnchors();
		}
		if (MainGame.me.gui_elements.tutorial_arrow != null)
		{
			MainGame.me.gui_elements.tutorial_arrow.AttachToWGO(worldGameObject);
		}
	}

	public void Update()
	{
		_refresh_time -= Time.deltaTime;
		if (_refresh_time < 0f)
		{
			_refresh_time = 60f;
			Redraw();
		}
	}
}
