using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[Serializable]
public class EnvironmentEngine : MonoBehaviour
{
	[Serializable]
	public class EnvironmentEngineData
	{
		public State state;

		public State prev_state;

		public List<SwitchableWeatherState> nature_weather_line;

		public List<SwitchableWeatherState> local_weather_line;

		public List<ForcedWeatherState> forced_weather_line;

		public List<LUTAtom> lut_line;

		[SmartDontSerialize]
		public LUTController lut_controller;

		public List<SmartWeatherState.SerializedWeatherState> serialized_states = new List<SmartWeatherState.SerializedWeatherState>();

		public State? stored_state;
	}

	public enum State
	{
		RealTime,
		Inside
	}

	public static bool log_weather;

	private static EnvironmentEngine _me;

	private List<WeatherState> _states;

	public TimeOfDay time_of_day;

	private float _cur_time = 0.72f;

	public float day_time_period = 9f;

	private bool _auto_adjust_time = true;

	private bool _is_rainy;

	public SmartWeatherState[] states;

	public bool weather_is_forced;

	public EnvironmentEngineData data = new EnvironmentEngineData();

	public Texture2D lut_inside;

	public LUTMorpher lut;

	public AmplifyColorEffect lut_effect_timeofday;

	[NonSerialized]
	public static EnvironmentPreset cur_preset;

	public static EnvironmentEngine me => _me ?? (_me = UnityEngine.Object.FindObjectOfType<EnvironmentEngine>());

	public bool auto_adjust_time => _auto_adjust_time;

	public bool is_rainy => _is_rainy;

	public void Awake()
	{
		_me = this;
		_states = GetComponentsInChildren<WeatherState>(includeInactive: true).ToList();
	}

	public void Init()
	{
		Debug.Log("EnvironmentEngine.Init()");
		MainGame.me.grain_fx_component = MainGame.me.GetComponent<NoiseAndGrain>();
		data = new EnvironmentEngineData();
		data.lut_controller = MainGame.me.gameObject.AddComponent<LUTController>();
		data.lut_controller.InitLUTController();
		ResetStates();
		FindStateByType(SmartWeatherState.WeatherType.Fog).controller.FindParameterOfType(SmartControllerParameter.Action.AbstractControllerCmp).abs_cmp = Fog.me;
	}

	public void Update()
	{
		if (MainGame.game_starting || MainGame.paused || !MainGame.game_started)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (!IsTimeStopped())
		{
			_cur_time += deltaTime / 225f;
			if (time_of_day != null)
			{
				time_of_day.time_of_day = _cur_time;
			}
			if (_cur_time > 1f)
			{
				_cur_time -= 2f;
				if (time_of_day != null)
				{
					time_of_day.time_of_day = _cur_time;
				}
				OnEndOfDay();
			}
		}
		UpdateCurEnvironmentPreset();
		UpdateWeather();
		_is_rainy = false;
		foreach (WeatherState state in _states)
		{
			state.StateUpdate(deltaTime);
			if (state.type == WeatherState.WeatherType.Rain && (double)state.amount > 0.7)
			{
				_is_rainy = true;
			}
		}
	}

	private void OnEndOfDay()
	{
		MainGame.me.save.day++;
		if (MainGame.me.gui_elements.hud != null)
		{
			MainGame.me.gui_elements.hud.OnEndOfDay();
		}
		MainGame.me.save.quests.CheckKeyQuests("end_of_day");
		SmartWeatherEngine.me.UpdateWeather();
		WorldMap.VendorsTradeWithBank();
		string[] array = new string[2] { "graveyard", "church" };
		foreach (string text in array)
		{
			WorldZone zoneByID = WorldZone.GetZoneByID(text);
			if (zoneByID == null)
			{
				Debug.LogError("Not found zone by name " + text);
				continue;
			}
			float totalQuality = zoneByID.GetTotalQuality();
			float param = MainGame.me.player.GetParam(text + "_qual");
			float num = totalQuality - param;
			Stats.ResourceEvent(num > 0f, text, Mathf.Abs(num), "quality");
			MainGame.me.player.SetParam(text + "_qual", totalQuality);
		}
		Stats.DesignEvent("Day");
	}

