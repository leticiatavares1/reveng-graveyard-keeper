using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UICustomizationWindow : LazyWindow<UICustomizationWindowData>
{
	[SerializeField]
	private UIDialogWindowButton applyButton;

	[SerializeField]
	private Camera playerCamera;

	[SerializeField]
	private UICharacterOptionSwitcher hairSwitcher;

	[SerializeField]
	private UICharacterOptionSwitcher beardSwitcher;

	[SerializeField]
	private UICharacterOptionSwitcher bodySwitcher;

	[SerializeField]
	private UICharacterOptionSwitcher hedColorSwitch;

	[SerializeField]
	private UICharacterOptionSwitcher bdy1ColorSwitch;

	[SerializeField]
	private UICharacterOptionSwitcher bdy2ColorSwitch;

	[SerializeField]
	private UICharacterOptionSwitcher bdy3ColorSwitch;

	[SerializeField]
	private Sprite nonInteractableSwitcherImage;

	public PlayerCustomizationData newCustomizationData;

	private List<CustomizablePart> allParts;

	private Dictionary<CustomizablePartType, List<CustomizablePart>> cachedPartsLists = new Dictionary<CustomizablePartType, List<CustomizablePart>>();

	private UIDialogWindowData.ButtonData btnData;

	public event Action<PlayerCustomizationData> OnCustomizationApplied;

	public event Action OnCustomizationCanceled;

	public override void Init()
	{
		base.Init();
		newCustomizationData = new PlayerCustomizationData();
		allParts = Addressables.LoadAssetsAsync<CustomizablePart>("player_skins").WaitForCompletion().ToList();
	}

	public override void Open(UICustomizationWindowData data)
	{
		base.Open(data);
		MainGame.PlayerController.View.CustomizationCharacter.gameObject.SetActive(value: true);
		newCustomizationData = PlayerCustomizationData.Copy(data.CurrentData);
		PlayerSkinHelper.ApplySkin(newCustomizationData, onlyForCustomizationCharacter: true);
		PlayerSkinHelper.ApplyPlayerColorsByData(newCustomizationData, onlyForCustomizationCharacter: true);
		playerCamera.gameObject.SetActive(value: true);
		InitPartSwitchButton(hairSwitcher, new List<CustomizablePartType> { CustomizablePartType.Hair });
		InitPartSwitchButton(beardSwitcher, new List<CustomizablePartType> { CustomizablePartType.Beard });
		InitPartSwitchButton(bodySwitcher, new List<CustomizablePartType>
		{
			CustomizablePartType.Body,
			CustomizablePartType.Arms
		});
		UpdateColorSwitchers();
		PlayerSkinHelper.ApplySkin(newCustomizationData, onlyForCustomizationCharacter: true);
		PlayerSkinHelper.ApplyPlayerColorsByData(newCustomizationData, onlyForCustomizationCharacter: true);
		btnData = new UIDialogWindowData.ButtonData(OnApplyPressed, LLBase.L("ui_apply"), null, replaceForGamepad: true, GameKey.Select);
		applyButton.Draw(btnData);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Close()
	{
		base.Close();
		MainGame.PlayerController.View.CustomizationCharacter.gameObject.SetActive(value: false);
		Hide();
		playerCamera.gameObject.SetActive(value: false);
	}

	private void UpdateColorSwitchers()
	{
		InitColorSwitchButton(hedColorSwitch, PlayerColorCustomizationType.Hed);
		InitColorSwitchButton(bdy1ColorSwitch, PlayerColorCustomizationType.Bdy1);
		InitColorSwitchButton(bdy2ColorSwitch, PlayerColorCustomizationType.Bdy2);
		InitColorSwitchButton(bdy3ColorSwitch, PlayerColorCustomizationType.Bdy3);
	}

	private int GetIndexOfPart(List<CustomizablePart> list, CustomizablePartType type)
	{
		string partId = newCustomizationData.GetCustomizationPartId(type);
		int num = list.FindIndex((CustomizablePart e) => e.name == partId);
		if (num != -1)
		{
			return num;
		}
		return 0;
	}

	private void SetCustomizationPart(PlayerCustomizationPartData newPartData)
	{
		SetCustomizationParts(new PlayerCustomizationPartData[1] { newPartData });
	}

	private void SetCustomizationParts(IEnumerable<PlayerCustomizationPartData> newPartsData)
	{
		foreach (PlayerCustomizationPartData newPartData in newPartsData)
		{
			newCustomizationData.customizationPartsData.RemoveAll((PlayerCustomizationPartData x) => x.type == newPartData.type);
			newCustomizationData.customizationPartsData.Add(newPartData);
			Debug.Log($"#customize# newPartData.id :[{newPartData.id}] newPartData.type:[{newPartData.type}]");
		}
		PlayerSkinHelper.ApplySkin(newCustomizationData, onlyForCustomizationCharacter: true);
		UpdateColorSwitchers();
		PlayerSkinHelper.ApplyPlayerColorsByData(newCustomizationData, onlyForCustomizationCharacter: true);
	}

	public void SetColorPaletteByIndex(int index, PlayerColorCustomizationType type)
	{
		PlayerColorCustomizationElementData playerColorCustomizationElementData = PlayerSkinHelper.CharacterCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type);
		newCustomizationData.SetColorCustomizationIndexForType(playerColorCustomizationElementData.playerColorCustomizationType, index);
		Debug.Log($"#customize# apply index:[{index}] for color type:[{type}]");
		PlayerSkinHelper.ApplyPlayerColors(PlayerSkinHelper.GetColorReplacementPalette(newCustomizationData), PlayerSkinHelper.CharacterCustomizationData.affectedPartTypes, onlyForCustomizationCharacter: true);
	}

	private Texture2D GetTextureForCustomizationPart(int index, PlayerColorCustomizationType type)
	{
		PlayerColorCustomizationElementData playerColorCustomizationElementData = PlayerSkinHelper.CharacterCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type);
		int partSkinIdForColorCustomizationType = newCustomizationData.GetPartSkinIdForColorCustomizationType(type);
		return playerColorCustomizationElementData.GetSkinElement(partSkinIdForColorCustomizationType).palettes[index];
	}

	private void OnApplyPressed()
	{
		data.CurrentData = newCustomizationData;
		this.OnCustomizationApplied?.Invoke(newCustomizationData);
		Close();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		return gameKeyDelegates;
	}

	private void OnBackPressed()
	{
		this.OnCustomizationCanceled?.Invoke();
		Close();
	}

	private void InitColorSwitchButton(UICharacterOptionSwitcher switchButton, PlayerColorCustomizationType type)
	{
		PlayerColorCustomizationElementData playerColorCustomizationElementData = PlayerSkinHelper.CharacterCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type);
		int partSkinIdForColorCustomizationType = newCustomizationData.GetPartSkinIdForColorCustomizationType(type);
		PlayerColorCustomizationSkinElementData skinElementData = playerColorCustomizationElementData?.GetSkinElement(partSkinIdForColorCustomizationType);
		List<int> unlockedIndices = new List<int>(newCustomizationData.GetUnlockedColorIndices(type, partSkinIdForColorCustomizationType, PlayerSkinHelper.CharacterCustomizationData));
		bool flag = skinElementData != null && skinElementData.palettes.Count != 0;
		if (flag)
		{
			unlockedIndices.RemoveAll((int index) => index < 0 || index >= skinElementData.palettes.Count);
		}
		switchButton.IsInteractable = flag && unlockedIndices.Count > 1;
		if (flag && unlockedIndices.Count > 0)
		{
			string[] array = new string[unlockedIndices.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = $"{i + 1}/{array.Length}";
			}
			int colorCustomizationIndexByType = newCustomizationData.GetColorCustomizationIndexByType(type);
			int num = unlockedIndices.IndexOf(colorCustomizationIndexByType);
			if (num < 0)
			{
				num = 0;
				newCustomizationData.SetColorCustomizationIndexForType(type, unlockedIndices[num]);
			}
			switchButton.SetColorImage(GetTextureForCustomizationPart(unlockedIndices[num], type));
			switchButton.Initialize(delegate(int fieldIndex)
			{
				int index2 = unlockedIndices[fieldIndex];
				SetColorPaletteByIndex(index2, type);
				switchButton.SetColorImage(GetTextureForCustomizationPart(index2, type));
			}, array, num, loopNavigation: false);
		}
		else
		{
			switchButton.SetSprite(nonInteractableSwitcherImage);
		}
	}

	private void InitPartSwitchButton(UICharacterOptionSwitcher switchButton, List<CustomizablePartType> types)
	{
		List<CustomizablePart> unlockedCustomizablePartsForType = GetUnlockedCustomizablePartsForType(types[0]);
		switchButton.IsInteractable = unlockedCustomizablePartsForType.Count > 1;
		if (unlockedCustomizablePartsForType.Count == 0)
		{
			switchButton.SetSprite(nonInteractableSwitcherImage);
			return;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < unlockedCustomizablePartsForType.Count; i++)
		{
			list.Add($"{i + 1}/{unlockedCustomizablePartsForType.Count}");
		}
		switchButton.Initialize(delegate(int s)
		{
			CustomizablePartType customizablePartType = types[0];
			string primaryId = GetUnlockedCustomizablePartsForType(customizablePartType)[s].name;
			List<PlayerCustomizationPartData> list2 = new List<PlayerCustomizationPartData>();
			foreach (CustomizablePartType type in types)
			{
				list2.Add(new PlayerCustomizationPartData
				{
					id = GetPairedCustomizationPartId(primaryId, customizablePartType, type),
					type = type
				});
			}
			SetCustomizationParts(list2);
		}, list.ToArray(), GetIndexOfPart(unlockedCustomizablePartsForType, types[0]), loopNavigation: false);
	}

	private static string GetPairedCustomizationPartId(string primaryId, CustomizablePartType primaryType, CustomizablePartType targetType)
	{
		if (primaryType == CustomizablePartType.Body && targetType == CustomizablePartType.Arms)
		{
			return primaryId.Replace("bdy_", "arm_");
		}
		return primaryId;
	}

	private List<CustomizablePart> GetUnlockedCustomizablePartsForType(CustomizablePartType type)
	{
		List<string> unlockedIds = newCustomizationData.GetUnlockedPartIds(type);
		return (from p in GetAllCustomizablePartsForType(type).FindAll((CustomizablePart p) => unlockedIds.Contains(p.name))
			orderby p.orderIndex
			select p).ToList();
	}

	private List<CustomizablePart> GetAllCustomizablePartsForType(CustomizablePartType type)
	{
		if (!cachedPartsLists.TryGetValue(type, out var value))
		{
			value = allParts.FindAll((CustomizablePart p) => p.type == type);
			cachedPartsLists.Add(type, value);
		}
		return value;
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Print(new LazyGameKeyTip(GameKey.PrevSubTab, "tip_prev"), new LazyGameKeyTip(GameKey.NextSubTab, "tip_next"), LazyGameKeyTip.Back());
	}

	protected override void TestDraw()
	{
		UICustomizationWindowData uICustomizationWindowData = new UICustomizationWindowData();
		uICustomizationWindowData.CurrentData = PlayerCustomizationData.Copy(PlayerSkinHelper.playerStandardCustomizationData);
		Open(uICustomizationWindowData);
	}
}
