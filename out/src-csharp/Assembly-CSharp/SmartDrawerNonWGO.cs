using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SmartDrawerNonWGO : MonoBehaviour
{
	public List<SmartDrawerAtom> smart_drawer_atoms = new List<SmartDrawerAtom>();

	public void Update()
	{
		Redraw();
	}

	public void Redraw(bool force = false)
	{
		foreach (SmartDrawerAtom smart_drawer_atom in smart_drawer_atoms)
		{
			if (force || smart_drawer_atom.CheckNeedRecalc(Time.deltaTime))
			{
				bool active = false;
				switch (smart_drawer_atom.condition)
				{
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
				case SmartDrawerAtom.SmartDrawingCondition.PlayerHasItem:
					active = MainGame.me.player.data.HasItemInInventory(smart_drawer_atom.string_value);
					break;
				case SmartDrawerAtom.SmartDrawingCondition.PlayerGameResEqual:
					active = Mathf.Abs(MainGame.me.player.data.GetParam(smart_drawer_atom.string_value) - smart_drawer_atom.float_value) < 0.001f;
					break;
				default:
					Debug.Log("ERROR TRYING TO USE WGO CONDITION IN NON WGO SMART DRAWER");
					break;
				case SmartDrawerAtom.SmartDrawingCondition.None:
					break;
				}
				smart_drawer_atom.obj.gameObject.SetActive(active);
			}
		}
	}
}
