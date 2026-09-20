using System;
using System.Collections.Generic;
using UnityEngine;

public static class KeyBindings
{
	[Serializable]
	private class SerializableBindings
	{
		[Serializable]
		public class KeysList
		{
			public int[] keys;
		}

		public List<GameKey> keys = new List<GameKey>();

		public List<KeysList> values = new List<KeysList>();
	}

	public static Dictionary<GameKey, GamePadButton> gamepad_bindings;

	public static Dictionary<GameKey, KeyCode[]> default_bindings;

	public static Dictionary<GameKey, KeyCode[]> keyboard_bindings;

	public static Dictionary<GameKey, GameKey[]> mouse_bindings;

	static KeyBindings()
	{
		gamepad_bindings = new Dictionary<GameKey, GamePadButton>
		{
			{
				GameKey.Attack,
				GamePadButton.X
			},
			{
				GameKey.Dash,
				GamePadButton.LB
			},
			{
				GameKey.Dash2,
				GamePadButton.RB
			},
			{
				GameKey.Interaction,
				GamePadButton.A
			},
			{
				GameKey.Work,
				GamePadButton.Y
			},
			{
				GameKey.Action,
				GamePadButton.X
			},
			{
				GameKey.GameGUI,
				GamePadButton.Back
			},
			{
				GameKey.IngameMenu,
				GamePadButton.Start
			},
			{
				GameKey.Toolbar1,
				GamePadButton.DUp
			},
			{
				GameKey.Toolbar2,
				GamePadButton.DRight
			},
			{
				GameKey.Toolbar3,
				GamePadButton.DDown
			},
			{
				GameKey.Toolbar4,
				GamePadButton.DLeft
			},
			{
				GameKey.Left,
				GamePadButton.DLeft
			},
			{
				GameKey.Right,
				GamePadButton.DRight
			},
			{
				GameKey.Up,
				GamePadButton.DUp
			},
			{
				GameKey.Down,
				GamePadButton.DDown
			},
			{
				GameKey.Select,
				GamePadButton.A
			},
			{
				GameKey.Back,
				GamePadButton.B
			},
			{
				GameKey.Option1,
				GamePadButton.X
			},
			{
				GameKey.Option2,
				GamePadButton.Y
			},
			{
				GameKey.SliderDec,
				GamePadButton.DLeft
			},
			{
				GameKey.SliderInc,
				GamePadButton.DRight
			},
			{
				GameKey.PrevTab,
				GamePadButton.LB
			},
			{
				GameKey.NextTab,
				GamePadButton.RB
			},
			{
				GameKey.PrevSubTab,
				GamePadButton.LT
			},
			{
				GameKey.NextSubTub,
				GamePadButton.RT
			},
			{
				GameKey.RotateLeft,
				GamePadButton.LB
			},
			{
				GameKey.RotateRight,
				GamePadButton.RB
			},
			{
				GameKey.ShowWGOQualities,
				GamePadButton.LT
			},
			{
				GameKey.MiniGameAction,
				GamePadButton.A
			}
		};
		default_bindings = null;
		keyboard_bindings = new Dictionary<GameKey, KeyCode[]>
		{
			{
				GameKey.Attack,
				new KeyCode[1] { KeyCode.Space }
			},
			{
				GameKey.Dash,
				new KeyCode[1] { KeyCode.LeftShift }
			},
			{
				GameKey.Dash2,
				new KeyCode[1] { KeyCode.RightShift }
			},
			{
				GameKey.Interaction,
				new KeyCode[1] { KeyCode.E }
			},
			{
				GameKey.Work,
				new KeyCode[1] { KeyCode.F }
			},
			{
				GameKey.Action,
				new KeyCode[1] { KeyCode.X }
			},
			{
				GameKey.GameGUI,
				new KeyCode[1] { KeyCode.Tab }
			},
			{
				GameKey.IngameMenu,
				new KeyCode[1] { KeyCode.Escape }
			},
			{
				GameKey.Toolbar1,
				new KeyCode[1] { KeyCode.Alpha1 }
			},
			{
				GameKey.Toolbar2,
				new KeyCode[1] { KeyCode.Alpha2 }
			},
			{
				GameKey.Toolbar3,
				new KeyCode[1] { KeyCode.Alpha3 }
			},
			{
				GameKey.Toolbar4,
				new KeyCode[1] { KeyCode.Alpha4 }
			},
			{
				GameKey.Left,
				new KeyCode[2]
				{
					KeyCode.A,
					KeyCode.LeftArrow
				}
			},
			{
				GameKey.Right,
				new KeyCode[2]
				{
					KeyCode.D,
					KeyCode.RightArrow
				}
			},
			{
				GameKey.Up,
				new KeyCode[2]
				{
					KeyCode.W,
					KeyCode.UpArrow
				}
			},
			{
				GameKey.Down,
				new KeyCode[2]
				{
					KeyCode.S,
					KeyCode.DownArrow
				}
			},
			{
				GameKey.Back,
				new KeyCode[1] { KeyCode.Escape }
			},
			{
				GameKey.SliderDec,
				new KeyCode[2]
				{
					KeyCode.A,
					KeyCode.LeftArrow
				}
			},
			{
				GameKey.SliderInc,
				new KeyCode[2]
				{
					KeyCode.D,
					KeyCode.RightArrow
				}
			},
			{
				GameKey.RotateRight,
				new KeyCode[1] { KeyCode.R }
			},
			{
				GameKey.ShowWGOQualities,
				new KeyCode[1] { KeyCode.LeftControl }
			},
			{
				GameKey.MiniGameAction,
				new KeyCode[1] { KeyCode.E }
			},
			{
				GameKey.Inventory,
				new KeyCode[1] { KeyCode.I }
			},
			{
				GameKey.Techs,
				new KeyCode[1] { KeyCode.T }
			},
			{
				GameKey.Map,
				new KeyCode[1] { KeyCode.M }
			},
			{
				GameKey.KnownNPCs,
				new KeyCode[1] { KeyCode.N }
			}
		};
		mouse_bindings = new Dictionary<GameKey, GameKey[]>
		{
			{
				GameKey.LeftClick,
				new GameKey[1] { GameKey.MiniGameAction }
			},
			{
				GameKey.RightClick,
				new GameKey[1] { GameKey.Interaction }
			}
		};
		Debug.Log("Save default key bindings");
		default_bindings = new Dictionary<GameKey, KeyCode[]>();
		foreach (KeyValuePair<GameKey, KeyCode[]> keyboard_binding in keyboard_bindings)
		{
			default_bindings.Add(keyboard_binding.Key, keyboard_binding.Value);
		}
	}

