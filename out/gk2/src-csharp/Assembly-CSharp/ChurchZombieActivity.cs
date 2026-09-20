using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public class ChurchZombieActivity : MonoBehaviour
{
	private enum ActivityType
	{
		Choir,
		Organ
	}

	private static readonly int startChoirSingingHash = Animator.StringToHash("choir_singing");

	private static readonly int stopChoirSingingHash = Animator.StringToHash("choir_static");

	private static readonly int startOrganPlayingHash = Animator.StringToHash("organ_playing");

	private static readonly int stopOrganPlayingHash = Animator.StringToHash("organ_static");

	[SerializeField]
	private ActivityType activityType;

	[SerializeField]
	private List<AnimationComponent> animComponents;

	[SerializeField]
	private WgoPart wgoPart;

	private void Start()
	{
		wgoPart = GetComponentInParent<WgoPart>();
		if (!(wgoPart == null) && !(wgoPart.Wgo == null))
		{
			wgoPart.Wgo.Data.Inventory.OnItemsAdd += OnItemsChanged;
			wgoPart.Wgo.Data.Inventory.OnItemsRemove += OnItemsChanged;
			OnItemsChanged();
		}
	}

	private void OnEnable()
	{
		if (!(wgoPart == null) && !(wgoPart.Wgo == null))
		{
			OnItemsChanged();
		}
	}

	private void OnDestroy()
	{
		if (!(wgoPart == null) && !(wgoPart.Wgo == null) && wgoPart.Wgo.Data != null)
		{
			wgoPart.Wgo.Data.Inventory.OnItemsAdd -= OnItemsChanged;
			wgoPart.Wgo.Data.Inventory.OnItemsRemove -= OnItemsChanged;
		}
	}

	private void OnItemsChanged(List<Item> items = null)
	{
		List<Item> itemsByGroupId = wgoPart.Wgo.Data.Inventory.GetItemsByGroupId("zombie");
		for (int i = 0; i < animComponents.Count; i++)
		{
			if (itemsByGroupId.Count <= i)
			{
				animComponents[i].gameObject.SetActive(value: false);
				continue;
			}
			SkinPresetGK2 skinPresetGK = null;
			if (itemsByGroupId[i].TryGetProperty<BodyZombieSkinSerializedItemProperty>(out var property))
			{
				switch (property.head)
				{
				case 1052:
				case 1054:
					skinPresetGK = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", 1001, 1052, string.Empty, property.headLut);
					break;
				case 1050:
				case 1056:
					skinPresetGK = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", 1001, 1050, string.Empty, property.headLut);
					break;
				case 1058:
					skinPresetGK = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", 1001, 1058, string.Empty, property.headLut);
					break;
				default:
					skinPresetGK = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", 1001, 1051, string.Empty, property.headLut);
					break;
				}
			}
			else
			{
				skinPresetGK = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", 1001, 1051, string.Empty, "hed_lut_01");
			}
			if (skinPresetGK != null)
			{
				animComponents[i].ChangeSkinPreset(skinPresetGK);
			}
			animComponents[i].gameObject.SetActive(value: true);
			animComponents[i].SetTrigger(GetStopActionHash());
		}
	}

	public void StartAction()
	{
		if (animComponents.IsNullOrEmpty())
		{
			return;
		}
		foreach (AnimationComponent animComponent in animComponents)
		{
			animComponent.Animator.ResetTrigger(GetStopActionHash());
			animComponent.Animator.SetTrigger(GetStartActionHash());
		}
	}

	public void StopAction()
	{
		if (animComponents.IsNullOrEmpty())
		{
			return;
		}
		foreach (AnimationComponent animComponent in animComponents)
		{
			animComponent.Animator.ResetTrigger(GetStartActionHash());
			animComponent.SetTrigger(GetStopActionHash());
		}
	}

	private int GetStartActionHash()
	{
		return activityType switch
		{
			ActivityType.Choir => startChoirSingingHash, 
			ActivityType.Organ => startOrganPlayingHash, 
			_ => 0, 
		};
	}

	private int GetStopActionHash()
	{
		return activityType switch
		{
			ActivityType.Choir => stopChoirSingingHash, 
			ActivityType.Organ => stopOrganPlayingHash, 
			_ => 0, 
		};
	}
}
