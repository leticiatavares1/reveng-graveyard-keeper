using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace LazyBearTechnology;

public class KeyboardController : BaseInputController
{
	public enum MovementType
	{
		Default,
		MouseOnly,
		MouseAndKeyboard
	}

	public const float DEAD_ZONE_RADIUS = 50f;

	private const float DOUBLE_CLICK_DELAY = 0.5f;

	private float lastClickTime;

	private List<KeyBinding> bindings;

	private MovementType currentMovementType;

	private float mouseScrollDelta;

	private Player rewiredPlayer;

	public MovementType CurrentMovementType
	{
		get
		{
			return currentMovementType;
		}
		set
		{
			currentMovementType = value;
		}
	}

	public float MouseScrollDelta => mouseScrollDelta;

	public KeyboardController(GameBindings gameBindings)
	{
		bindings = gameBindings.keyBindings;
		rewiredPlayer = ReInput.players.GetPlayer(0);
	}

	public override void Update()
	{
		base.Update();
		if (!ReInput.isReady)
		{
			return;
		}
		rewiredPlayer = ReInput.players.GetPlayer(0);
		for (int i = 0; i < bindings.Count; i++)
		{
			GameKey gameKey = bindings[i].gameKey;
			KeyCode keyCode = bindings[i].keyCode;
			KeyCode[] additionalKeyCodes = bindings[i].additionalKeyCodes;
			if (Input.GetKey(keyCode) && !holdedKeys.Contains(gameKey))
			{
				holdedKeys.Add(gameKey);
			}
			if (!Input.GetKeyDown(keyCode) || pressedKeys.Contains(gameKey))
			{
				continue;
			}
			if (additionalKeyCodes.Length != 0)
			{
				bool flag = true;
				for (int j = 0; j < additionalKeyCodes.Length; j++)
				{
					if (!Input.GetKey(additionalKeyCodes[j]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					pressedKeys.Add(gameKey);
				}
			}
			else
			{
				pressedKeys.Add(gameKey);
			}
		}
		HandleMouseClicks();
		HandleMouseScroll();
		HandleDirection();
	}

	public void EraseMouseControl()
	{
		mouseScrollDelta = 0f;
	}

	private void HandleMouseClicks()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (Time.time - lastClickTime <= 0.5f)
			{
				pressedKeys.Add(GameKey.DoubleClick);
				lastClickTime = 0f;
			}
			else
			{
				lastClickTime = Time.time;
			}
		}
	}

	private void HandleMouseScroll()
	{
		mouseScrollDelta = 0f - rewiredPlayer.GetAxis(18);
	}

	private void HandleDirection()
	{
		direction = Vector2.zero;
		switch (currentMovementType)
		{
		case MovementType.MouseOnly:
			if (holdedKeys.Contains(GameKey.LeftClick))
			{
				CalcDirectionFromMousePosition();
			}
			return;
		case MovementType.MouseAndKeyboard:
			if (holdedKeys.Contains(GameKey.Up) || holdedKeys.Contains(GameKey.Down) || holdedKeys.Contains(GameKey.Left) || holdedKeys.Contains(GameKey.Right))
			{
				CalcDirectionFromMousePosition();
			}
			return;
		}
		if (holdedKeys.Contains(GameKey.Left))
		{
			direction.x = -1f;
		}
		if (holdedKeys.Contains(GameKey.Right))
		{
			direction.x = 1f;
		}
		if (holdedKeys.Contains(GameKey.Up))
		{
			direction.y = 1f;
		}
		if (holdedKeys.Contains(GameKey.Down))
		{
			direction.y = -1f;
		}
	}

	private void CalcDirectionFromMousePosition()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x -= Screen.width / 2;
		mousePosition.y -= Screen.height / 2;
		if (Mathf.Sqrt(Mathf.Pow(mousePosition.x, 2f) + Mathf.Pow(mousePosition.y, 2f)) > 50f)
		{
			if ((double)mousePosition.x < 0.5 * (double)mousePosition.y && (double)mousePosition.x < -0.5 * (double)mousePosition.y)
			{
				direction.x = -1f;
			}
			if ((double)mousePosition.x >= 0.5 * (double)mousePosition.y && (double)mousePosition.x >= -0.5 * (double)mousePosition.y)
			{
				direction.x = 1f;
			}
			if ((double)mousePosition.y >= 0.5 * (double)mousePosition.x && (double)mousePosition.y >= -0.5 * (double)mousePosition.x)
			{
				direction.y = 1f;
			}
			if ((double)mousePosition.y < 0.5 * (double)mousePosition.x && (double)mousePosition.y < -0.5 * (double)mousePosition.x)
			{
				direction.y = -1f;
			}
		}
	}
}
