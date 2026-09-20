using System;
using System.Collections.Generic;
using CodeStage.AdvancedFPSCounter.CountersData;
using CodeStage.AdvancedFPSCounter.Labels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CodeStage.AdvancedFPSCounter;

[DisallowMultipleComponent]
[AddComponentMenu("Code Stage/Advanced FPS Counter")]
public class AFPSCounter : MonoBehaviour
{
	private const string MENU_PATH = "Code Stage/Advanced FPS Counter";

	private const string COMPONENT_NAME = "Advanced FPS Counter";

	internal const string LOG_PREFIX = "<b>[AFPSCounter]:</b> ";

	internal const char NEW_LINE = '\n';

	internal const char SPACE = ' ';

	public FPSCounterData fpsCounter = new FPSCounterData();

	public MemoryCounterData memoryCounter = new MemoryCounterData();

	public DeviceInfoCounterData deviceInfoCounter = new DeviceInfoCounterData();

	[Tooltip("Used to enable / disable plugin at runtime.\nSet to None to disable.")]
	public KeyCode hotKey = KeyCode.BackQuote;

	[Tooltip("Used to enable / disable plugin at runtime.\nMake two circle gestures with your finger \\ mouse to switch plugin on and off.")]
	public bool circleGesture;

	[Tooltip("Hot key modifier: any Control on Windows or any Command on Mac.")]
	public bool hotKeyCtrl;

	[Tooltip("Hot key modifier: any Shift.")]
	public bool hotKeyShift;

	[Tooltip("Hot key modifier: any Alt.")]
	public bool hotKeyAlt;

	[SerializeField]
	[Tooltip("Prevents current or other topmost Game Object from destroying on level (scene) load.\nApplied once, on Start phase.")]
	private bool keepAlive = true;

	private Canvas canvas;

	private CanvasScaler canvasScaler;

	private bool externalCanvas;

	private DrawableLabel[] labels;

	private int anchorsCount;

	private int cachedVSync = -1;

	private int cachedFrameRate = -1;

	private bool inited;

	private readonly List<Vector2> gesturePoints = new List<Vector2>();

	private int gestureCount;

	[SerializeField]
	[Tooltip("Disabled: removes labels and stops all internal processes except Hot Key listener.\n\nBackground: removes labels keeping counters alive; use for hidden performance monitoring.\n\nNormal: shows labels and runs all internal processes as usual.")]
	private OperationMode operationMode = OperationMode.Normal;

	[SerializeField]
	[Tooltip("Allows to see how your game performs on specified frame rate.\nDoes not guarantee selected frame rate. Set -1 to render as fast as possible in current conditions.\nIMPORTANT: this option disables VSync while enabled!")]
	private bool forceFrameRate;

	[Range(-1f, 200f)]
	[SerializeField]
	private int forcedFrameRate = -1;

	[SerializeField]
	[Tooltip("Background for all texts. Cheapest effect. Overhead: 1 Draw Call.")]
	private bool background = true;

	[SerializeField]
	[Tooltip("Color of the background.")]
	private Color backgroundColor = new Color32(0, 0, 0, 155);

	[Range(0f, 30f)]
	[SerializeField]
	[Tooltip("Padding of the background.")]
	private int backgroundPadding = 5;

	[SerializeField]
	[Tooltip("Shadow effect for all texts. This effect uses extra resources. Overhead: medium CPU and light GPU usage.")]
	private bool shadow;

	[SerializeField]
	[Tooltip("Color of the shadow effect.")]
	private Color shadowColor = new Color32(0, 0, 0, 128);

	[SerializeField]
	[Tooltip("Distance of the shadow effect.")]
	private Vector2 shadowDistance = new Vector2(1f, -1f);

	[Tooltip("Outline effect for all texts. Resource-heaviest effect. Overhead: huge CPU and medium GPU usage. Not recommended for use unless really necessary.")]
	[SerializeField]
	private bool outline;

	[SerializeField]
	[Tooltip("Color of the outline effect.")]
	private Color outlineColor = new Color32(0, 0, 0, 128);

	[SerializeField]
	[Tooltip("Distance of the outline effect.")]
	private Vector2 outlineDistance = new Vector2(1f, -1f);

	[SerializeField]
	[Tooltip("Controls own canvas scaler scale mode. Chec to use ScaleWithScreenSize. Otherwise ConstantPixelSize will be used.")]
	private bool autoScale;

	[Tooltip("Controls global scale of all texts.")]
	[Range(0f, 30f)]
	[SerializeField]
	private float scaleFactor = 1f;

