using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : BaseInputController
{
	private const float DOUBLE_CLICK_DELAY = 0.5f;

	private float _last_click_time;

	public override void Update()
	{
		base.Update();
		foreach (KeyValuePair<GameKey, KeyCode[]> keyboard_binding in KeyBindings.keyboard_bindings)
		{
			GameKey key = keyboard_binding.Key;
			KeyCode[] value = keyboard_binding.Value;
			foreach (KeyCode key2 in value)
			{
				if (Input.GetKeyDown(key2) && !pressed_keys.Contains(key))
				{
					pressed_keys.Add(key);
				}
				if (Input.GetKey(key2) && !holded_keys.Contains(key))
				{
					holded_keys.Add(key);
				}
			}
		}
		if (Input.GetMouseButtonDown(0))
		{
			pressed_keys.Add(GameKey.LeftClick);
			if (Time.time - _last_click_time <= 0.5f)
			{
				pressed_keys.Add(GameKey.DoubleClick);
				_last_click_time = 0f;
			}
			else
			{
				_last_click_time = Time.time;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				pressed_keys.Add(GameKey.MoveAllStack);
			}
			GameKey[] array = KeyBindings.mouse_bindings[GameKey.LeftClick];
			foreach (GameKey item in array)
			{
				pressed_keys.Add(item);
			}
		}
		if (Input.GetMouseButton(0))
		{
			holded_keys.Add(GameKey.LeftClick);
			GameKey[] array = KeyBindings.mouse_bindings[GameKey.LeftClick];
			foreach (GameKey item2 in array)
			{
				holded_keys.Add(item2);
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			pressed_keys.Add(GameKey.RightClick);
			if (Input.GetKey(KeyCode.LeftShift))
			{
				pressed_keys.Add(GameKey.MoveAllStack);
			}
			GameKey[] array = KeyBindings.mouse_bindings[GameKey.RightClick];
			foreach (GameKey item3 in array)
			{
				pressed_keys.Add(item3);
			}
		}
		if (Input.GetMouseButton(1))
		{
			holded_keys.Add(GameKey.RightClick);
			GameKey[] array = KeyBindings.mouse_bindings[GameKey.RightClick];
			foreach (GameKey item4 in array)
			{
				holded_keys.Add(item4);
			}
		}
		dir = Vector2.zero;
		if (holded_keys.Contains(GameKey.Left))
		{
			dir.x -= 1f;
		}
		if (holded_keys.Contains(GameKey.Right))
		{
			dir.x += 1f;
		}
		if (holded_keys.Contains(GameKey.Up))
		{
			dir.y += 1f;
		}
		if (holded_keys.Contains(GameKey.Down))
		{
			dir.y -= 1f;
		}
	}
}
