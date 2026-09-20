using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Unity.Collections;
using UnityEngine;

public class CursorController : LazySingleton<CursorController>, ICursorChanger
{
	private const bool IS_CONSOLE_BUILD = false;

	[SerializeField]
	private List<CursorConfiguration> cursorConfigurations = new List<CursorConfiguration>();

	[SerializeField]
	[ReadOnly]
	private List<CursorState> states = new List<CursorState>();

	private bool isInitialized;

	[SerializeField]
	private float mouseAutohideTime = 4f;

	private Vector2 lastMousePos = Vector2.zero;

	private float lastMoveTime;

	private readonly Dictionary<(Texture2D source, int scaleHundredths), Texture2D> scaledCursorCache = new Dictionary<(Texture2D, int), Texture2D>();

	private int appliedSoftwareCursorScaleHundredths;

	protected override void Awake()
	{
		base.Awake();
		if (!(LazySingleton<CursorController>.Instance != this))
		{
			Init();
		}
	}

	private void Start()
	{
		if (!isInitialized && LazySingleton<CursorController>.Instance == this)
		{
			Init();
		}
	}

	private void OnDestroy()
	{
		GameSettings.OnResolutionChanged -= OnResolutionChanged;
		ClearScaledCursorCache();
	}

	private void OnResolutionChanged(IntVector2 _)
	{
		if (isInitialized && GameSettings.Instance.cursorMode != 0 && GetSoftwareCursorScaleHundredths() != appliedSoftwareCursorScaleHundredths)
		{
			UpdateState();
		}
	}

	private void Update()
	{
		AutoHideCursor();
	}

	private void AutoHideCursor()
	{
		if (!LazyInput.IsGamepadActive && !(MainGame.Instance == null))
		{
			Vector2 vector = Input.mousePosition;
			float magnitude = (lastMousePos - vector).magnitude;
			lastMousePos = vector;
			bool visible = true;
			if (magnitude > 0.1f)
			{
				lastMoveTime = Time.time;
			}
			else if (Time.time - lastMoveTime > mouseAutohideTime)
			{
				visible = false;
			}
			if (MainGame.Instance.gameState != MainGame.GameState.InGame || LazyWindowsStackController.ActiveWindow != null)
			{
				visible = true;
				lastMoveTime = Time.time;
			}
			Cursor.visible = visible;
		}
	}

	public void Init()
	{
		if (!(LazySingleton<CursorController>.Instance != this) && !isInitialized)
		{
			AddCursorState(CursorType.Default, this);
			isInitialized = true;
			GameSettings.OnResolutionChanged += OnResolutionChanged;
		}
	}

	public static void AddCursorState(CursorType type, ICursorChanger changer)
	{
		LazySingleton<CursorController>.Instance.states.Add(new CursorState(type, changer));
		LazySingleton<CursorController>.Instance.UpdateState();
	}

	public static void RemoveAllWithType(CursorType cursorType)
	{
		for (int num = LazySingleton<CursorController>.Instance.states.Count - 1; num >= 0; num--)
		{
			CursorState cursorState = LazySingleton<CursorController>.Instance.states[num];
			if (cursorState != null && cursorState.type == cursorType)
			{
				RemoveCursorState(cursorState.changer);
			}
		}
	}

	public static void RemoveCursorState(ICursorChanger changer)
	{
		CursorState stateByChanger = GetStateByChanger(changer);
		if (stateByChanger != null)
		{
			LazySingleton<CursorController>.Instance.states.Remove(stateByChanger);
			LazySingleton<CursorController>.Instance.UpdateState();
		}
	}

	public static void UpdateCursorState()
	{
		LazySingleton<CursorController>.Instance.UpdateState();
	}

	public static void ChangeCursorVisibleState(bool visible)
	{
		Cursor.visible = visible;
		if (visible)
		{
			LazySingleton<CursorController>.Instance.ResetCursorState(ignoreInvisible: false);
		}
	}

