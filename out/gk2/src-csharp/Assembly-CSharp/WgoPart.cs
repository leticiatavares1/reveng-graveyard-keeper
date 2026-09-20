using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using LazyBearTechnology;
using LinqTools;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ProBuilder;

public class WgoPart : MonoBehaviour
{
	private const float START_PHYSICS_COL_RADIUS = 0.001f;

	[SerializeField]
	private Transform bubblePoint;

	private Wgo wgo;

	private Object3D object3D;

	[SerializeField]
	private List<Object3D> cachedObjects3D;

	private Transform customBubblePoint;

	[SerializeField]
	private List<WgoPartState> variations = new List<WgoPartState>();

	[SerializeField]
	private List<int> customRotationSequence = new List<int>();

	[SerializeField]
	private SpawnConfiguration spawnConfiguration;

	[SerializeField]
	private List<Collider> interactableColliders = new List<Collider>();

	[SerializeField]
	private List<DockPoint> dockPoints = new List<DockPoint>();

	[SerializeField]
	private CapsuleCollider capsulePhysicsCollider;

	[SerializeField]
	private DropViewAtomMesh dropViewBig;

	[SerializeField]
	private DropViewAtomSprite dropViewSmall;

	[SerializeField]
	[CanBeNull]
	private ReservoirView reservoirView;

	[SerializeField]
	[CanBeNull]
	private RiverBodyReceiver riverBodyReceiver;

	[SerializeField]
	[CanBeNull]
	private List<FlagPlacementPoint> flagPlacementPoints;

	private ConditionalDrawer conditionalDrawer;

	private bool dropViewUnderInteraction;

	private GameObject assetReference;

	[CanBeNull]
	private AnimationComponentBase animationComponent;

	[SerializeField]
	[Space]
	[CanBeNull]
	private TextMeshPro label;

	private bool animationComponentInitialized;

	private Tween physicsCollAnimTween;

	public IReadOnlyList<DockPoint> DockPoints => dockPoints;

	public List<WgoPartState> Variations => variations;

	public WgoPartData WgoPartData { get; set; }

	public string PooledAddressableKey { get; set; }

	public Transform BubblePoint
	{
		get
		{
			if (customBubblePoint != null)
			{
				return customBubblePoint;
			}
			if (bubblePoint != null)
			{
				return bubblePoint;
			}
			return null;
		}
	}

	public Wgo Wgo => wgo;

	public string Id
	{
		get
		{
			if (WgoPartData == null || string.IsNullOrEmpty(WgoPartData.id))
			{
				return base.name;
			}
			return WgoPartData.id;
		}
	}

	public Object3D Object3D => object3D;

	public AnimationComponentBase AnimationComponent => animationComponent;

	public List<Collider> InteractableColliders => interactableColliders;

	public WgoPartState CurrentWgoPartState
	{
		get
		{
			if (WgoPartData == null)
			{
				return null;
			}
			return GetWgoPartState(WgoPartData.variationId, WgoPartData.rotationIndex);
		}
	}

	public Transform CustomBubblePoint => customBubblePoint;

	public SpawnConfiguration SpawnConfiguration
	{
		get
		{
			return spawnConfiguration;
		}
		set
		{
			spawnConfiguration = value;
		}
	}

	public bool EnsureAnimationComponentInitialized()
	{
		if (animationComponentInitialized)
		{
			return this.animationComponent != null;
		}
		this.animationComponent = GetComponentInChildren<AnimationComponentBase>();
		if (this.animationComponent == null)
		{
			return false;
		}
		string text = ((wgo.Data.Definition == null) ? string.Empty : (wgo.Data.Definition.hasCustomVisualId ? wgo.Data.Definition.customVisualId : wgo.Data.Definition.id));
		if (wgo.Data is ZombieWgoData zombieWgoData)
		{
			SetupZombieSkin(zombieWgoData, text);
		}
		else if (wgo.Data.Definition != null && !string.IsNullOrEmpty(wgo.Data.Definition.zombieRollDataId))
		{
			this.animationComponent.SetSkinPreset(ZombieSkinHelper.GetPresetForWgoData(wgo.Data, wgo.Data.Definition.zombieRollDataId));
			this.animationComponent.InitWithSkinOrApplyCurrentSkin(text);
		}
		else
		{
			this.animationComponent.InitWithSkinOrApplySkin(text);
		}
		if (this.animationComponent is AnimationComponent { AutoInitOnAwake: not false } animationComponent)
		{
			this.animationComponent.SetDirection(animationComponent.Direction);
		}
		else
		{
			this.animationComponent.SetDirection(wgo.Data.direction.Value);
		}
		wgo.Data.OnDirectionChanged += HandleDirectionChanged;
		animationComponentInitialized = true;
		return true;
	}