	public static void Reset()
	{
		Debug.Log("Reset key bindings");
		foreach (KeyValuePair<GameKey, KeyCode[]> default_binding in default_bindings)
		{
			keyboard_bindings[default_binding.Key] = default_binding.Value;
		}
	}

	public static string ToJSON()
	{
		SerializableBindings serializableBindings = new SerializableBindings();
		foreach (KeyValuePair<GameKey, KeyCode[]> keyboard_binding in keyboard_bindings)
		{
			serializableBindings.keys.Add(keyboard_binding.Key);
			SerializableBindings.KeysList keysList = new SerializableBindings.KeysList
			{
				keys = new int[keyboard_binding.Value.Length]
			};
			for (int i = 0; i < keyboard_binding.Value.Length; i++)
			{
				keysList.keys[i] = (int)keyboard_binding.Value[i];
			}
			serializableBindings.values.Add(keysList);
		}
		return JsonUtility.ToJson(serializableBindings);
	}

	public static void FromJSON(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return;
		}
		SerializableBindings serializableBindings = null;
		try
		{
			serializableBindings = JsonUtility.FromJson<SerializableBindings>(json);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error reading key binding: " + ex);
			return;
		}
		if (serializableBindings == null)
		{
			return;
		}
		for (int i = 0; i < serializableBindings.keys.Count; i++)
		{
			GameKey key = serializableBindings.keys[i];
			SerializableBindings.KeysList keysList = serializableBindings.values[i];
			KeyCode[] array = new KeyCode[keysList.keys.Length];
			for (int j = 0; j < keysList.keys.Length; j++)
			{
				array[j] = (KeyCode)keysList.keys[j];
			}
			keyboard_bindings[key] = array;
		}
		Debug.Log($"Deserialize keyboard binding: {serializableBindings.keys.Count}");
	}

	public static void RedefineKey(GameKey gk, KeyCode key)
	{
		foreach (GameKey key2 in keyboard_bindings.Keys)
		{
			if (key2 == gk)
			{
				keyboard_bindings[gk] = new KeyCode[1] { key };
				return;
			}
		}
		keyboard_bindings[GameKey.MiniGameAction] = keyboard_bindings[GameKey.Interaction];
		Debug.LogError("Couldn't find a GameKey to redefine: " + gk);
	}
}
