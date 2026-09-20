using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SmartDrawer : MonoBehaviour
{
	public List<SmartDrawerAtom> smart_drawer_atoms = new List<SmartDrawerAtom>();

	private WorldGameObject _wgo;

	public void Update()
	{
		Redraw();
	}

	public void Redraw(bool force = false)
	{
		if (_wgo == null)
		{
			_wgo = base.gameObject.transform.GetComponentInParent<WorldGameObject>();
			if (_wgo == null)
			{
				return;
			}
		}
		foreach (SmartDrawerAtom smart_drawer_atom in smart_drawer_atoms)
		{
			if (!force && !smart_drawer_atom.CheckNeedRecalc(Time.deltaTime))
			{
				continue;
			}
			bool active = false;
			switch (smart_drawer_atom.condition)
			{
			case SmartDrawerAtom.SmartDrawingCondition.OnCrafting:
				active = _wgo.components.craft.is_crafting;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.ItemsEqual:
				active = _wgo.data.inventory.Count == smart_drawer_atom.int_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.ItemsMoreEqual:
				active = _wgo.data.inventory.Count >= smart_drawer_atom.int_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.GameResEqual:
				active = Mathf.Abs(_wgo.data.GetParam(smart_drawer_atom.string_value) - smart_drawer_atom.float_value) < 0.001f;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.GameResMore:
				active = _wgo.data.GetParam(smart_drawer_atom.string_value) > smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.GameResLess:
				active = _wgo.data.GetParam(smart_drawer_atom.string_value) <= smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.GameResBetween:
			{
				float param = _wgo.data.GetParam(smart_drawer_atom.string_value);
				active = smart_drawer_atom.float_value < param && param <= smart_drawer_atom.float_value_2;
				break;
			}
			case SmartDrawerAtom.SmartDrawingCondition.ConcreteItemsBetween:
			{
				int itemsCount2 = _wgo.data.GetItemsCount(smart_drawer_atom.string_value);
				active = smart_drawer_atom.int_value < itemsCount2 && itemsCount2 <= smart_drawer_atom.int_value_2;
				break;
			}
			case SmartDrawerAtom.SmartDrawingCondition.ConcreteItemsMore:
			{
				int itemsCount = _wgo.data.GetItemsCount(smart_drawer_atom.string_value);
				active = smart_drawer_atom.int_value < itemsCount;
				break;
			}
			case SmartDrawerAtom.SmartDrawingCondition.DayTime_Day:
				active = !TimeOfDay.me.is_night;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.DayTime_Night:
				active = TimeOfDay.me.is_night;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.RainLess:
				active = EnvironmentEngine.me.FindStateByType(SmartWeatherState.WeatherType.Rain).value <= smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.RainMore:
				active = EnvironmentEngine.me.FindStateByType(SmartWeatherState.WeatherType.Rain).value > smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.WindLess:
				active = EnvironmentEngine.me.FindStateByType(SmartWeatherState.WeatherType.Wind).value <= smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.WindMore:
				active = EnvironmentEngine.me.FindStateByType(SmartWeatherState.WeatherType.Rain).value > smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.FogLess:
				active = EnvironmentEngine.me.FindStateByType(SmartWeatherState.WeatherType.Rain).value <= smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.FogMore:
				active = EnvironmentEngine.me.FindStateByType(SmartWeatherState.WeatherType.Rain).value > smart_drawer_atom.float_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.ContainItem:
				foreach (Item item in _wgo.data.inventory)
				{
					if (item.id == smart_drawer_atom.string_value)
					{
						active = true;
						break;
					}
				}
				break;
			case SmartDrawerAtom.SmartDrawingCondition.NoLinkedWorker:
				active = !_wgo.has_linked_worker;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.PorterStationStateIsNot:
				active = !(_wgo.porter_station == null) && _wgo.has_linked_worker && _wgo.porter_station.state != (PorterStation.PorterState)smart_drawer_atom.int_value;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.HasLinkedWorker:
				active = _wgo.has_linked_worker;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.TotalItemsCount:
			{
				int num = 0;
				foreach (Item item2 in _wgo.data.inventory)
				{
					num += item2.value;
				}
				active = num >= smart_drawer_atom.int_value && num < smart_drawer_atom.int_value_2;
				break;
			}
			case SmartDrawerAtom.SmartDrawingCondition.PlayerHasItem:
				active = MainGame.me.player.data.HasItemInInventory(smart_drawer_atom.string_value);
				break;
			case SmartDrawerAtom.SmartDrawingCondition.PlayerGameResEqual:
				active = Mathf.Abs(MainGame.me.player.data.GetParam(smart_drawer_atom.string_value) - smart_drawer_atom.float_value) < 0.001f;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.HasItemInInventoryByIndex:
			{
				Item itemByIndex = _wgo.data.GetItemByIndex(smart_drawer_atom.int_value);
				active = itemByIndex != null && !itemByIndex.IsEmpty();
				break;
			}
			case SmartDrawerAtom.SmartDrawingCondition.GDPointEnabled:
				active = WorldMap.GetGDPointByGDTag(smart_drawer_atom.string_value).gameObject.activeSelf;
				break;
			case SmartDrawerAtom.SmartDrawingCondition.GDPointDisabled:
				active = !WorldMap.GetGDPointByGDTag(smart_drawer_atom.string_value).gameObject.activeSelf;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SmartDrawerAtom.SmartDrawingCondition.None:
				break;
			}
			smart_drawer_atom.obj.gameObject.SetActive(active);
		}
	}
}