	private void UpdateState()
	{
		if (states != null && states.Count != 0)
		{
			List<CursorState> list = states;
			CursorConfiguration configByType = GetConfigByType(list[list.Count - 1].type);
			CursorMode cursorMode = ((GameSettings.Instance.cursorMode != 0) ? CursorMode.ForceSoftware : CursorMode.Auto);
			Texture2D texture2D = configByType.sprite;
			Vector2 hotSpot = configByType.hotSpot;
			if (cursorMode == CursorMode.ForceSoftware)
			{
				int num = (appliedSoftwareCursorScaleHundredths = GetSoftwareCursorScaleHundredths());
				float num2 = (float)num / 100f;
				texture2D = GetScaledCursor(texture2D, num, num2);
				hotSpot *= num2;
			}
			Cursor.SetCursor(texture2D, hotSpot, cursorMode);
		}
	}

	private static int GetSoftwareCursorScaleHundredths()
	{
		if (GameSettings.Instance.cursorMode != GameCursorMode.Software150)
		{
			return 100;
		}
		return 150;
	}

	private Texture2D GetScaledCursor(Texture2D source, int scaleHundredths, float scale)
	{
		if (source == null || scaleHundredths <= 100)
		{
			return source;
		}
		(Texture2D, int) key = (source, scaleHundredths);
		if (scaledCursorCache.TryGetValue(key, out var value) && value != null)
		{
			return value;
		}
		int num = Mathf.RoundToInt((float)source.width * scale);
		int num2 = Mathf.RoundToInt((float)source.height * scale);
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGBA32, mipChain: false)
		{
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp,
			hideFlags = HideFlags.HideAndDontSave,
			name = $"{source.name}_x{scaleHundredths}"
		};
		Color32[] pixels = source.GetPixels32();
		Color32[] array = new Color32[num * num2];
		int width = source.width;
		int height = source.height;
		for (int i = 0; i < num2; i++)
		{
			int num3 = Mathf.Min(height - 1, Mathf.FloorToInt((float)i / scale)) * width;
			int num4 = i * num;
			for (int j = 0; j < num; j++)
			{
				array[num4 + j] = pixels[num3 + Mathf.Min(width - 1, Mathf.FloorToInt((float)j / scale))];
			}
		}
		texture2D.SetPixels32(array);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		scaledCursorCache[key] = texture2D;
		return texture2D;
	}

	private void ClearScaledCursorCache()
	{
		foreach (Texture2D value in scaledCursorCache.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value);
			}
		}
		scaledCursorCache.Clear();
	}

	private void ResetCursorState(bool ignoreInvisible)
	{
		if (states.Count <= 1)
		{
			return;
		}
		CursorState cursorState = null;
		if (!ignoreInvisible)
		{
			cursorState = states.Find((CursorState s) => s.type == CursorType.Invisible);
		}
		LazySingleton<CursorController>.Instance.states.Clear();
		AddCursorState(CursorType.Default, this);
		if (!ignoreInvisible && cursorState != null)
		{
			AddCursorState(CursorType.Invisible, cursorState.changer);
		}
	}

	private static CursorState GetStateByChanger(ICursorChanger changer)
	{
		try
		{
			for (int i = 0; i < LazySingleton<CursorController>.Instance.states.Count; i++)
			{
				CursorState cursorState = LazySingleton<CursorController>.Instance.states[i];
				if (cursorState != null && cursorState.changer == changer)
				{
					return cursorState;
				}
			}
		}
		catch (Exception)
		{
			return null;
		}
		return null;
	}

	private CursorConfiguration GetConfigByType(CursorType type)
	{
		return cursorConfigurations.Find((CursorConfiguration c) => c.type == type);
	}

	public static bool TryGetCursorConfiguration(CursorType type, out CursorConfiguration configuration)
	{
		configuration = null;
		if (LazySingleton<CursorController>.Instance == null)
		{
			return false;
		}
		configuration = LazySingleton<CursorController>.Instance.GetConfigByType(type);
		if (configuration != null)
		{
			return configuration.sprite != null;
		}
		return false;
	}

	public static float GetSoftwareCursorScale()
	{
		if (GameSettings.Instance == null)
		{
			return 1f;
		}
		if (GameSettings.Instance.cursorMode != GameCursorMode.Software150)
		{
			return 1f;
		}
		return 1.5f;
	}
}
