using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyInput : MonoBehaviour
{
	private static bool isInitialized;

	private static bool isGamepadActive = false;

	private static bool isInputActive = true;

	private static bool isStateForced;

	private static bool forcedState;

	private static bool isGamepadActivityForcedByPlatform;

	private static bool dontDestroyOnLoad;

	private static LazyInput instance;

	private static GamepadController gamepad;

	private static KeyboardController keyboard;

	private List<GameKey> holdedKeys = new List<GameKey>();

	private List<GameKey> pressedKeys = new List<GameKey>();

	private List<GameKey> ignoreUntilRelease = new List<GameKey>();

	private Vector2 direction = Vector2.zero;

	private Vector2 direction2 = Vector2.zero;

	[SerializeField]
	private GameBindings gameBindings;

	[SerializeField]
	private GamepadTypeData gamepadTypeConfiguration;

	private static bool debugGamepadTypeForced;

	private static GamepadType forcedGamepadType;

	private GamepadType currentGamepadType;

	[SerializeField]
	private ControllerIconLibrary controllerIconLibrary;

	private Controller currentGamepad;

	private Player rewiredPlayer;

	public static GameBindings GameBindings => Instance.gameBindings;

	public static bool IsInitialized => isInitialized;

	public static GamepadType CurrentGamepadType
	{
		get
		{
			if (debugGamepadTypeForced)
			{
				return forcedGamepadType;
			}
			return Instance.currentGamepadType;
		}
	}

	public static ControllerIconLibrary ControllerIconLibrary => instance.controllerIconLibrary;

	public static bool IsAnyKeyDown => Instance.pressedKeys.Count != 0;

	public static bool IsAnyKey => Instance.holdedKeys.Count != 0;

	public static bool IsGamepadActive
	{
		get
		{
			if (!isGamepadActivityForcedByPlatform)
			{
				if (!isStateForced)
				{
					return isGamepadActive;
				}
				return forcedState;
			}
			return true;
		}
	}

	public new static bool DontDestroyOnLoad
	{
		get
		{
			return dontDestroyOnLoad;
		}
		set
		{
			dontDestroyOnLoad = value;
			if (dontDestroyOnLoad && instance != null)
			{
				UnityEngine.Object.DontDestroyOnLoad(instance);
			}
		}
	}

	private static LazyInput Instance
	{
		get
		{
			if (!Application.isPlaying)
			{
				return UnityEngine.Object.FindObjectOfType<LazyInput>();
			}
			TryInit();
			return instance;
		}
	}

	private static bool ShouldActivateGamepadAtStartByPlatform => false;

	public static event Action OnInputChanged;

	public static event Action OnActiveGamepadChangedEvent;

	public static void TryInit()
	{
		if (!isInitialized)
		{
			LazyInput lazyInput = UnityEngine.Object.FindObjectOfType<LazyInput>();
			if (lazyInput == null)
			{
				lazyInput = new GameObject("LazyInput").AddComponent<LazyInput>();
			}
			instance = lazyInput;
			if (instance.gameBindings == null)
			{
				instance.gameBindings = LazySingletonSO<GameBindings>.Instance;
			}
			if (instance.gamepadTypeConfiguration == null)
			{
				instance.gamepadTypeConfiguration = LazySingletonSO<GamepadTypeData>.Instance;
			}
			if (instance.controllerIconLibrary == null)
			{
				instance.controllerIconLibrary = LazySingletonSO<ControllerIconLibrary>.Instance;
			}
			gamepad = new GamepadController(instance.gameBindings);
			keyboard = new KeyboardController(instance.gameBindings);
			isGamepadActive = isGamepadActivityForcedByPlatform || (ShouldActivateGamepadAtStartByPlatform && ReInput.controllers.joystickCount > 0);
			instance.rewiredPlayer = ReInput.players.GetPlayer(0);
			TryCacheConnectedGamepadType(isGamepadActive);
			ReInput.ControllerConnectedEvent += OnGamepadConnected;
			ReInput.ControllerDisconnectedEvent += OnGamepadDisconnected;
			isInitialized = true;
			if (instance.controllerIconLibrary != null)
			{
				instance.controllerIconLibrary.Init();
			}
			if (dontDestroyOnLoad)
			{
				UnityEngine.Object.DontDestroyOnLoad(instance);
			}
		}
	}

	public void SetConfigurationReferences(GameBindings gameBindings, GamepadTypeData gamepadTypeData, ControllerIconLibrary controllerIconLibrary)
	{
		this.gameBindings = gameBindings;
		gamepadTypeConfiguration = gamepadTypeData;
		this.controllerIconLibrary = controllerIconLibrary;
	}

	private void Update()
	{
		if (!isInitialized)
		{
			return;
		}
		keyboard.EraseMouseControl();
		if (!isInputActive)
		{
			return;
		}
		gamepad.Update();
		if (!isGamepadActivityForcedByPlatform)
		{
			keyboard.Update();
		}
		pressedKeys.Clear();
		holdedKeys.Clear();
		bool flag = false;
		if (isGamepadActivityForcedByPlatform)
		{
			flag = !isGamepadActive;
			isGamepadActive = true;
		}
		else if (gamepad.IsActive())
		{
			flag = !isGamepadActive;
			isGamepadActive = true;
		}
		else if (keyboard.IsActive())
		{
			flag = isGamepadActive;
			isGamepadActive = false;
		}
		if (IsGamepadActive && gamepad.IsActive())
		{
			CheckGamepadChange();
		}
		if (flag)
		{
			LazyInput.OnInputChanged?.Invoke();
		}
		BaseInputController baseInputController = DefineActiveInput();
		foreach (GameKey pressedKey in baseInputController.pressedKeys)
		{
			AddPressed(pressedKey);
		}
		foreach (GameKey holdedKey in baseInputController.holdedKeys)
		{
			AddHolded(holdedKey);
		}
		direction = baseInputController.Direction;
		direction2 = baseInputController.Direction2;
		CheckIgnoreUntilReleaseKeys();
	}

	private BaseInputController DefineActiveInput()
	{
		if (isGamepadActive)
		{
			return gamepad;
		}
		return keyboard;
	}

	private void AddPressed(GameKey key)
	{
		if (!(key == GameKey.None) && !pressedKeys.Contains(key) && !ignoreUntilRelease.Contains(key))
		{
			pressedKeys.Add(key);
		}
	}

	private void AddHolded(GameKey key)
	{
		if (!(key == GameKey.None) && !holdedKeys.Contains(key))
		{
			holdedKeys.Add(key);
		}
	}

	private void CheckIgnoreUntilReleaseKeys()
	{
		for (int i = 0; i < ignoreUntilRelease.Count; i++)
		{
			if (holdedKeys.Contains(ignoreUntilRelease[i]))
			{
				holdedKeys.Remove(ignoreUntilRelease[i]);
				continue;
			}
			ignoreUntilRelease.RemoveAt(i);
			i--;
		}
	}

	private bool CheckGamepadChange()
	{
		if (!ReInput.isReady)
		{
			return false;
		}
		rewiredPlayer = ReInput.players.GetPlayer(0);
		if (rewiredPlayer == null)
		{
			return false;
		}
		Controller lastActiveController = rewiredPlayer.controllers.GetLastActiveController();
		if (lastActiveController != currentGamepad)
		{
			OnActiveGamepadChanged(lastActiveController, out var gamepadTypeChanged);
			return gamepadTypeChanged;
		}
		return false;
	}

	private static void TryCacheConnectedGamepadType(bool notify)
	{
		if (ReInput.isReady && instance.rewiredPlayer != null)
		{
			Controller controller = null;
			if (notify)
			{
				controller = instance.rewiredPlayer.controllers.GetController(ControllerType.Joystick, 0);
			}
			else if (instance.rewiredPlayer.controllers.joystickCount > 0)
			{
				controller = instance.rewiredPlayer.controllers.Joysticks[0];
			}
			else if (ReInput.controllers.joystickCount > 0)
			{
				controller = ReInput.controllers.Joysticks[0];
			}
			OnActiveGamepadChanged(controller, out var _, notify);
		}
	}

	private static void OnGamepadConnected(ControllerStatusChangedEventArgs args)
	{
		bool num = !isGamepadActive;
		isGamepadActive = true;
		OnActiveGamepadChanged(args.controller, out var gamepadTypeChanged);
		if (num && !gamepadTypeChanged)
		{
			LazyInput.OnInputChanged?.Invoke();
		}
	}

	private static void OnGamepadDisconnected(ControllerStatusChangedEventArgs args)
	{
		bool num = isGamepadActive;
		isGamepadActive = isGamepadActivityForcedByPlatform || ReInput.controllers.joystickCount > 0;
		if (num != isGamepadActive)
		{
			LazyInput.OnInputChanged?.Invoke();
		}
		if (isGamepadActive && ReInput.isReady)
		{
			instance.rewiredPlayer = ReInput.players.GetPlayer(0);
			if (instance.rewiredPlayer != null)
			{
				OnActiveGamepadChanged(instance.rewiredPlayer.controllers.GetLastActiveController(), out var _);
			}
		}
	}

	private static void OnActiveGamepadChanged(Controller controller, out bool gamepadTypeChanged, bool notify = true)
	{
		if (controller == null)
		{
			gamepadTypeChanged = false;
			return;
		}
		GamepadType gamepadType = instance.currentGamepadType;
		instance.currentGamepad = controller;
		instance.currentGamepadType = instance.gamepadTypeConfiguration.GetTypeByGuid(controller.hardwareTypeGuid);
		gamepadTypeChanged = gamepadType != instance.currentGamepadType;
		if (gamepadTypeChanged && notify)
		{
			Debug.Log("Active gamepad changed:[" + controller.name + "] type:[" + instance.currentGamepadType.value + "]");
			LazyInput.OnActiveGamepadChangedEvent?.Invoke();
			LazyInput.OnInputChanged?.Invoke();
		}
	}

	public static bool GetKey(GameKey key)
	{
		if (!Instance.ignoreUntilRelease.Contains(key))
		{
			return Instance.holdedKeys.Contains(key);
		}
		return false;
	}

	public static bool AnyKeyDown()
	{
		return Instance.pressedKeys.Count > 0;
	}

	public static bool AnyKeyDownExceptSticks()
	{
		if (IsGamepadActive)
		{
			return gamepad.AnyKeyDownExceptSticks();
		}
		return AnyKeyDown();
	}

	public static bool GetKeyDown(GameKey key)
	{
		if (!Instance.ignoreUntilRelease.Contains(key))
		{
			return Instance.pressedKeys.Contains(key);
		}
		return false;
	}

	public static Vector2 GetDirection()
	{
		return Instance.direction;
	}

	public static Vector2 GetDirection2()
	{
		return Instance.direction2;
	}

	public static float MouseScrollDelta()
	{
		return keyboard.MouseScrollDelta;
	}

	public static void ClearKey(GameKey key)
	{
		Instance.holdedKeys.Remove(key);
	}

	public static void ClearKeyDown(GameKey key)
	{
		Instance.pressedKeys.Remove(key);
	}

	public static void ClearAllKeysDown()
	{
		Instance.pressedKeys.Clear();
	}

	public static void WaitForRelease(GameKey key)
	{
		if (!(key == GameKey.None) && !Instance.ignoreUntilRelease.Contains(key))
		{
			Instance.ignoreUntilRelease.Add(key);
		}
	}

	public static void SetInputActivity(bool isInputActive)
	{
		LazyInput.isInputActive = isInputActive;
	}

	public static bool IsInputActive()
	{
		return isInputActive;
	}

	public static void SetKeyboardMovementType(KeyboardController.MovementType movementType)
	{
		keyboard.CurrentMovementType = movementType;
	}

	public static KeyboardController.MovementType GetKeyboardMovementType()
	{
		return keyboard.CurrentMovementType;
	}

	public static void Vibrate(float value, float duration)
	{
		gamepad.Vibrate(value, duration);
	}

	public static bool AnyClick()
	{
		if (!GetKeyDown(GameKey.LeftClick))
		{
			return GetKeyDown(GameKey.RightClick);
		}
		return true;
	}

	public static void ForceGamepadActivityState(bool isActive)
	{
		isStateForced = true;
		forcedState = isActive;
	}

	public static void SetGamepadActivityForcedByPlatform(bool isForced)
	{
		bool num = IsGamepadActive;
		isGamepadActivityForcedByPlatform = isForced;
		if (isGamepadActivityForcedByPlatform)
		{
			isGamepadActive = true;
		}
		if (num != IsGamepadActive)
		{
			LazyInput.OnInputChanged?.Invoke();
		}
	}

	public static void ClearGamepadActivityState()
	{
		if (isStateForced)
		{
			bool num = IsGamepadActive;
			isStateForced = false;
			if (num != IsGamepadActive)
			{
				LazyInput.OnInputChanged?.Invoke();
			}
		}
	}

	public static void NotifyInputChanged()
	{
		LazyInput.OnInputChanged?.Invoke();
	}

	public static void ForceDebugGamepadType(GamepadType gamepadType)
	{
		debugGamepadTypeForced = true;
		forcedGamepadType = gamepadType;
	}

	public static void ClearForcedGamepadType()
	{
		debugGamepadTypeForced = false;
	}
}
