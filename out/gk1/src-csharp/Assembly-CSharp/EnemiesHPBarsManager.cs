using System.Collections.Generic;
using UnityEngine;

public class EnemiesHPBarsManager : MonoBehaviour
{
	public static EnemiesHPBarsManager me;

	public OverheadGUI enemy_bar_prefab;

	private List<OverheadGUI> _bars = new List<OverheadGUI>();

	private bool _initialized;

	private void Awake()
	{
		if (!_initialized)
		{
			me = this;
			enemy_bar_prefab.Deactivate();
			_initialized = true;
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _bars.Count; i++)
		{
			if (_bars[i].IsNotNeededAnymore())
			{
				_bars[i].DestroyGO();
				_bars.RemoveAt(i);
				i--;
				break;
			}
			_bars[i].UpdateForLinkedObj();
		}
	}

	public void AddIfNeeded(WorldGameObject obj)
	{
		if (obj == null || obj.hp < 0f)
		{
			return;
		}
		long unique_id = obj.unique_id;
		foreach (OverheadGUI bar in _bars)
		{
			if (bar.linked_obj_id == unique_id)
			{
				return;
			}
		}
		OverheadGUI overheadGUI = enemy_bar_prefab.Copy(null, activate: true, "hp bar " + obj.obj_id);
		overheadGUI.LinkToObj(obj);
		_bars.Add(overheadGUI);
	}
}