	[Tooltip("Leave blank to use default font.")]
	[SerializeField]
	private Font labelsFont;

	[Tooltip("Set to 0 to use font size specified in the font importer.")]
	[Range(0f, 100f)]
	[SerializeField]
	private int fontSize = 14;

	[Range(0f, 10f)]
	[SerializeField]
	[Tooltip("Space between lines in labels.")]
	private float lineSpacing = 1f;

	[Tooltip("Lines count between different counters in a single label.")]
	[Range(0f, 10f)]
	[SerializeField]
	private int countersSpacing;

	[Tooltip("Pixel offset for anchored labels. Automatically applied to all labels.")]
	[SerializeField]
	private Vector2 paddingOffset = new Vector2(5f, 5f);

	[SerializeField]
	[Tooltip("Controls own canvas Pixel Perfect property.")]
	private bool pixelPerfect = true;

	[SerializeField]
	[Tooltip("Sorting order to use for the canvas.\nSet higher value to get closer to the user.")]
	private int sortingOrder = 10000;

	public bool KeepAlive => keepAlive;

	public OperationMode OperationMode
	{
		get
		{
			return operationMode;
		}
		set
		{
			if (operationMode == value || !Application.isPlaying)
			{
				return;
			}
			operationMode = value;
			if (operationMode != 0)
			{
				if (operationMode == OperationMode.Background)
				{
					for (int i = 0; i < anchorsCount; i++)
					{
						labels[i].Clear();
					}
				}
				OnEnable();
				fpsCounter.UpdateValue();
				memoryCounter.UpdateValue();
				deviceInfoCounter.UpdateValue();
				UpdateTexts();
			}
			else
			{
				OnDisable();
			}
		}
	}

	public bool ForceFrameRate
	{
		get
		{
			return forceFrameRate;
		}
		set
		{
			if (forceFrameRate != value && Application.isPlaying)
			{
				forceFrameRate = value;
				if (operationMode != 0)
				{
					RefreshForcedFrameRate();
				}
			}
		}
	}

	public int ForcedFrameRate
	{
		get
		{
			return forcedFrameRate;
		}
		set
		{
			if (forcedFrameRate != value && Application.isPlaying)
			{
				forcedFrameRate = value;
				if (operationMode != 0)
				{
					RefreshForcedFrameRate();
				}
			}
		}
	}

