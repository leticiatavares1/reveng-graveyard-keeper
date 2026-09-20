using System;
using System.Globalization;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISaveSlot : MonoBehaviour
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private GamepadNavigationItem navigationItem;

	[SerializeField]
	private LazyButton deleteButton;

	[SerializeField]
	private LazyButton importButton;

	[SerializeField]
	private GameObject resourcesParent;

	[SerializeField]
	private Image slotBack;

	[SerializeField]
	private Image slotBackLocked;

	[SerializeField]
	private TextMeshProUGUI dayLabel;

	[SerializeField]
	private TextMeshProUGUI saveDateTimeLabel;

	[SerializeField]
	private TextMeshProUGUI saveNameLabel;

	[SerializeField]
	private TextMeshProUGUI sourceLabel;

	[SerializeField]
	private TextMeshProUGUI[] resourcesLabels;

	[SerializeField]
	private Image[] resourcesIcons;

	[SerializeField]
	private GameObject newSaveText;

	[SerializeField]
	private GameObject selector;

	[SerializeField]
	private TextStyle timePart1Style;

	[SerializeField]
	private TextStyle timePart1StyleInactive;

	[SerializeField]
	private TextStyle timePart2Style;

	[SerializeField]
	private TextStyle timePart2StyleInactive;

	[SerializeField]
	private TextStyle daysPart1Style;

	[SerializeField]
	private TextStyle daysPart2Style;

	[SerializeField]
	private TextStyle daysStyleInactive;

	[SerializeField]
	private TextStyle resourcesStyle;

	[SerializeField]
	private TextStyle resourcesStyleInactive;

	[SerializeField]
	private TextStyleComponent timeStyleComponent;

	[SerializeField]
	private TextStyleComponent dayStyleComponent;

	[SerializeField]
	private TextStyleComponent[] resourcesStyleComponents;

	[SerializeField]
	private Color resourceIconActiveColor = Color.white;

	[SerializeField]
	private Color resourceIconInactiveColor = new Color(1f, 1f, 1f, 0.7f);

	private SaveSlotData linkedSaveSlot;

	private bool canDeleteCurrentSlot = true;

	private bool canImportCurrentSlot;

	public SaveSlotData LinkedSaveSlot => linkedSaveSlot;

	public static event Action<SaveSlotData> OnSaveSlotSelected;

	public static event Action<UISaveSlot, SaveSlotData> OnSaveSlotSelectedWithSlot;

	public static event Action<UISaveSlot, SaveSlotData> OnSaveSlotDelete;

	public static event Action<UISaveSlot> OnSaveSlotImport;

	public static event Action<UISaveSlot> OnSaveSlotEntered;

	private static void SaveImportLog(string message)
	{
	}

	private void Awake()
	{
		button.onClick.AddListener(OnClicked);
		button.onEnter.AddListener(OnEnter);
		button.onExit.AddListener(OnExit);
		deleteButton.onClick.AddListener(OnDelete);
		if (importButton != null)
		{
			importButton.onClick.AddListener(OnImport);
		}
	}

	private void Update()
	{
		if (navigationItem.IsFocused)
		{
			if (LazyInput.GetKeyDown(GameKey.SaveDelete))
			{
				OnDelete();
			}
			if (LazyInput.GetKeyDown(GameKey.SaveImport))
			{
				OnImport();
			}
		}
	}

	public bool CanDeleteSaveSlot()
	{
		if (canDeleteCurrentSlot && linkedSaveSlot != null && deleteButton.gameObject.activeSelf)
		{
			return deleteButton.interactable;
		}
		return false;
	}

	public bool CanImportSaveSlot()
	{
		bool flag = canImportCurrentSlot && importButton != null && importButton.gameObject.activeSelf && importButton.interactable;
		SaveImportLog($"CanImportSaveSlot result:[{flag}] slotName:[{linkedSaveSlot?.slotName}] canImportCurrentSlot:[{canImportCurrentSlot}] importButtonNull:[{importButton == null}] importButtonActive:[{importButton != null && importButton.gameObject.activeSelf}] importButtonInteractable:[{importButton != null && importButton.interactable}]");
		return flag;
	}

	public void Show(SaveSlotData saveSlotDataToDisplay, bool canDelete = true, bool canImport = false)
	{
		linkedSaveSlot = saveSlotDataToDisplay;
		canDeleteCurrentSlot = canDelete;
		canImportCurrentSlot = canImport;
		SaveImportLog($"Show slotName:[{saveSlotDataToDisplay?.slotName}] platform:[{saveSlotDataToDisplay?.platform}] isDemoSave:[{saveSlotDataToDisplay?.isDemoSave}] canDelete:[{canDelete}] canImport:[{canImport}] importButtonNull:[{importButton == null}]");
		saveNameLabel.gameObject.SetActive(value: false);
		SetSourceLabel(string.Empty);
		SetImportButtonState(canImportCurrentSlot);
		if (saveSlotDataToDisplay == null)
		{
			deleteButton.gameObject.SetActive(value: false);
			button.interactable = true;
			slotBack.gameObject.SetActive(value: true);
			slotBackLocked.gameObject.SetActive(value: false);
			resourcesParent.SetActive(value: false);
			newSaveText.SetActive(value: true);
			dayLabel.gameObject.SetActive(value: false);
			saveDateTimeLabel.gameObject.SetActive(value: false);
		}
		else
		{
			button.interactable = SaveSystem.CanLoadSaveSlot(saveSlotDataToDisplay);
			newSaveText.SetActive(value: false);
			saveDateTimeLabel.gameObject.SetActive(value: true);
			dayLabel.gameObject.SetActive(value: true);
			resourcesLabels[0].text = $"{saveSlotDataToDisplay.graveyardQuality}";
			resourcesLabels[1].text = $"{saveSlotDataToDisplay.churchQuality}";
			resourcesLabels[2].text = $"{saveSlotDataToDisplay.villageRep}";
			slotBack.gameObject.SetActive(button.interactable);
			slotBackLocked.gameObject.SetActive(!button.interactable);
			resourcesParent.SetActive(value: true);
			if (button.interactable)
			{
				deleteButton.gameObject.SetActive(canDeleteCurrentSlot);
				TextStyleComponent[] array = resourcesStyleComponents;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetTextStyle(resourcesStyle);
				}
				Image[] array2 = resourcesIcons;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].color = resourceIconActiveColor;
				}
				dayStyleComponent.SetTextStyle(daysPart1Style);
				dayLabel.text = LLBase.L("save_slot_descr", daysPart2Style.ApplyStyleToString(linkedSaveSlot.day.ToString()));
				timeStyleComponent.SetTextStyle(timePart1Style);
				saveDateTimeLabel.text = GetStyledSaveDateTime(linkedSaveSlot.GetSaveDateTime(), timePart2Style) ?? "";
			}
			else
			{
				deleteButton.gameObject.SetActive(value: false);
				TextStyleComponent[] array = resourcesStyleComponents;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetTextStyle(resourcesStyleInactive);
				}
				Image[] array2 = resourcesIcons;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].color = resourceIconInactiveColor;
				}
				dayStyleComponent.SetTextStyle(daysStyleInactive);
				dayLabel.text = LLBase.L("save_slot_descr", linkedSaveSlot.day.ToString());
				timeStyleComponent.SetTextStyle(timePart1StyleInactive);
				saveDateTimeLabel.text = GetStyledSaveDateTime(linkedSaveSlot.GetSaveDateTime(), timePart2StyleInactive) ?? "";
			}
		}
		base.gameObject.SetActive(value: true);
	}

	public void SetSourceLabel(string text)
	{
		if (!(sourceLabel == null))
		{
			bool flag = !string.IsNullOrEmpty(text);
			sourceLabel.gameObject.SetActive(flag);
			if (flag)
			{
				sourceLabel.text = text;
			}
		}
	}

	private string GetStyledSaveDateTime(DateTime saveDateTime, TextStyle textStyle)
	{
		string text = saveDateTime.ToString(CultureInfo.CurrentCulture);
		string text2 = saveDateTime.ToString("d", CultureInfo.CurrentCulture);
		int num = text.IndexOf(text2, StringComparison.CurrentCulture);
		if (num < 0)
		{
			return text;
		}
		string text3 = text.Substring(0, num);
		string text4 = text.Substring(num + text2.Length);
		return text3 + textStyle.ApplyStyleToString(text2) + text4;
	}

	private void OnEnter()
	{
		selector.gameObject.SetActive(value: true);
		UISaveSlot.OnSaveSlotEntered?.Invoke(this);
	}

	private void OnExit()
	{
		selector.gameObject.SetActive(value: false);
	}

	private void OnClicked()
	{
		selector.gameObject.SetActive(value: false);
		UISaveSlot.OnSaveSlotSelectedWithSlot?.Invoke(this, linkedSaveSlot);
		UISaveSlot.OnSaveSlotSelected?.Invoke(linkedSaveSlot);
	}

	private void OnDelete()
	{
		if (CanDeleteSaveSlot())
		{
			selector.gameObject.SetActive(value: false);
			UISaveSlot.OnSaveSlotDelete?.Invoke(this, linkedSaveSlot);
		}
	}

	private void OnImport()
	{
		if (!CanImportSaveSlot())
		{
			SaveImportLog("OnImport ignored slotName:[" + linkedSaveSlot?.slotName + "]");
			return;
		}
		SaveImportLog("OnImport accepted slotName:[" + linkedSaveSlot?.slotName + "]");
		selector.gameObject.SetActive(value: false);
		UISaveSlot.OnSaveSlotImport?.Invoke(this);
	}

	private void SetImportButtonState(bool isActive)
	{
		if (importButton != null)
		{
			importButton.gameObject.SetActive(isActive);
			SaveImportLog($"SetImportButtonState isActive:[{isActive}] slotName:[{linkedSaveSlot?.slotName}]");
		}
		else
		{
			SaveImportLog($"SetImportButtonState skipped because importButton is null. Requested isActive:[{isActive}] slotName:[{linkedSaveSlot?.slotName}]");
		}
	}

	private void OnDisable()
	{
		selector.gameObject.SetActive(value: false);
	}
}