	public DockPointData GetDockPointData(DockPoint dockPoint)
	{
		int num = dockPoints.IndexOf(dockPoint);
		if (num == -1)
		{
			return null;
		}
		return WgoPartData.GetDockPointByIndex(num);
	}

	public void InitVisuals(GameObject assetReference, WgoPartData wgoPartData, WGODef definition)
	{
		this.assetReference = assetReference;
		WgoPartData = wgoPartData;
		UpdateAvailableVariationsData();
		EnsureAnimationComponentInitialized();
		if (BubblePoint != null)
		{
			Wgo.Data.SetBubblePointOffset(BubblePoint.localPosition);
		}
		SubscribeToDataChanges();
		if (label != null)
		{
			label.gameObject.SetActive(!string.IsNullOrEmpty(definition.devLabelStr));
			label.text = definition.devLabelStr;
		}
		UpdateDropViewFromInventory();
		InitDockPoints();
	}

	public void CleanupChunkableComponents()
	{
		ChunkableObjectComponent[] componentsInChildren = GetComponentsInChildren<ChunkableObjectComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		BakedChunkableObjectComponent[] componentsInChildren2 = GetComponentsInChildren<BakedChunkableObjectComponent>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Object.Destroy(componentsInChildren2[j]);
		}
	}

	public void DeInitForPool()
	{
		if (WgoPartData != null)
		{
			UnsubscribeFromDataChanges();
		}
		if (flagPlacementPoints != null)
		{
			foreach (FlagPlacementPoint flagPlacementPoint in flagPlacementPoints)
			{
				flagPlacementPoint.DeInit();
			}
		}
		if (Object3D?.Object3DMeshes != null)
		{
			for (int i = 0; i < Object3D.Object3DMeshes.Count; i++)
			{
				Object3DMesh object3DMesh = Object3D.Object3DMeshes[i];
				if (!(object3DMesh == null))
				{
					object3DMesh.ResetAnimValues();
				}
			}
		}
		DecoyComponent componentInChildren = GetComponentInChildren<DecoyComponent>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			Object.Destroy(componentInChildren);
		}
		riverBodyReceiver?.DeInit();
		animationComponent?.ResetForPool();
		assetReference = null;
		WgoPartData = null;
		wgo = null;
		animationComponent = null;
		animationComponentInitialized = false;
	}

	public void ReInitFromPool(Wgo newWgo)
	{
		wgo = newWgo;
		bool activeSelf = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		Init(newWgo);
		if (activeSelf)
		{
			if ((object)conditionalDrawer == null)
			{
				conditionalDrawer = GetComponent<ConditionalDrawer>();
			}
			conditionalDrawer?.Init(this);
		}
		if (reservoirView != null)
		{
			reservoirView.Init(wgo);
		}
		if ((object)riverBodyReceiver == null)
		{
			riverBodyReceiver = GetComponentInChildren<RiverBodyReceiver>(includeInactive: true);
		}
		if (riverBodyReceiver != null)
		{
			riverBodyReceiver.Init(wgo);
		}
		if (flagPlacementPoints == null)
		{
			return;
		}
		foreach (FlagPlacementPoint flagPlacementPoint in flagPlacementPoints)
		{
			flagPlacementPoint.Init(wgo?.Data);
		}
	}

	public void SetCustomBubblePoint(Transform bubblePoint)
	{
		customBubblePoint = bubblePoint;
	}

	public void SetSelectionTint(Color color, float amount)
	{
		if (cachedObjects3D != null)
		{
			for (int i = 0; i < cachedObjects3D.Count; i++)
			{
				cachedObjects3D[i]?.SetSelectionTint(color, amount);
			}
		}
	}

	public void SetDropViewInteractionState(bool isUnderInteraction)
	{
		dropViewUnderInteraction = isUnderInteraction;
		ApplyDropViewInteractionState();
	}

	private void Init(Wgo wgo)
	{
		this.wgo = wgo;
		object3D = GetComponentInChildren<Object3D>();
		cachedObjects3D = GetComponentsInChildren<Object3D>(includeInactive: true).ToList();
	}

	private void Start()
	{
		UpdateColliders();
	}

	private void OnEnable()
	{
		if ((object)conditionalDrawer == null)
		{
			conditionalDrawer = GetComponent<ConditionalDrawer>();
		}
		if ((bool)conditionalDrawer)
		{
			conditionalDrawer.Init(this);
		}
		if ((object)reservoirView == null)
		{
			reservoirView = GetComponent<ReservoirView>();
		}
		if ((bool)reservoirView)
		{
			reservoirView.Init(wgo);
		}
		if ((object)riverBodyReceiver == null)
		{
			riverBodyReceiver = GetComponentInChildren<RiverBodyReceiver>(includeInactive: true);
		}
		if ((bool)riverBodyReceiver)
		{
			riverBodyReceiver.Init(wgo);
		}
	}

	private void OnDisable()
	{
		if (conditionalDrawer != null)
		{
			conditionalDrawer.DeInit();
		}
	}

	public void KillPhysicsCollAnimTween()
	{
		if (physicsCollAnimTween != null)
		{
			physicsCollAnimTween.Kill();
			physicsCollAnimTween = null;
		}
	}

	private void OnDestroy()
	{
		KillPhysicsCollAnimTween();
		if (WgoPartData != null)
		{
			UnsubscribeFromDataChanges();
		}
		if (assetReference != null)
		{
			Addressables.Release(assetReference);
			assetReference = null;
		}
	}

	private void UpdateColliders()
	{
		List<Collider> cols = new List<Collider>();
		cachedObjects3D.ForEach(delegate(Object3D obj)
		{
			cols.AddRange(obj.CachedColliders);
		});
		foreach (Collider item in cols)
		{
			if (item == null)
			{
				Debug.LogError("Null cached collider on WgoPart:[" + Id + "]");
			}
			else
			{
				if (!item.IsLossyScaleNegative())
				{
					continue;
				}
				if (item is BoxCollider boxCol)
				{
					boxCol.FixBoxColliderLossyScale();
				}
				if (item is MeshCollider meshCollider && (bool)meshCollider.GetComponent<ProBuilderMesh>())
				{
					MirrorMeshCollider component = meshCollider.GetComponent<MirrorMeshCollider>();
					if (component == null)
					{
						meshCollider.gameObject.AddComponent<MirrorMeshCollider>();
					}
					else
					{
						component.MirrorCollider();
					}
				}
			}
		}
	}

	private void SubscribeToDataChanges()
	{
		WgoPartData.OnStateChange += ApplyWgoPartState;
	}

	private void UnsubscribeFromDataChanges()
	{
		WgoPartData.OnStateChange -= ApplyWgoPartState;
		wgo.Data.OnDirectionChanged -= HandleDirectionChanged;
	}

	private void InitDockPoints()
	{
		foreach (DockPoint dockPoint in DockPoints)
		{
			dockPoint.Init(this);
		}
	}

	public bool CanBeRotated(string variationId = "", int rotationIndex = -1)
	{
		string text = (string.IsNullOrEmpty(variationId) ? WgoPartData.variationId : variationId);
		int num = ((rotationIndex == -1) ? WgoPartData.rotationIndex : rotationIndex);
		foreach (WgoPartState variation in variations)
		{
			if (variation.variationId == text && variation.rotationIndex != num)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryApplyCustomRotationSequenceStart()
	{
		if (customRotationSequence == null || customRotationSequence.Count == 0)
		{
			return false;
		}
		string text = ((WgoPartData != null) ? WgoPartData.variationId : null);
		if (string.IsNullOrEmpty(text))
		{
			text = GetDefaultVariationId();
		}
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		WgoPartState wgoPartState = GetWgoPartState(text, customRotationSequence[0]);
		if (wgoPartState == null)
		{
			return false;
		}
		ApplyWgoPartState(wgoPartState);
		return true;
	}

	public bool Rotate(bool inReverse = false)
	{
		if (customRotationSequence != null && customRotationSequence.Count > 0)
		{
			int num = customRotationSequence.IndexOf(WgoPartData.rotationIndex);
			if (num == -1)
			{
				return false;
			}
			int index = MathUtilities.ClampCycle(inReverse ? (num - 1) : (num + 1), 0, customRotationSequence.Count - 1);
			WgoPartState wgoPartState = GetWgoPartState(WgoPartData.variationId, customRotationSequence[index]);
			if (wgoPartState == null)
			{
				return false;
			}
			ApplyWgoPartState(wgoPartState);
			return true;
		}
		List<WgoPartState> list = new List<WgoPartState>();
		foreach (WgoPartState variation in variations)
		{
			if (variation.variationId == WgoPartData.variationId)
			{
				list.Add(variation);
			}
		}
		int num2 = WgoPartData.rotationIndex;
		int num3 = num2;
		do
		{
			num2 = MathUtilities.ClampCycle((!inReverse) ? (++num2) : (--num2), 0, 3);
			foreach (WgoPartState item in list)
			{
				if (item.rotationIndex == num2)
				{
					WgoPartData.rotationIndex = num2;
					ApplyWgoPartState(item);
					return true;
				}
			}
		}
		while (num2 != num3);
		return false;
	}

	public void ApplyWgoPartState()
	{
		if (variations.Count == 0)
		{
			return;
		}
		if (string.IsNullOrEmpty(WgoPartData.variationId) && WgoPartData.rotationIndex == -1)
		{
			ApplyDefaultWgoPartState();
			return;
		}
		foreach (WgoPartState variation in variations)
		{
			if (string.IsNullOrEmpty(WgoPartData.variationId) && variation.rotationIndex == WgoPartData.rotationIndex)
			{
				ApplyWgoPartState(variation);
				return;
			}
			if (WgoPartData.rotationIndex == -1 && variation.variationId == WgoPartData.variationId)
			{
				ApplyWgoPartState(variation);
				return;
			}
			if (variation.variationId == WgoPartData.variationId && variation.rotationIndex == WgoPartData.rotationIndex)
			{
				ApplyWgoPartState(variation);
				return;
			}
		}
		Debug.LogError("Cannot apply WgoPartState for WgoPartData: " + WgoPartData.ToString());
		ApplyWgoPartState(variations[0]);
	}

	public void ApplyDefaultWgoPartState()
	{
		if (variations.Count == 0)
		{
			return;
		}
		bool flag = false;
		foreach (WgoPartState variation in variations)
		{
			if (variation.isDefault)
			{
				ApplyWgoPartState(variation);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogError("WgoPart [" + base.gameObject.name + "] initialized without default state.");
			ApplyWgoPartState(variations[0]);
		}
	}

	private string GetDefaultVariationId()
	{
		foreach (WgoPartState variation in variations)
		{
			if (variation.isDefault)
			{
				return variation.variationId;
			}
		}
		if (variations.Count <= 0)
		{
			return string.Empty;
		}
		return variations[0].variationId;
	}

	public WgoPartState GetWgoPartState(string variationId, int rotationIndex = -1)
	{
		List<WgoPartState> list = variations;
		if (list != null && list.Count > 0)
		{
			foreach (WgoPartState variation in Variations)
			{
				if (variation.variationId == variationId && (rotationIndex == -1 || variation.rotationIndex == rotationIndex))
				{
					return variation;
				}
			}
			Debug.LogError(string.Format("[{0}]: Cannot find state for wgoPart [{1}] with [{2}-{3}]", "WgoPart", Id, variationId, rotationIndex), this);
		}
		return null;
	}

	public void ApplyWgoPartState(string id, int rotationIndex)
	{
		WgoPartState wgoPartState = GetWgoPartState(id, rotationIndex);
		if (wgoPartState != null)
		{
			ApplyWgoPartState(wgoPartState);
		}
	}

	public void TryAnimatePhysicsCollider()
	{
		if (!(capsulePhysicsCollider != null))
		{
			return;
		}
		float startRadius = 0.001f;
		float radius = capsulePhysicsCollider.radius;
		capsulePhysicsCollider.radius = startRadius;
		physicsCollAnimTween = DOTween.To(() => startRadius, delegate(float x)
		{
			startRadius = x;
			if (capsulePhysicsCollider != null)
			{
				capsulePhysicsCollider.radius = x;
			}
		}, radius, 0.15f);
	}

	private void ApplyWgoPartState(WgoPartState wgoPartState)
	{
		foreach (WgoPartState variation in variations)
		{
			if (!(variation.gameObject == null))
			{
				variation.gameObject.SetActive(value: false);
			}
		}
		WgoPartData.variationId = wgoPartState.variationId;
		WgoPartData.rotationIndex = wgoPartState.rotationIndex;
		if (wgoPartState.gameObject != null)
		{
			wgoPartState.gameObject.SetActive(value: true);
			Vector3 localScale = wgoPartState.gameObject.transform.localScale;
			wgoPartState.gameObject.transform.localScale = new Vector3(Mathf.Abs(localScale.x) * (float)((!wgoPartState.mirror) ? 1 : (-1)), localScale.y, localScale.z);
			object3D = wgoPartState.gameObject.GetComponentInChildren<Object3D>(includeInactive: true);
		}
		customBubblePoint = wgoPartState.customBubblePoint;
		foreach (UnityEvent customEvent in wgoPartState.customEvents)
		{
			customEvent?.Invoke();
		}
		UpdateColliders();
	}

	private void UpdateAvailableVariationsData()
	{
		WgoPartData.AvailableVariations = new List<WgoPartStateData>();
		foreach (WgoPartState variation in variations)
		{
			WgoPartStateData item = new WgoPartStateData(variation.variationId, variation.rotationIndex, variation.mirror);
			WgoPartData.AvailableVariations.Add(item);
		}
	}

	private void HandleDirectionChanged(Vector2 direction)
	{
		if (animationComponent != null)
		{
			animationComponent.SetDirection(direction);
		}
	}

	private void SetupZombieSkin(ZombieWgoData zombieWgoData, string visualId = "")
	{
		if (animationComponent == null)
		{
			return;
		}
		int? num = null;
		switch (zombieWgoData.ZombieType)
		{
		case ZombieType.Crafter:
		{
			WgoData attachedWgoData = zombieWgoData.AttachedWgoData;
			if (attachedWgoData != null && attachedWgoData.Definition.interactionType == WGODef.InteractionType.ZombieMine)
			{
				num = 1700;
			}
			break;
		}
		case ZombieType.Gardener:
			num = 1800;
			break;
		}
		if (num.HasValue)
		{
			int gameResInt = zombieWgoData.GetGameResInt("zombie_head_id");
			int gameResInt2 = zombieWgoData.GetGameResInt("zombie_body_id");
			string headLut = zombieWgoData.GameResStr.Get("zombie_head_lut");
			int num2;
			switch (gameResInt)
			{
			case 1050:
			case 1056:
				num2 = 1;
				break;
			case 1052:
			case 1054:
				num2 = 2;
				break;
			case 1058:
				num2 = 3;
				break;
			default:
				num2 = 0;
				break;
			}
			int num3 = num2;
			SkinPresetGK2 presetForCustomizationData = ZombieSkinHelper.GetPresetForCustomizationData("zombie_worker", gameResInt2, num.Value + num3, string.Empty, headLut);
			animationComponent.SetSkinPreset(presetForCustomizationData);
			animationComponent.InitWithSkinOrApplyCurrentSkin(visualId);
		}
		else
		{
			animationComponent.SetSkinPreset(ZombieSkinHelper.GetPresetForWgoData(zombieWgoData, "zombie_worker"));
			animationComponent.InitWithSkinOrApplyCurrentSkin(visualId);
		}
	}

	public void UpdateDropViewFromInventory(List<Item> customInventory = null)
	{
		if (customInventory == null && wgo != null && wgo.Data != null)
		{
			if (wgo.Data.Inventory.Data.Inventory.Count == 0)
			{
				UpdateDropViewGroup(null);
			}
			else
			{
				UpdateDropViewGroup(wgo.Data.Inventory.Data.Inventory[0].Definition);
			}
		}
		else if (customInventory.Count == 0)
		{
			UpdateDropViewGroup(null);
		}
		else
		{
			UpdateDropViewGroup(customInventory[0].Definition);
		}
	}

	public void UpdateDropViewFromItemData(ConveyorMovableItemData itemData)
	{
		if (itemData == null || string.IsNullOrEmpty(itemData.itemId))
		{
			UpdateDropViewGroup(null);
		}
		else
		{
			UpdateDropViewGroup(GameBalance.Me.GetData<ItemDef>(itemData.itemId));
		}
	}

	private void UpdateDropView(ItemDef itemDef, DropViewAtomMesh dropBig, DropViewAtomSprite dropSmall)
	{
		if (dropBig == null || dropSmall == null)
		{
			return;
		}
		if (itemDef == null)
		{
			dropBig.Deactivate();
			dropSmall.Deactivate();
			return;
		}
		switch (itemDef.itemSize)
		{
		case ItemSize.Small:
			if (!dropSmall.gameObject.activeSelf || string.IsNullOrEmpty(dropSmall.IconId) || !(dropSmall.IconId == itemDef.iconId))
			{
				dropSmall.Activate(itemDef.iconId);
				dropBig.Deactivate();
			}
			break;
		case ItemSize.Big:
			if (!dropBig.gameObject.activeSelf || string.IsNullOrEmpty(dropBig.IconId) || !(dropBig.IconId == itemDef.iconId))
			{
				dropBig.Activate(itemDef.iconId);
				dropSmall.Deactivate();
			}
			break;
		}
	}

	private void UpdateDropViewGroup(ItemDef itemDef)
	{
		UpdateDropView(itemDef, dropViewBig, dropViewSmall);
		ApplyDropViewInteractionState();
	}

	private void ApplyDropViewInteractionState()
	{
		dropViewBig?.SetInteractionState(dropViewUnderInteraction);
		dropViewSmall?.SetInteractionState(dropViewUnderInteraction);
	}
}
