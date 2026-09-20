using System.Collections.Generic;
using UnityEngine;

public class QuestListItemGUI : MonoBehaviour
{
	public UILabel txt;

	public UIWidget fx_point;

	private string _quest_id;

	private static Dictionary<string, UIWidget> _effects = new Dictionary<string, UIWidget>();

	public void Draw(QuestState qs, bool is_new = false)
	{
		_quest_id = qs.definition.id;
		txt.text = GJL.L("qt_" + _quest_id) + "(quest)";
		if (is_new)
		{
			ShowNewQuestEffect();
			return;
		}
		Debug.Log("re-linking effect for q = " + _quest_id, this);
		if (_effects.ContainsKey(_quest_id))
		{
			Debug.Log("re-link found", _effects[_quest_id]);
			_effects[_quest_id].SetAnchor(fx_point.gameObject);
		}
	}

	private void ShowNewQuestEffect()
	{
		if (GUIElements.me.quest_list.new_quest_fx_prefab == null)
		{
			return;
		}
		GameObject go = GUIElements.me.quest_list.new_quest_fx_prefab.Copy(fx_point.transform);
		go.transform.localPosition = Vector3.zero;
		go.SetActive(value: true);
		go.transform.SetParent(GUIElements.me.quest_list.transform, worldPositionStays: false);
		UIWidget component = go.GetComponent<UIWidget>();
		component.SetAnchor(fx_point.gameObject);
		_effects.Add(_quest_id, component);
		GJTimer.AddTimer(5f, delegate
		{
			if (go != null && go.gameObject != null && go.transform.parent != null)
			{
				_effects.Remove(_quest_id);
				Object.Destroy(go);
			}
		});
	}
}