	public void SetEngineGlobalState(State engine_state)
	{
		if (data == null)
		{
			Debug.LogError("Data of EnvironmentEngine is null", this);
			return;
		}
		data.state = engine_state;
		UpdateCurEnvironmentPreset();
	}

	private void UpdateCurEnvironmentPreset()
	{
		switch (data.state)
		{
		case State.Inside:
			if (cur_preset == null)
			{
				ApplyEnvironmentPreset(EnvironmentPreset.Load("inside"));
			}
			break;
		case State.RealTime:
			if (cur_preset != null && cur_preset.name == "inside")
			{
				ApplyEnvironmentPreset(null);
			}
			break;
		}
	}

	public void ChangeEnvironment(string teleport_tag)
	{
		Debug.Log("Change Environment, teleport_tag = " + teleport_tag);
		string[] array = teleport_tag.Split('_');
		if (array.Length >= 3 && array[0] == "tp")
		{
			MainGame.me.save.SetEnvironmentPreset(array);
		}
		else
		{
			Debug.LogError("Unknown teleport_tag format: " + teleport_tag);
		}
	}

	public void ApplyEnvironmentPreset(EnvironmentPreset preset)
	{
		float morph_speed = 4f;
		bool flag = true;
		if (preset != null)
		{
			morph_speed = preset.lut_morph_speed;
			flag = !preset.disable_timeofday_lut;
			if (preset.name.Contains("_none_"))
			{
				preset = null;
			}
		}
		cur_preset = preset;
		lut.SetMainLUT((preset == null) ? null : preset.lut, morph_speed);
		lut_effect_timeofday.enabled = flag;
		UpdateCurEnvironmentPreset();
		Update();
		time_of_day.Update();
	}

	public static void SetTime(float time)
	{
		if (Application.isPlaying && !(_me == null))
		{
			_me._cur_time = time;
		}
	}

	public void EnableTime(bool enable)
	{
		Debug.Log("EnableTime: " + enable);
		_auto_adjust_time = enable;
	}

	public bool IsTimeStopped()
	{
		if (_auto_adjust_time)
		{
			if (MainGame.me.player_char != null)
			{
				return !MainGame.me.player_char.control_enabled;
			}
			return false;
		}
		return true;
	}

	public void ResetStates()
	{
		SmartWeatherState[] array = states;
		foreach (SmartWeatherState smartWeatherState in array)
		{
			if (!(smartWeatherState == null))
			{
				smartWeatherState.value = 0f;
			}
		}
		data.nature_weather_line = new List<SwitchableWeatherState>();
		data.local_weather_line = new List<SwitchableWeatherState>();
		data.forced_weather_line = new List<ForcedWeatherState>();
		data.lut_line = new List<LUTAtom>();
		UpdateWeather();
	}

	public void DisableAutoTime()
	{
		_auto_adjust_time = false;
	}