	public bool Background
	{
		get
		{
			return background;
		}
		set
		{
			if (background == value || !Application.isPlaying)
			{
				return;
			}
			background = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeBackground(background);
				}
			}
		}
	}

	public Color BackgroundColor
	{
		get
		{
			return backgroundColor;
		}
		set
		{
			if (backgroundColor == value || !Application.isPlaying)
			{
				return;
			}
			backgroundColor = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeBackgroundColor(backgroundColor);
				}
			}
		}
	}

	public int BackgroundPadding
	{
		get
		{
			return backgroundPadding;
		}
		set
		{
			if (backgroundPadding == value || !Application.isPlaying)
			{
				return;
			}
			backgroundPadding = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeBackgroundPadding(backgroundPadding);
				}
			}
		}
	}

	public bool Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow == value || !Application.isPlaying)
			{
				return;
			}
			shadow = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeShadow(shadow);
				}
			}
		}
	}

	public Color ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (shadowColor == value || !Application.isPlaying)
			{
				return;
			}
			shadowColor = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeShadowColor(shadowColor);
				}
			}
		}
	}

	public Vector2 ShadowDistance
	{
		get
		{
			return shadowDistance;
		}
		set
		{
			if (shadowDistance == value || !Application.isPlaying)
			{
				return;
			}
			shadowDistance = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeShadowDistance(shadowDistance);
				}
			}
		}
	}

	public bool Outline
	{
		get
		{
			return outline;
		}
		set
		{
			if (outline == value || !Application.isPlaying)
			{
				return;
			}
			outline = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeOutline(outline);
				}
			}
		}
	}

	public Color OutlineColor
	{
		get
		{
			return outlineColor;
		}
		set
		{
			if (outlineColor == value || !Application.isPlaying)
			{
				return;
			}
			outlineColor = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeOutlineColor(outlineColor);
				}
			}
		}
	}

	public Vector2 OutlineDistance
	{
		get
		{
			return outlineDistance;
		}
		set
		{
			if (outlineDistance == value || !Application.isPlaying)
			{
				return;
			}
			outlineDistance = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeOutlineDistance(outlineDistance);
				}
			}
		}
	}

	public bool AutoScale
	{
		get
		{
			return autoScale;
		}
		set
		{
			if (autoScale != value && Application.isPlaying)
			{
				autoScale = value;
				if (operationMode != 0 && labels != null && !(canvasScaler == null))
				{
					canvasScaler.uiScaleMode = (autoScale ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize);
				}
			}
		}
	}

	public float ScaleFactor
	{
		get
		{
			return scaleFactor;
		}
		set
		{
			if (!(Math.Abs(scaleFactor - value) < 0.001f) && Application.isPlaying)
			{
				scaleFactor = value;
				if (operationMode != 0 && !(canvasScaler == null) && !(canvasScaler == null))
				{
					canvasScaler.scaleFactor = scaleFactor;
				}
			}
		}
	}

	public Font LabelsFont
	{
		get
		{
			return labelsFont;
		}
		set
		{
			if (labelsFont == value || !Application.isPlaying)
			{
				return;
			}
			labelsFont = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeFont(labelsFont);
				}
			}
		}
	}

	public int FontSize
	{
		get
		{
			return fontSize;
		}
		set
		{
			if (fontSize == value || !Application.isPlaying)
			{
				return;
			}
			fontSize = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeFontSize(fontSize);
				}
			}
		}
	}

	public float LineSpacing
	{
		get
		{
			return lineSpacing;
		}
		set
		{
			if (Math.Abs(lineSpacing - value) < 0.001f || !Application.isPlaying)
			{
				return;
			}
			lineSpacing = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeLineSpacing(lineSpacing);
				}
			}
		}
	}

	public int CountersSpacing
	{
		get
		{
			return countersSpacing;
		}
		set
		{
			if (countersSpacing == value || !Application.isPlaying)
			{
				return;
			}
			countersSpacing = value;
			if (operationMode != 0 && labels != null)
			{
				UpdateTexts();
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].dirty = true;
				}
			}
		}
	}

	public Vector2 PaddingOffset
	{
		get
		{
			return paddingOffset;
		}
		set
		{
			if (paddingOffset == value || !Application.isPlaying)
			{
				return;
			}
			paddingOffset = value;
			if (operationMode != 0 && labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeOffset(paddingOffset);
				}
			}
		}
	}

	public bool PixelPerfect
	{
		get
		{
			return pixelPerfect;
		}
		set
		{
			if (pixelPerfect != value && Application.isPlaying)
			{
				pixelPerfect = value;
				if (operationMode != 0 && labels != null)
				{
					canvas.pixelPerfect = pixelPerfect;
				}
			}
		}
	}

	public int SortingOrder
	{
		get
		{
			return sortingOrder;
		}
		set
		{
			if (sortingOrder != value && Application.isPlaying)
			{
				sortingOrder = value;
				if (operationMode != 0 && !(canvas == null))
				{
					canvas.sortingOrder = sortingOrder;
				}
			}
		}
	}

	public static AFPSCounter Instance { get; private set; }

	private AFPSCounter()
	{
	}

	private static AFPSCounter GetOrCreateInstance(bool keepAlive)
	{
		if (Instance != null)
		{
			return Instance;
		}
		AFPSCounter aFPSCounter = UnityEngine.Object.FindObjectOfType<AFPSCounter>();
		if (aFPSCounter != null)
		{
			Instance = aFPSCounter;
		}
		else
		{
			CreateInScene(lookForExistingContainer: false).keepAlive = keepAlive;
		}
		return Instance;
	}

	public static AFPSCounter AddToScene()
	{
		return AddToScene(keepAlive: true);
	}

	public static AFPSCounter AddToScene(bool keepAlive)
	{
		return GetOrCreateInstance(keepAlive);
	}

	public static void Dispose()
	{
		if (Instance != null)
		{
			Instance.DisposeInternal();
		}
	}

	internal static string Color32ToHex(Color32 color)
	{
		return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + color.a.ToString("x2");
	}

	private static AFPSCounter CreateInScene(bool lookForExistingContainer = true)
	{
		GameObject gameObject = (lookForExistingContainer ? GameObject.Find("Advanced FPS Counter") : null);
		if (gameObject == null)
		{
			gameObject = new GameObject("Advanced FPS Counter")
			{
				layer = LayerMask.NameToLayer("UI")
			};
		}
		return gameObject.AddComponent<AFPSCounter>();
	}

	private void Awake()
	{
		if (Instance != null && Instance.keepAlive)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Instance = this;
		fpsCounter.Init(this);
		memoryCounter.Init(this);
		deviceInfoCounter.Init(this);
		ConfigureCanvas();
		ConfigureLabels();
		inited = true;
	}

	private void Start()
	{
		if (keepAlive)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.transform.root.gameObject);
			SceneManager.sceneLoaded += OnLevelWasLoadedNew;
		}
	}

	private void Update()
	{
		if (inited)
		{
			ProcessHotKey();
			if (circleGesture && CircleGestureMade())
			{
				SwitchCounter();
			}
		}
	}

	private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
	{
		OnLevelLoadedCallback();
	}

	private void OnLevelLoadedCallback()
	{
		if (inited && fpsCounter.Enabled)
		{
			fpsCounter.OnLevelLoadedCallback();
		}
	}

	private void OnEnable()
	{
		if (inited && operationMode != 0)
		{
			ActivateCounters();
			Invoke("RefreshForcedFrameRate", 0.5f);
		}
	}

	private void OnDisable()
	{
		if (!inited)
		{
			return;
		}
		DeactivateCounters();
		if (IsInvoking("RefreshForcedFrameRate"))
		{
			CancelInvoke("RefreshForcedFrameRate");
		}
		RefreshForcedFrameRate(disabling: true);
		for (int i = 0; i < anchorsCount; i++)
		{
			if (labels[i] != null)
			{
				labels[i].Clear();
			}
		}
	}

	private void OnDestroy()
	{
		if (inited)
		{
			fpsCounter.Dispose();
			memoryCounter.Dispose();
			deviceInfoCounter.Dispose();
			if (labels != null)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].Dispose();
				}
				Array.Clear(labels, 0, anchorsCount);
				labels = null;
			}
			inited = false;
		}
		if (canvas != null)
		{
			UnityEngine.Object.Destroy(canvas.gameObject);
		}
		if (base.transform.childCount <= 1)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (Instance == this)
		{
			Instance = null;
		}
	}

	internal void MakeDrawableLabelDirty(LabelAnchor anchor)
	{
		if (operationMode == OperationMode.Normal)
		{
			labels[(uint)anchor].dirty = true;
		}
	}

	internal void UpdateTexts()
	{
		if (operationMode != OperationMode.Normal)
		{
			return;
		}
		bool flag = false;
		if (fpsCounter.Enabled)
		{
			DrawableLabel drawableLabel = labels[(uint)fpsCounter.Anchor];
			if (drawableLabel.newText.Length > 0)
			{
				drawableLabel.newText.Append(new string('\n', countersSpacing + 1));
			}
			drawableLabel.newText.Append(fpsCounter.text);
			drawableLabel.dirty |= fpsCounter.dirty;
			fpsCounter.dirty = false;
			flag = true;
		}
		if (memoryCounter.Enabled)
		{
			DrawableLabel drawableLabel2 = labels[(uint)memoryCounter.Anchor];
			if (drawableLabel2.newText.Length > 0)
			{
				drawableLabel2.newText.Append(new string('\n', countersSpacing + 1));
			}
			drawableLabel2.newText.Append(memoryCounter.text);
			drawableLabel2.dirty |= memoryCounter.dirty;
			memoryCounter.dirty = false;
			flag = true;
		}
		if (deviceInfoCounter.Enabled)
		{
			DrawableLabel drawableLabel3 = labels[(uint)deviceInfoCounter.Anchor];
			if (drawableLabel3.newText.Length > 0)
			{
				drawableLabel3.newText.Append(new string('\n', countersSpacing + 1));
			}
			drawableLabel3.newText.Append(deviceInfoCounter.text);
			drawableLabel3.dirty |= deviceInfoCounter.dirty;
			deviceInfoCounter.dirty = false;
			flag = true;
		}
		if (flag)
		{
			for (int i = 0; i < anchorsCount; i++)
			{
				labels[i].CheckAndUpdate();
			}
		}
		else
		{
			for (int j = 0; j < anchorsCount; j++)
			{
				labels[j].Clear();
			}
		}
	}

	private void ConfigureCanvas()
	{
		if (GetComponentInParent<Canvas>() != null)
		{
			externalCanvas = true;
			RectTransform rectTransform = base.gameObject.GetComponent<RectTransform>();
			if (rectTransform == null)
			{
				rectTransform = base.gameObject.AddComponent<RectTransform>();
			}
			UIUtils.ResetRectTransform(rectTransform);
		}
		GameObject gameObject = new GameObject("CountersCanvas", typeof(Canvas));
		gameObject.tag = base.gameObject.tag;
		gameObject.layer = base.gameObject.layer;
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		canvas = gameObject.GetComponent<Canvas>();
		UIUtils.ResetRectTransform(gameObject.GetComponent<RectTransform>());
		if (!externalCanvas)
		{
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.pixelPerfect = pixelPerfect;
			canvas.sortingOrder = sortingOrder;
			canvasScaler = gameObject.AddComponent<CanvasScaler>();
			if (autoScale)
			{
				canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			}
			else
			{
				canvasScaler.scaleFactor = scaleFactor;
			}
		}
	}

	private void ConfigureLabels()
	{
		anchorsCount = Enum.GetNames(typeof(LabelAnchor)).Length;
		labels = new DrawableLabel[anchorsCount];
		for (int i = 0; i < anchorsCount; i++)
		{
			labels[i] = new DrawableLabel(canvas.gameObject, (LabelAnchor)i, new LabelEffect(background, backgroundColor, backgroundPadding), new LabelEffect(shadow, shadowColor, shadowDistance), new LabelEffect(outline, outlineColor, outlineDistance), labelsFont, fontSize, lineSpacing, paddingOffset);
		}
	}

	private void DisposeInternal()
	{
		UnityEngine.Object.Destroy(this);
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void ProcessHotKey()
	{
		if (hotKey != 0 && Input.GetKeyDown(hotKey) && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
		{
			bool flag = true;
			if (hotKeyCtrl)
			{
				flag &= Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
			}
			if (hotKeyAlt)
			{
				flag &= Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
			}
			if (hotKeyShift)
			{
				flag &= Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			}
			if (flag)
			{
				SwitchCounter();
			}
		}
	}

	private bool CircleGestureMade()
	{
		bool result = false;
		int num = gesturePoints.Count;
		if (Input.GetMouseButton(0))
		{
			Vector2 vector = Input.mousePosition;
			if (num == 0 || (vector - gesturePoints[num - 1]).magnitude > 10f)
			{
				gesturePoints.Add(vector);
				num++;
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			num = 0;
			gestureCount = 0;
			gesturePoints.Clear();
		}
		if (num < 10)
		{
			return result;
		}
		float num2 = 0f;
		Vector2 zero = Vector2.zero;
		Vector2 rhs = Vector2.zero;
		for (int i = 0; i < num - 2; i++)
		{
			Vector2 vector2 = gesturePoints[i + 1] - gesturePoints[i];
			zero += vector2;
			float magnitude = vector2.magnitude;
			num2 += magnitude;
			if (Vector2.Dot(vector2, rhs) < 0f)
			{
				gesturePoints.Clear();
				gestureCount = 0;
				return result;
			}
			rhs = vector2;
		}
		int num3 = (Screen.width + Screen.height) / 4;
		if (num2 > (float)num3 && zero.magnitude < (float)num3 / 2f)
		{
			gesturePoints.Clear();
			gestureCount++;
			if (gestureCount >= 2)
			{
				gestureCount = 0;
				result = true;
			}
		}
		return result;
	}

	public void SwitchCounter()
	{
		if (operationMode == OperationMode.Disabled)
		{
			OperationMode = OperationMode.Normal;
		}
		else if (operationMode == OperationMode.Normal)
		{
			OperationMode = OperationMode.Disabled;
		}
	}

	private void ActivateCounters()
	{
		fpsCounter.Activate();
		memoryCounter.Activate();
		deviceInfoCounter.Activate();
		if (fpsCounter.Enabled || memoryCounter.Enabled || deviceInfoCounter.Enabled)
		{
			UpdateTexts();
		}
	}

	private void DeactivateCounters()
	{
		if (!(Instance == null))
		{
			fpsCounter.Deactivate();
			memoryCounter.Deactivate();
			deviceInfoCounter.Deactivate();
		}
	}

	private void RefreshForcedFrameRate()
	{
		RefreshForcedFrameRate(disabling: false);
	}

	private void RefreshForcedFrameRate(bool disabling)
	{
		if (forceFrameRate && !disabling)
		{
			if (cachedVSync == -1)
			{
				cachedVSync = QualitySettings.vSyncCount;
				cachedFrameRate = Application.targetFrameRate;
				QualitySettings.vSyncCount = 0;
			}
			Application.targetFrameRate = forcedFrameRate;
		}
		else if (cachedVSync != -1)
		{
			QualitySettings.vSyncCount = cachedVSync;
			Application.targetFrameRate = cachedFrameRate;
			cachedVSync = -1;
		}
	}
}
