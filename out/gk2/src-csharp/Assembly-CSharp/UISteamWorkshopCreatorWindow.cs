using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISteamWorkshopCreatorWindow : LazyWindow<LazyWidgetDataBase>
{
	private const string HeaderTitle = "Steam Workshop Creator Interface";

	private const int MaxConsoleLines = 400;

	private const string TagTranslation = "Translation";

	private const string TagVoiceOver = "Voiceover";

	private const float FontSize = 16f;

	private const float ButtonHeight = 26f;

	private const float CloseButtonSize = 26f;

	private const float ItemTileHeight = 50f;

	private const bool EnableVoiceOverWorkshopType = false;

	private static readonly string[] TagOptions = new string[1] { "Translation" };

	private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

	private static UISteamWorkshopCreatorWindow instance;

	private SteamWorkshopCreatorService service;

	private readonly List<string> consoleLines = new List<string>();

	private readonly StringBuilder consoleBuilder = new StringBuilder();

	private const string ButtonTextStyleName = "small_font_bold-btn_red_active";

	private TextStyle headerStyle;

	private TextStyle bodyStyle;

	private TextStyle buttonStyle;

	private TextStyle buttonDisabledStyle;

	private TextTransition buttonTextTransition;

	private TextMeshProUGUI headerSource;

	private TextMeshProUGUI bodySource;

	private Image buttonGraphicTemplate;

	private RectTransform tilesContent;

	private TextMeshProUGUI consoleText;

	private ScrollRect consoleScroll;

	private GameObject createOverlay;

	private TMP_InputField nameInput;

	private int tagIndex;

	private TextMeshProUGUI tagValueLabel;

	private UISwitchButton tagSwitch;

	private TextMeshProUGUI folderPathLabel;

	private LazyButton createSubmitButton;

	private string selectedFolder = "";

	public static void Toggle()
	{
		if (SteamWorkshopCreatorConfig.Enabled)
		{
			if (!SteamManager.Initialized)
			{
				Debug.Log("[SteamWorkshopCreator] Steam is not initialized.");
			}
			else if (instance == null)
			{
				instance = CreateInstance();
				instance.Open(null);
			}
			else if (instance.IsShown)
			{
				instance.Close();
			}
			else
			{
				instance.Open(null);
			}
		}
	}

	private static UISteamWorkshopCreatorWindow CreateInstance()
	{
		GameObject gameObject = new GameObject("UISteamWorkshopCreatorWindow");
		Transform transform = FindUiRoot();
		if (transform != null)
		{
			gameObject.transform.SetParent(transform, worldPositionStays: false);
		}
		else
		{
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		Canvas obj = gameObject.AddComponent<Canvas>();
		obj.renderMode = RenderMode.ScreenSpaceOverlay;
		obj.overrideSorting = true;
		obj.sortingOrder = 500;
		if (transform == null)
		{
			CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			canvasScaler.scaleFactor = ResolutionConfig.GetUiScaleFactor();
		}
		gameObject.AddComponent<GraphicRaycaster>();
		gameObject.AddComponent<GamepadNavigationController>();
		UISteamWorkshopCreatorWindow uISteamWorkshopCreatorWindow = gameObject.AddComponent<UISteamWorkshopCreatorWindow>();
		uISteamWorkshopCreatorWindow.Init();
		return uISteamWorkshopCreatorWindow;
	}

	private static Transform FindUiRoot()
	{
		GUIElements gUIElements = GUIElements.Instance;
		if (gUIElements == null)
		{
			return null;
		}
		UIFitter privateField = GetPrivateField<UIFitter>(gUIElements, "uiFitter");
		if (!(privateField != null))
		{
			return gUIElements.Root;
		}
		return privateField.transform;
	}

	private void Awake()
	{
		service = new SteamWorkshopCreatorService();
		service.Logged += AppendConsole;
		service.ItemsChanged += RefreshTiles;
		ResolveUiTemplates();
		BuildUi();
	}

	protected override void Update()
	{
		base.Update();
		service?.Tick();
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		HideCreatePopup();
		RefreshTiles();
		AppendConsole("Steam Workshop Creator ready.");
	}

	public override void Close()
	{
		HideCreatePopup();
		base.Close();
	}

	protected override bool OnPressedBack()
	{
		if (createOverlay != null && createOverlay.activeSelf)
		{
			HideCreatePopup();
			return true;
		}
		return base.OnPressedBack();
	}

	private void OnDestroy()
	{
		if (service != null)
		{
			service.Logged -= AppendConsole;
			service.ItemsChanged -= RefreshTiles;
			service.Dispose();
			service = null;
		}
		if (instance == this)
		{
			instance = null;
		}
	}

	protected override void TestDraw()
	{
	}

	private void ResolveUiTemplates()
	{
		UIDialogWindow uIDialogWindow = FindDialogWindow();
		if (uIDialogWindow == null)
		{
			return;
		}
		headerSource = GetPrivateField<TextMeshProUGUI>(uIDialogWindow, "header");
		bodySource = GetPrivateField<TextMeshProUGUI>(uIDialogWindow, "informationBotText");
		if (bodySource == null)
		{
			bodySource = GetPrivateField<TextMeshProUGUI>(uIDialogWindow, "information");
		}
		UIDialogWindowButton privateField = GetPrivateField<UIDialogWindowButton>(uIDialogWindow, "buttonPrefab");
		if (privateField != null)
		{
			LazyButton lazyButton = privateField.LazyButton;
			if (lazyButton != null && lazyButton.targetGraphic is Image image)
			{
				buttonGraphicTemplate = image;
			}
			if (buttonGraphicTemplate == null)
			{
				buttonGraphicTemplate = privateField.GetComponentInChildren<Image>(includeInactive: true);
			}
			if (lazyButton != null)
			{
				List<TextTransition> privateField2 = GetPrivateField<List<TextTransition>>(lazyButton, "textTransitions");
				if (privateField2 != null && privateField2.Count > 0)
				{
					buttonTextTransition = privateField2[0];
					buttonStyle = buttonTextTransition.defaultStyle;
					buttonDisabledStyle = buttonTextTransition.disabledStyle;
				}
			}
			if (buttonStyle == null)
			{
				TextMeshProUGUI privateField3 = GetPrivateField<TextMeshProUGUI>(privateField, "label");
				if (privateField3 != null)
				{
					TextStyleComponent component = privateField3.GetComponent<TextStyleComponent>();
					if (component != null)
					{
						buttonStyle = component.CurrentTextStyle;
					}
				}
			}
		}
		if (buttonStyle == null)
		{
			buttonStyle = FindTextStyleByName("small_font_bold-btn_red_active");
		}
		if (headerSource != null)
		{
			TextStyleComponent component2 = headerSource.GetComponent<TextStyleComponent>();
			if (component2 != null)
			{
				headerStyle = component2.CurrentTextStyle;
			}
		}
		if (bodySource != null)
		{
			TextStyleComponent component3 = bodySource.GetComponent<TextStyleComponent>();
			if (component3 != null)
			{
				bodyStyle = component3.CurrentTextStyle;
			}
		}
	}

	private static T GetPrivateField<T>(object obj, string name) where T : class
	{
		Type type = obj.GetType();
		while (type != null)
		{
			FieldInfo field = type.GetField(name, PrivateInstance);
			if (field != null)
			{
				return field.GetValue(obj) as T;
			}
			type = type.BaseType;
		}
		return null;
	}

	private static UIDialogWindow FindDialogWindow()
	{
		try
		{
			return LazyUI.GetWindow<UIDialogWindow>();
		}
		catch
		{
			return null;
		}
	}

	private void BuildUi()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		Image image = base.gameObject.AddComponent<Image>();
		image.color = new Color(0.05f, 0.03f, 0.02f, 0.92f);
		image.raycastTarget = true;
		RectTransform rectTransform2 = CreatePanel(rectTransform, "Frame", new Color(0.16f, 0.11f, 0.08f, 0.98f));
		Stretch(rectTransform2, Vector2.zero, Vector2.one, new Vector2(16f, 16f), new Vector2(-16f, -16f));
		RectTransform rectTransform3 = CreatePanel(rectTransform2, "Header", new Color(0.22f, 0.14f, 0.1f, 1f));
		Stretch(rectTransform3, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -40f), Vector2.zero);
		Stretch(CreateLabel(rectTransform3, "Title", "Steam Workshop Creator Interface", TextAlignmentOptions.MidlineLeft, header: true).rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-48f, 0f));
		closeButton = CreateRedButton(rectTransform3, "CloseButton", "X");
		PlaceCloseButton((RectTransform)closeButton.transform);
		RectTransform rectTransform4 = CreateRect(rectTransform2, "Body");
		Stretch(rectTransform4, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -48f));
		RectTransform rectTransform5 = CreatePanel(rectTransform4, "TilesArea", new Color(0.12f, 0.08f, 0.06f, 1f));
		Stretch(rectTransform5, new Vector2(0f, 0.3f), Vector2.one, Vector2.zero, Vector2.zero);
		Stretch((RectTransform)CreateScroll(rectTransform5, "TilesScroll", out var viewport, out tilesContent).transform, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
		Stretch(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		VerticalLayoutGroup verticalLayoutGroup = tilesContent.gameObject.AddComponent<VerticalLayoutGroup>();
		verticalLayoutGroup.spacing = 6f;
		verticalLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
		verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
		verticalLayoutGroup.childControlHeight = false;
		verticalLayoutGroup.childControlWidth = true;
		verticalLayoutGroup.childForceExpandHeight = false;
		verticalLayoutGroup.childForceExpandWidth = true;
		tilesContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		PinTopStretch(tilesContent);
		RectTransform rectTransform6 = CreatePanel(rectTransform4, "ConsoleArea", new Color(0.07f, 0.06f, 0.05f, 1f));
		Stretch(rectTransform6, Vector2.zero, new Vector2(1f, 0.3f), Vector2.zero, Vector2.zero);
		Stretch(CreateLabel(rectTransform6, "ConsoleHeader", "Console", TextAlignmentOptions.MidlineLeft, header: true).rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(12f, -28f), new Vector2(-12f, 0f));
		consoleScroll = CreateScroll(rectTransform6, "ConsoleScroll", out var viewport2, out var _);
		Stretch((RectTransform)consoleScroll.transform, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -32f));
		Stretch(viewport2, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		consoleText = CreateLabel(viewport2, "ConsoleText", "", TextAlignmentOptions.TopLeft);
		consoleText.enableWordWrapping = true;
		consoleText.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		PinTopStretch(consoleText.rectTransform);
		Vector2 offsetMin = consoleText.rectTransform.offsetMin;
		offsetMin.x = 4f;
		consoleText.rectTransform.offsetMin = offsetMin;
		consoleScroll.content = consoleText.rectTransform;
		BuildCreateOverlay(rectTransform);
	}

	private void BuildCreateOverlay(RectTransform root)
	{
		createOverlay = CreatePanel(root, "CreateOverlay", new Color(0f, 0f, 0f, 0.55f)).gameObject;
		Stretch((RectTransform)createOverlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		RectTransform rectTransform = CreatePanel(createOverlay.transform, "CreatePopup", new Color(0.18f, 0.12f, 0.09f, 1f));
		rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.sizeDelta = new Vector2(520f, 360f);
		RectTransform rectTransform2 = CreatePanel(rectTransform, "PopupHeader", new Color(0.24f, 0.15f, 0.11f, 1f));
		Stretch(rectTransform2, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -40f), Vector2.zero);
		Stretch(CreateLabel(rectTransform2, "PopupTitle", "Create workshop item", TextAlignmentOptions.MidlineLeft, header: true).rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-44f, 0f));
		LazyButton lazyButton = CreateRedButton(rectTransform2, "PopupClose", "X");
		lazyButton.onClick.AddListener(HideCreatePopup);
		PlaceCloseButton((RectTransform)lazyButton.transform);
		Stretch(CreateLabel(rectTransform, "NameLabel", "Name", TextAlignmentOptions.MidlineLeft).rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -72f), new Vector2(-16f, -48f));
		nameInput = CreateInputField(rectTransform, "NameInput");
		Stretch((RectTransform)nameInput.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -108f), new Vector2(-16f, -76f));
		nameInput.onValueChanged.AddListener(delegate
		{
			RefreshCreateButtonState();
		});
		tagSwitch = CreateTagSwitch(rectTransform);
		if (tagSwitch != null)
		{
			PlaceTagSwitch((RectTransform)tagSwitch.transform);
		}
		else
		{
			Stretch(CreateLabel(rectTransform, "TagLabel", "Type", TextAlignmentOptions.MidlineLeft).rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -140f), new Vector2(-16f, -116f));
			LazyButton lazyButton2 = CreateRedButton(rectTransform, "TagButton", "Translation");
			lazyButton2.onClick.AddListener(CycleTag);
			Stretch((RectTransform)lazyButton2.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -176f), new Vector2(-16f, -144f));
			tagValueLabel = GetButtonLabel(lazyButton2);
		}
		TextMeshProUGUI textMeshProUGUI = CreateLabel(rectTransform, "FolderHelp", "Linked folder: Steam uploads this directory as the workshop item content. Pick the folder that contains your files.", TextAlignmentOptions.TopLeft);
		textMeshProUGUI.enableWordWrapping = true;
		Stretch(textMeshProUGUI.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -228f), new Vector2(-16f, -176f));
		folderPathLabel = CreateLabel(rectTransform, "FolderPath", "No folder selected", TextAlignmentOptions.MidlineLeft);
		folderPathLabel.enableWordWrapping = false;
		folderPathLabel.overflowMode = TextOverflowModes.Ellipsis;
		Stretch(folderPathLabel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -264f), new Vector2(-56f, -232f));
		LazyButton lazyButton3 = CreateRedButton(rectTransform, "BrowseButton", "...");
		lazyButton3.onClick.AddListener(BrowseFolder);
		RectTransform obj = (RectTransform)lazyButton3.transform;
		obj.anchorMin = new Vector2(1f, 1f);
		obj.anchorMax = new Vector2(1f, 1f);
		obj.pivot = new Vector2(1f, 1f);
		obj.anchoredPosition = new Vector2(-16f, -232f);
		SizeButtonToText(lazyButton3, 26f);
		createSubmitButton = CreateRedButton(rectTransform, "CreateButton", "Create");
		createSubmitButton.onClick.AddListener(OnCreatePressed);
		RectTransform obj2 = (RectTransform)createSubmitButton.transform;
		obj2.anchorMin = new Vector2(0.5f, 0f);
		obj2.anchorMax = new Vector2(0.5f, 0f);
		obj2.pivot = new Vector2(0.5f, 0f);
		obj2.anchoredPosition = new Vector2(0f, 16f);
		obj2.sizeDelta = new Vector2(280f, 26f);
		createOverlay.SetActive(value: false);
	}

	private void RefreshTiles()
	{
		if (tilesContent == null || service == null)
		{
			return;
		}
		for (int num = tilesContent.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(tilesContent.GetChild(num).gameObject);
		}
		CreateCreateTile();
		foreach (SteamWorkshopItemRecord item in service.Items)
		{
			CreateItemTile(item);
		}
	}

	private void CreateCreateTile()
	{
		RectTransform rectTransform = CreatePanel(tilesContent, "CreateTile", new Color(0.28f, 0.18f, 0.12f, 1f));
		rectTransform.sizeDelta = new Vector2(0f, 50f);
		LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
		layoutElement.minHeight = 50f;
		layoutElement.preferredHeight = 50f;
		LazyButton lazyButton = CreateRedButton(rectTransform, "CreateNewButton", "Create new workshop item");
		lazyButton.onClick.AddListener(ShowCreatePopup);
		Stretch((RectTransform)lazyButton.transform, Vector2.zero, Vector2.one, new Vector2(12f, 7f), new Vector2(-12f, -7f));
	}

	private void CreateItemTile(SteamWorkshopItemRecord record)
	{
		RectTransform rectTransform = CreatePanel(tilesContent, "ItemTile", new Color(0.24f, 0.17f, 0.13f, 1f));
		rectTransform.sizeDelta = new Vector2(0f, 50f);
		LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
		layoutElement.minHeight = 50f;
		layoutElement.preferredHeight = 50f;
		Stretch(CreateLabel(rectTransform, "ItemTitle", record.title, TextAlignmentOptions.MidlineLeft, header: true).rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 25f), new Vector2(-180f, -2f));
		TextMeshProUGUI textMeshProUGUI = CreateLabel(rectTransform, "ItemMeta", $"{record.tag}  {record.publishedFileId}", TextAlignmentOptions.MidlineLeft);
		textMeshProUGUI.enableWordWrapping = false;
		textMeshProUGUI.overflowMode = TextOverflowModes.Ellipsis;
		Stretch(textMeshProUGUI.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 2f), new Vector2(-180f, -25f));
		LazyButton lazyButton = CreateRedButton(rectTransform, "DeleteButton", "Delete");
		lazyButton.onClick.AddListener(delegate
		{
			ConfirmDelete(record);
		});
		PlaceRightButton((RectTransform)lazyButton.transform, -8f);
		SizeButtonToText(lazyButton);
		LazyButton lazyButton2 = CreateRedButton(rectTransform, "UploadButton", "Upload");
		lazyButton2.onClick.AddListener(delegate
		{
			service.Upload(record);
		});
		PlaceRightButton(xFromRight: -16f - ((RectTransform)lazyButton.transform).sizeDelta.x, rect: (RectTransform)lazyButton2.transform);
		SizeButtonToText(lazyButton2);
	}

	private void ShowCreatePopup()
	{
		selectedFolder = "";
		tagIndex = 0;
		if (nameInput != null)
		{
			nameInput.text = "";
		}
		if (tagSwitch != null)
		{
			tagSwitch.UpdateField(0, fireCallback: false);
		}
		if (tagValueLabel != null)
		{
			tagValueLabel.text = TagOptions[tagIndex];
		}
		if (folderPathLabel != null)
		{
			folderPathLabel.text = "No folder selected";
		}
		RefreshCreateButtonState();
		createOverlay.SetActive(value: true);
		createOverlay.transform.SetAsLastSibling();
	}

	private void HideCreatePopup()
	{
		if (createOverlay != null)
		{
			createOverlay.SetActive(value: false);
		}
	}

	private UISwitchButton CreateTagSwitch(Transform parent)
	{
		UISwitchButton uISwitchButton = null;
		try
		{
			UIGameSettingsWindow window = LazyUI.GetWindow<UIGameSettingsWindow>();
			if (window != null)
			{
				uISwitchButton = GetPrivateField<UISwitchButton>(window, "languageButton") ?? GetPrivateField<UISwitchButton>(window, "fullscreenButton");
			}
		}
		catch
		{
			uISwitchButton = null;
		}
		if (uISwitchButton == null)
		{
			return null;
		}
		UISwitchButton uISwitchButton2 = UnityEngine.Object.Instantiate(uISwitchButton, parent, worldPositionStays: false);
		uISwitchButton2.name = "TagSwitch";
		uISwitchButton2.gameObject.SetActive(value: true);
		LocalizedLabel[] componentsInChildren = uISwitchButton2.GetComponentsInChildren<LocalizedLabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].IgnoreLocalize = true;
		}
		TMP_Text[] componentsInChildren2 = uISwitchButton2.GetComponentsInChildren<TMP_Text>(includeInactive: true);
		foreach (TMP_Text tMP_Text in componentsInChildren2)
		{
			if (tMP_Text.GetComponent<StaticFontLabel>() == null)
			{
				tMP_Text.gameObject.AddComponent<StaticFontLabel>();
			}
		}
		uISwitchButton2.Initialize(delegate(int index)
		{
			tagIndex = index;
			RefreshCreateButtonState();
		}, TagOptions, 0, "Type");
		return uISwitchButton2;
	}

	private static void PlaceTagSwitch(RectTransform rect)
	{
		rect.localScale = Vector3.one;
		rect.anchorMin = new Vector2(0f, 1f);
		rect.anchorMax = new Vector2(0f, 1f);
		rect.pivot = new Vector2(0f, 0.5f);
		rect.sizeDelta = new Vector2(330f, 16f);
		rect.anchoredPosition = new Vector2(16f, -148f);
	}

	private void CycleTag()
	{
		tagIndex = (tagIndex + 1) % TagOptions.Length;
		if (tagValueLabel != null)
		{
			tagValueLabel.text = TagOptions[tagIndex];
		}
		RefreshCreateButtonState();
	}

	private void RefreshCreateButtonState()
	{
		if (!(createSubmitButton == null))
		{
			bool flag = nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text);
			bool flag2 = Directory.Exists(selectedFolder);
			bool flag3 = !string.IsNullOrWhiteSpace(GetSelectedTag());
			createSubmitButton.interactable = flag && flag2 && flag3;
		}
	}

	private string GetSelectedTag()
	{
		return TagOptions[Mathf.Clamp(tagIndex, 0, TagOptions.Length - 1)];
	}

	private void BrowseFolder()
	{
		string text = PickFolder(Directory.Exists(selectedFolder) ? selectedFolder : ModsPaths.Root);
		if (!string.IsNullOrEmpty(text) && Directory.Exists(text))
		{
			selectedFolder = text;
			if (folderPathLabel != null)
			{
				folderPathLabel.text = selectedFolder;
			}
			service.EnsureThumbnail(selectedFolder);
			RefreshCreateButtonState();
		}
	}

	private static string PickFolder(string startDirectory)
	{
		return Win32FolderPicker.PickFolder("Select workshop item folder", startDirectory);
	}

	private void OnCreatePressed()
	{
		string title = ((nameInput != null) ? nameInput.text.Trim() : "");
		string selectedTag = GetSelectedTag();
		string localFolder = selectedFolder;
		HideCreatePopup();
		service.CreateAndUpload(title, selectedTag, localFolder);
	}

	private void ConfirmDelete(SteamWorkshopItemRecord record)
	{
		UIDialogWindow dialog = FindDialogWindow();
		if (dialog == null)
		{
			service.Delete(record);
			return;
		}
		UIDialogWindowData uIDialogWindowData = new UIDialogWindowData("Delete workshop item", "This deletes the item on Steam and removes it from the local creator list. Files in the linked folder are not deleted.", Yes, dialog.Close);
		uIDialogWindowData.ShowCloseButton = true;
		dialog.Open(uIDialogWindowData);
		void Yes()
		{
			dialog.Close();
			service.Delete(record);
		}
	}

	private void AppendConsole(string line)
	{
		if (string.IsNullOrEmpty(line))
		{
			return;
		}
		consoleLines.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
		while (consoleLines.Count > 400)
		{
			consoleLines.RemoveAt(0);
		}
		consoleBuilder.Clear();
		for (int i = 0; i < consoleLines.Count; i++)
		{
			if (i > 0)
			{
				consoleBuilder.Append('\n');
			}
			consoleBuilder.Append(consoleLines[i]);
		}
		if (consoleText != null)
		{
			consoleText.text = consoleBuilder.ToString();
			LayoutRebuilder.ForceRebuildLayoutImmediate(consoleText.rectTransform);
		}
		if (consoleScroll != null)
		{
			Canvas.ForceUpdateCanvases();
			consoleScroll.verticalNormalizedPosition = 0f;
		}
	}

	private RectTransform CreateRect(Transform parent, string name)
	{
		GameObject obj = new GameObject(name, typeof(RectTransform));
		obj.transform.SetParent(parent, worldPositionStays: false);
		obj.transform.localScale = Vector3.one;
		return (RectTransform)obj.transform;
	}

	private RectTransform CreatePanel(Transform parent, string name, Color color)
	{
		RectTransform rectTransform = CreateRect(parent, name);
		Image image = rectTransform.gameObject.AddComponent<Image>();
		image.color = color;
		image.raycastTarget = true;
		return rectTransform;
	}

	private TextMeshProUGUI CreateLabel(Transform parent, string name, string text, TextAlignmentOptions alignment, bool header = false)
	{
		TextMeshProUGUI textMeshProUGUI = CreateRect(parent, name).gameObject.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.text = text;
		textMeshProUGUI.alignment = alignment;
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.enableAutoSizing = false;
		if (textMeshProUGUI.GetComponent<StaticFontLabel>() == null)
		{
			textMeshProUGUI.gameObject.AddComponent<StaticFontLabel>();
		}
		ApplyGameTextStyle(textMeshProUGUI, header);
		return textMeshProUGUI;
	}

	private void ApplyGameTextStyle(TextMeshProUGUI label, bool header)
	{
		TextStyle textStyle = (header ? headerStyle : bodyStyle);
		TextMeshProUGUI textMeshProUGUI = (header ? headerSource : bodySource);
		if (textStyle == null && !header)
		{
			textStyle = headerStyle;
			textMeshProUGUI = headerSource;
		}
		if (textStyle != null)
		{
			textStyle.ApplyStyle(label);
		}
		else if (textMeshProUGUI != null)
		{
			label.font = textMeshProUGUI.font;
			label.fontSharedMaterial = textMeshProUGUI.fontSharedMaterial;
			label.color = textMeshProUGUI.color;
			label.extraPadding = textMeshProUGUI.extraPadding;
			label.fontSize = textMeshProUGUI.fontSize;
		}
		label.enableAutoSizing = false;
		label.ForceMeshUpdate();
	}

	private LazyButton CreateRedButton(Transform parent, string name, string text)
	{
		RectTransform rectTransform = CreateRect(parent, name);
		Image image = rectTransform.gameObject.AddComponent<Image>();
		ApplyButtonGraphic(image);
		image.raycastTarget = true;
		LazyButton lazyButton = rectTransform.gameObject.AddComponent<LazyButton>();
		lazyButton.targetGraphic = image;
		rectTransform.gameObject.AddComponent<GamepadNavigationItem>();
		TextMeshProUGUI textMeshProUGUI = CreateLabel(rectTransform, "Label", text, TextAlignmentOptions.Center);
		textMeshProUGUI.enableWordWrapping = false;
		textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
		Stretch(textMeshProUGUI.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
		BindButtonText(lazyButton, textMeshProUGUI);
		rectTransform.sizeDelta = new Vector2(Mathf.Max(textMeshProUGUI.preferredWidth + 24f, 48f), 26f);
		return lazyButton;
	}

	private void BindButtonText(LazyButton button, TextMeshProUGUI label)
	{
		TextStyle textStyle = buttonStyle ?? FindTextStyleByName("small_font_bold-btn_red_active");
		if (textStyle != null)
		{
			textStyle.ApplyStyle(label);
		}
		List<TextTransition> privateField = GetPrivateField<List<TextTransition>>(button, "textTransitions");
		if (privateField != null)
		{
			privateField.Clear();
			privateField.Add(new TextTransition
			{
				targetLabel = label,
				defaultStyle = (buttonTextTransition?.defaultStyle ?? textStyle),
				highlightedStyle = (buttonTextTransition?.highlightedStyle ?? textStyle),
				pressedStyle = (buttonTextTransition?.pressedStyle ?? textStyle),
				selectedStyle = (buttonTextTransition?.selectedStyle ?? textStyle),
				disabledStyle = (buttonTextTransition?.disabledStyle ?? buttonDisabledStyle ?? textStyle)
			});
			button.RefreshTextTransitions();
		}
		label.enableAutoSizing = false;
		label.ForceMeshUpdate();
	}

	private static TextStyle FindTextStyleByName(string name)
	{
		TextStyle[] array = Resources.FindObjectsOfTypeAll<TextStyle>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].name == name)
			{
				return array[i];
			}
		}
		return null;
	}

	private void ApplyButtonGraphic(Image image)
	{
		if (buttonGraphicTemplate == null)
		{
			image.color = new Color(0.72f, 0.16f, 0.12f, 1f);
			return;
		}
		image.sprite = buttonGraphicTemplate.sprite;
		image.type = buttonGraphicTemplate.type;
		image.fillCenter = buttonGraphicTemplate.fillCenter;
		image.pixelsPerUnitMultiplier = buttonGraphicTemplate.pixelsPerUnitMultiplier;
		image.material = buttonGraphicTemplate.material;
		image.color = buttonGraphicTemplate.color;
		image.preserveAspect = false;
	}

	private static TextMeshProUGUI GetButtonLabel(LazyButton button)
	{
		return button.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
	}

	private static void PlaceCloseButton(RectTransform rect)
	{
		rect.anchorMin = new Vector2(1f, 0.5f);
		rect.anchorMax = new Vector2(1f, 0.5f);
		rect.pivot = new Vector2(1f, 0.5f);
		rect.anchoredPosition = new Vector2(-6f, 0f);
		rect.sizeDelta = new Vector2(26f, 26f);
	}

	private static void PlaceRightButton(RectTransform rect, float xFromRight)
	{
		rect.anchorMin = new Vector2(1f, 0.5f);
		rect.anchorMax = new Vector2(1f, 0.5f);
		rect.pivot = new Vector2(1f, 0.5f);
		rect.anchoredPosition = new Vector2(xFromRight, 0f);
	}

	private static void SizeButtonToText(LazyButton button, float minWidth = 48f)
	{
		TextMeshProUGUI buttonLabel = GetButtonLabel(button);
		float x = minWidth;
		if (buttonLabel != null)
		{
			x = Mathf.Max(minWidth, buttonLabel.preferredWidth + 24f);
		}
		((RectTransform)button.transform).sizeDelta = new Vector2(x, 26f);
	}

	private TMP_InputField CreateInputField(Transform parent, string name)
	{
		RectTransform rectTransform = CreatePanel(parent, name, new Color(0.08f, 0.06f, 0.05f, 1f));
		RectTransform rectTransform2 = CreateRect(rectTransform, "Text Area");
		Stretch(rectTransform2, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
		rectTransform2.gameObject.AddComponent<RectMask2D>();
		TextMeshProUGUI textMeshProUGUI = CreateLabel(rectTransform2, "Placeholder", "", TextAlignmentOptions.MidlineLeft);
		Stretch(textMeshProUGUI.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		textMeshProUGUI.enableWordWrapping = false;
		textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
		Color color = textMeshProUGUI.color;
		color.a = 0.45f;
		textMeshProUGUI.color = color;
		TextMeshProUGUI textMeshProUGUI2 = CreateLabel(rectTransform2, "Text", "", TextAlignmentOptions.MidlineLeft);
		Stretch(textMeshProUGUI2.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		textMeshProUGUI2.enableWordWrapping = false;
		textMeshProUGUI2.overflowMode = TextOverflowModes.Overflow;
		textMeshProUGUI2.raycastTarget = true;
		textMeshProUGUI2.margin = new Vector4(2f, 0f, 0f, 0f);
		textMeshProUGUI.margin = new Vector4(2f, 0f, 0f, 0f);
		TMP_FontAsset font = textMeshProUGUI2.font;
		Material fontSharedMaterial = textMeshProUGUI2.fontSharedMaterial;
		float fontSize = ((textMeshProUGUI2.fontSize > 0f) ? textMeshProUGUI2.fontSize : 16f);
		TMP_InputField tMP_InputField = rectTransform.gameObject.AddComponent<TMP_InputField>();
		tMP_InputField.textViewport = rectTransform2;
		tMP_InputField.textComponent = textMeshProUGUI2;
		tMP_InputField.placeholder = textMeshProUGUI;
		if (font != null)
		{
			tMP_InputField.fontAsset = font;
		}
		RestoreLabelFont(textMeshProUGUI2, font, fontSharedMaterial, fontSize);
		RestoreLabelFont(textMeshProUGUI, font, fontSharedMaterial, fontSize);
		textMeshProUGUI2.margin = new Vector4(2f, 0f, 0f, 0f);
		textMeshProUGUI.margin = new Vector4(2f, 0f, 0f, 0f);
		tMP_InputField.customCaretColor = true;
		tMP_InputField.caretColor = Color.white;
		tMP_InputField.caretWidth = 2;
		tMP_InputField.lineType = TMP_InputField.LineType.SingleLine;
		return tMP_InputField;
	}

	private static void RestoreLabelFont(TextMeshProUGUI label, TMP_FontAsset font, Material material, float fontSize)
	{
		if (font != null)
		{
			label.font = font;
		}
		if (material != null)
		{
			label.fontSharedMaterial = material;
		}
		label.fontSize = fontSize;
		label.enableAutoSizing = false;
		label.enableWordWrapping = false;
		label.ForceMeshUpdate();
	}

	private static ScrollRect CreateScroll(Transform parent, string name, out RectTransform viewport, out RectTransform content)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.transform.localScale = Vector3.one;
		ScrollRect scrollRect = gameObject.AddComponent<ScrollRect>();
		scrollRect.horizontal = false;
		scrollRect.vertical = true;
		scrollRect.movementType = ScrollRect.MovementType.Clamped;
		scrollRect.scrollSensitivity = 24f;
		gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);
		GameObject gameObject2 = new GameObject("Viewport", typeof(RectTransform));
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		gameObject2.transform.localScale = Vector3.one;
		viewport = (RectTransform)gameObject2.transform;
		gameObject2.AddComponent<RectMask2D>();
		gameObject2.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
		scrollRect.viewport = viewport;
		GameObject gameObject3 = new GameObject("Content", typeof(RectTransform));
		gameObject3.transform.SetParent(gameObject2.transform, worldPositionStays: false);
		gameObject3.transform.localScale = Vector3.one;
		content = (RectTransform)gameObject3.transform;
		scrollRect.content = content;
		return scrollRect;
	}

	private static void PinTopStretch(RectTransform rect)
	{
		rect.anchorMin = new Vector2(0f, 1f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(0.5f, 1f);
		rect.anchoredPosition = Vector2.zero;
		rect.sizeDelta = new Vector2(0f, 0f);
	}

	private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
	{
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.offsetMin = offsetMin;
		rect.offsetMax = offsetMax;
	}
}