	private void SetWeatherEnabled(bool enable)
	{
		SmartWeatherState[] array = states;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEnabled(enable);
		}
	}

	public SmartWeatherState FindStateByType(SmartWeatherState.WeatherType type)
	{
		if (type == SmartWeatherState.WeatherType.LUT)
		{
			return null;
		}
		SmartWeatherState[] array = states;
		foreach (SmartWeatherState smartWeatherState in array)
		{
			if (smartWeatherState.type == type)
			{
				return smartWeatherState;
			}
		}
		Debug.LogError("Couldn't find a weather state: " + type);
		return null;
	}

	public void UpdateWeather()
	{
		if (!MainGame.game_started || data == null)
		{
			if (data == null)
			{
				Debug.LogError("EnvEngine: Data is null", this);
			}
			return;
		}
		if (data.forced_weather_line == null)
		{
			data.forced_weather_line = new List<ForcedWeatherState>();
		}
		if (data.local_weather_line == null)
		{
			data.local_weather_line = new List<SwitchableWeatherState>();
		}
		if (data.nature_weather_line == null)
		{
			data.nature_weather_line = new List<SwitchableWeatherState>();
		}
		data.lut_line = new List<LUTAtom>();
		if (data.lut_controller == null)
		{
			data.lut_controller = MainGame.me.GetComponent<LUTController>();
		}
		if (data.state == State.Inside && data.prev_state == State.RealTime)
		{
			SetWeatherEnabled(enable: false);
		}
		else if (data.state == State.RealTime && data.prev_state == State.Inside)
		{
			SetWeatherEnabled(enable: true);
		}
		UpdateNatureWeatherLine();
		ApplyNatureWeatherState();
		UpdateLocalWeatherLine();
		if (data.state == State.Inside)
		{
			data.lut_line = new List<LUTAtom>();
		}
		UpdateForcedWeatherLine();
		if (data.state == State.Inside)
		{
			SmartWeatherState[] array = states;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetValueImmediate(0f);
			}
		}
		if (data.prev_state != data.state)
		{
			SmartWeatherState[] array = states;
			foreach (SmartWeatherState obj in array)
			{
				obj.SetValueImmediate(obj.value);
			}
		}
		if (weather_is_forced)
		{
			data.lut_line = new List<LUTAtom>();
			SmartWeatherState[] array = states;
			foreach (SmartWeatherState obj2 in array)
			{
				obj2.SetValueImmediate(obj2.forced_value);
			}
		}
		else
		{
			SmartWeatherState[] array = states;
			foreach (SmartWeatherState obj3 in array)
			{
				obj3.forced_value = obj3.value;
			}
		}
		data.lut_controller.UpdateColorEffects(data.lut_line);
		data.prev_state = data.state;
	}

	public bool UpdateForcedWeatherLine()
	{
		if (data.forced_weather_line.Count == 0)
		{
			return false;
		}
		bool result = false;
		float game_time = MainGame.game_time;
		List<ForcedWeatherState> list = new List<ForcedWeatherState>();
		foreach (ForcedWeatherState item in data.forced_weather_line)
		{
			if (!(item.t_start < game_time))
			{
				continue;
			}
			SmartWeatherState smartWeatherState = FindStateByType(item.type);
			if (item.t_start + item.t_atk > game_time)
			{
				float num = (game_time - item.t_start) / item.t_atk;
				if (item.type == SmartWeatherState.WeatherType.LUT)
				{
					InsertLUTAtomInLUTLine(num, item);
					continue;
				}
				smartWeatherState.value = Mathf.Lerp(smartWeatherState.value, item.value, num);
				result = true;
			}
			else if (item.t_start + item.t_atk + item.t_flat > game_time)
			{
				if (item.type == SmartWeatherState.WeatherType.LUT)
				{
					InsertLUTAtomInLUTLine(1f, item);
					continue;
				}
				smartWeatherState.value = item.value;
				result = true;
			}
			else if (item.t_start + item.t_atk + item.t_flat + item.t_dec > game_time)
			{
				float num2 = (game_time - item.t_start - item.t_atk - item.t_flat) / item.t_dec;
				if (item.type == SmartWeatherState.WeatherType.LUT)
				{
					InsertLUTAtomInLUTLine(1f - num2, item);
				}
				else
				{
					smartWeatherState.value = Mathf.Lerp(item.value, smartWeatherState.value, num2);
				}
				result = true;
			}
			else
			{
				list.Add(item);
				if (log_weather)
				{
					Debug.Log("#weather#Remove forced state [" + item.type.ToString() + "]");
				}
			}
		}
		if (list.Count > 0)
		{
			foreach (ForcedWeatherState item2 in list)
			{
				data.forced_weather_line.Remove(item2);
			}
		}
		return result;
	}

	public bool UpdateLocalWeatherLine()
	{
		return UpdateSwitchableWeatherLine(data.local_weather_line);
	}

	private bool UpdateSwitchableWeatherLine(List<SwitchableWeatherState> switchable_weather_line, bool is_nature = false)
	{
		if (switchable_weather_line.Count == 0)
		{
			return false;
		}
		bool result = false;
		float game_time = MainGame.game_time;
		for (int i = 0; i < switchable_weather_line.Count && !(switchable_weather_line[i].t_start > game_time); i++)
		{
			SwitchableWeatherState switchableWeatherState = switchable_weather_line[i];
			SmartWeatherState smartWeatherState = FindStateByType(switchableWeatherState.type);
			if (switchableWeatherState.start_removing_time > 1f)
			{
				if (switchableWeatherState.start_removing_time + switchableWeatherState.t_dec < game_time)
				{
					switchable_weather_line.RemoveAt(i);
					i--;
					if (log_weather)
					{
						Debug.Log("#weather#Removed local weather state from line [" + switchableWeatherState.preset_name + "]");
					}
					continue;
				}
				if (!switchableWeatherState.do_dec_now && switchableWeatherState.start_removing_time < game_time && switchableWeatherState.t_start + switchableWeatherState.t_atk > game_time)
				{
					float num = (game_time - switchableWeatherState.t_start) / switchableWeatherState.t_atk;
					switchableWeatherState.start_removing_time = game_time - (1f - num) * switchableWeatherState.t_dec;
					switchableWeatherState.do_dec_now = true;
				}
				if (switchableWeatherState.do_dec_now || switchableWeatherState.start_removing_time < game_time)
				{
					float num2 = (game_time - switchableWeatherState.start_removing_time) / switchableWeatherState.t_dec;
					if (switchableWeatherState.type == SmartWeatherState.WeatherType.LUT)
					{
						InsertLUTAtomInLUTLine(1f - num2, switchableWeatherState);
						continue;
					}
					if (is_nature)
					{
						smartWeatherState.nature_value = Mathf.Lerp(switchableWeatherState.value, smartWeatherState.nature_value, num2);
					}
					else
					{
						smartWeatherState.value = Mathf.Lerp(switchableWeatherState.value, smartWeatherState.value, num2);
					}
					result = true;
					continue;
				}
			}
			if (switchableWeatherState.t_start + switchableWeatherState.t_atk > game_time)
			{
				float num3 = (game_time - switchableWeatherState.t_start) / switchableWeatherState.t_atk;
				if (switchableWeatherState.type == SmartWeatherState.WeatherType.LUT)
				{
					InsertLUTAtomInLUTLine(num3, switchableWeatherState);
					continue;
				}
				if (is_nature)
				{
					smartWeatherState.nature_value = Mathf.Lerp(smartWeatherState.nature_value, switchableWeatherState.value, num3);
				}
				else
				{
					smartWeatherState.value = Mathf.Lerp(smartWeatherState.value, switchableWeatherState.value, num3);
				}
				result = true;
			}
			else if (switchableWeatherState.type == SmartWeatherState.WeatherType.LUT)
			{
				InsertLUTAtomInLUTLine(1f, switchableWeatherState);
			}
			else
			{
				if (is_nature)
				{
					smartWeatherState.nature_value = switchableWeatherState.value;
				}
				else
				{
					smartWeatherState.value = switchableWeatherState.value;
				}
				result = true;
			}
		}
		return result;
	}

	public bool UpdateNatureWeatherLine()
	{
		return UpdateSwitchableWeatherLine(data.nature_weather_line, is_nature: true);
	}

	public void ApplyNatureWeatherState()
	{
		SmartWeatherState[] array = states;
		foreach (SmartWeatherState obj in array)
		{
			obj.value = obj.nature_value;
		}
	}

	public void AddNatureWeatherState(SwitchableWeatherState n_state)
	{
		AddSwitchableWeatherState(n_state, data.nature_weather_line);
	}

	public void AddLocalWeatherState(SwitchableWeatherState l_state)
	{
		AddSwitchableWeatherState(l_state, data.local_weather_line);
	}

	private void AddSwitchableWeatherState(SwitchableWeatherState sw_state, List<SwitchableWeatherState> switchable_weather_line)
	{
		if (switchable_weather_line == null)
		{
			switchable_weather_line = new List<SwitchableWeatherState>();
		}
		if (sw_state == null)
		{
			Debug.LogError("Switchable state is null!");
			return;
		}
		if (switchable_weather_line.Count == 0)
		{
			switchable_weather_line.Add(sw_state);
		}
		else
		{
			float game_time = MainGame.game_time;
			if (Mathf.Abs(sw_state.t_start - game_time) < 0.001f)
			{
				foreach (SwitchableWeatherState item in switchable_weather_line)
				{
					if (!(item.preset_name != sw_state.preset_name) && item.do_dec_now)
					{
						float num = (game_time - item.start_removing_time) / item.t_dec;
						item.t_start = game_time - (1f - num) * item.t_atk;
						item.do_dec_now = false;
						item.start_removing_time = -1f;
						item.t_dec = 0f;
						if (log_weather)
						{
							Debug.Log("#weather#Changed [" + item.type.ToString() + "] switchable state to ATK");
						}
						return;
					}
				}
			}
			bool flag = false;
			for (int i = 0; i < switchable_weather_line.Count; i++)
			{
				if (sw_state.t_start < switchable_weather_line[i].t_start)
				{
					switchable_weather_line.Insert(i, sw_state);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				switchable_weather_line.Add(sw_state);
			}
		}
		if (log_weather)
		{
			Debug.Log("#weather#Inserted switchable weather state to line " + sw_state?.ToString() + " [" + (switchable_weather_line.IndexOf(sw_state) + 1) + " of " + switchable_weather_line.Count + "]");
		}
	}

	public bool TryRemoveLocalWeatherState(string preset_name, float start_removing_time, float dec_time)
	{
		return TryRemoveSwitchableWeatherState(data.local_weather_line, preset_name, start_removing_time, dec_time);
	}

	public bool TryRemoveNatureWeatherState(string preset_name, float start_removing_time, float dec_time)
	{
		return TryRemoveSwitchableWeatherState(data.nature_weather_line, preset_name, start_removing_time, dec_time);
	}

	private bool TryRemoveSwitchableWeatherState(List<SwitchableWeatherState> switchable_weather_line, string preset_name, float start_removing_time, float dec_time)
	{
		if (switchable_weather_line == null)
		{
			switchable_weather_line = new List<SwitchableWeatherState>();
			return false;
		}
		if (string.IsNullOrEmpty(preset_name))
		{
			Debug.LogError("Preset name is null!");
			return false;
		}
		if (switchable_weather_line.Count == 0)
		{
			return false;
		}
		int num = 0;
		foreach (SwitchableWeatherState item in switchable_weather_line)
		{
			if (item.preset_name == preset_name && !item.HasRemoveCommand())
			{
				item.t_dec = dec_time;
				item.start_removing_time = start_removing_time;
				num++;
				if (log_weather)
				{
					Debug.Log("#weather#Added comand: removing [" + num + "] switchable weather state from line [" + preset_name + "], start_removing_time = " + start_removing_time + ", dec_time = " + dec_time);
				}
			}
		}
		if (num == 0)
		{
			if (log_weather)
			{
				Debug.Log("#weather#Error removing switchable weather state from line [" + preset_name + "]");
			}
			return false;
		}
		return true;
	}

	public void AddForcedWeatherState(ForcedWeatherState f_state)
	{
		if (data.forced_weather_line == null)
		{
			data.forced_weather_line = new List<ForcedWeatherState>();
		}
		if (f_state == null)
		{
			Debug.LogError("Forced state is null!");
			return;
		}
		if (data.forced_weather_line.Count == 0)
		{
			data.forced_weather_line.Add(f_state);
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < data.forced_weather_line.Count; i++)
			{
				if (f_state.t_start < data.forced_weather_line[i].t_start)
				{
					data.forced_weather_line.Insert(i, f_state);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				data.forced_weather_line.Add(f_state);
			}
		}
		if (log_weather)
		{
			Debug.Log("#weather#Inserted forced weather state to line [" + (data.forced_weather_line.IndexOf(f_state) + 1) + " of " + data.forced_weather_line.Count + "]");
		}
	}

	public bool TryGetLUTAtomFromLUTLine(string texture_name, out LUTAtom found_lut_atom)
	{
		found_lut_atom = null;
		if (data.lut_line == null)
		{
			Debug.LogError("LUT Line is null!");
			data.lut_line = new List<LUTAtom>();
			return false;
		}
		if (data.lut_line.Count == 0)
		{
			return false;
		}
		foreach (LUTAtom item in data.lut_line)
		{
			if (item.lut_name == texture_name)
			{
				found_lut_atom = item;
				return true;
			}
		}
		return false;
	}

	private void InsertLUTAtomInLUTLine(float value_k, WeatherStateBase w_state)
	{
		if (TryGetLUTAtomFromLUTLine(w_state.lut_texture.name, out var found_lut_atom))
		{
			found_lut_atom.value = w_state.value * value_k;
			return;
		}
		data.lut_line.Add(new LUTAtom
		{
			lut_texture = w_state.lut_texture,
			lut_name = w_state.lut_texture.name,
			value = w_state.value * value_k
		});
	}

	private List<string> FindPresetsWithoutRemoves(List<SwitchableWeatherState> switchable_weather_line)
	{
		List<string> list = new List<string>();
		foreach (SwitchableWeatherState item in switchable_weather_line)
		{
			if (!item.HasRemoveCommand() && !list.Contains(item.preset_name))
			{
				list.Add(item.preset_name);
			}
		}
		return list;
	}

	public List<string> FindNatureWithoutRemoves()
	{
		return FindPresetsWithoutRemoves(data.nature_weather_line);
	}

	public void PrepareForSave()
	{
		if (states == null || states.Length == 0)
		{
			Awake();
		}
		data.serialized_states = new List<SmartWeatherState.SerializedWeatherState>();
		SmartWeatherState[] array = states;
		foreach (SmartWeatherState smartWeatherState in array)
		{
			data.serialized_states.Add(smartWeatherState.Serialize());
		}
	}

	public void DeserializeData(EnvironmentEngineData loaded_data)
	{
		if (loaded_data == null)
		{
			return;
		}
		data = loaded_data;
		data.lut_controller = MainGame.me.GetComponent<LUTController>();
		_auto_adjust_time = true;
		if (states == null || states.Length == 0)
		{
			Awake();
		}
		foreach (SmartWeatherState.SerializedWeatherState serialized_state in loaded_data.serialized_states)
		{
			bool flag = false;
			SmartWeatherState[] array = states;
			foreach (SmartWeatherState smartWeatherState in array)
			{
				if (smartWeatherState.name == serialized_state.name)
				{
					flag = true;
					smartWeatherState.Deserialize(serialized_state);
					break;
				}
			}
			if (!flag)
			{
				Debug.LogError("Couldn't find a weather state with name = " + serialized_state.name);
			}
		}
	}

	public void StoreEnvironmentEngineState()
	{
		data.stored_state = data.state;
	}

	public void RestoreEnvironmentEngineState()
	{
		if (data.stored_state.HasValue)
		{
			data.state = data.stored_state.Value;
			data.stored_state = null;
		}
	}
}
