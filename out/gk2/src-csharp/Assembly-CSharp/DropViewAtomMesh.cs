using System.Collections.Generic;
using UnityEngine;

public class DropViewAtomMesh : DropViewAtomBase
{
	[SerializeField]
	private List<DropViewAtomMeshElement> elements = new List<DropViewAtomMeshElement>();

	[SerializeField]
	private DropViewAtomMeshElement defaultElement;

	[SerializeField]
	[Space]
	private DropViewAtomMeshZombieContainer zombieOverheadContainer;

	[SerializeField]
	private DropViewAtomMeshZombieContainer zombieDropContainer;

	[SerializeField]
	[Space]
	private Collider zombieOverheadCollider;

	[SerializeField]
	private Collider zombieDropCollider;

	[SerializeField]
	private List<DropViewAtomMeshElement> zombieWorkerOverheadPrefabs;

	[SerializeField]
	private List<DropViewAtomMeshElement> zombieWorkerDropPrefabs;

	[SerializeField]
	private DropViewAtomMeshElement boxElement;

	[SerializeField]
	private SpriteRenderer boxSpriteRenderer;

	[SerializeField]
	public bool isOverhead;

	private DropViewAtomMeshElement displayedElement;

	private DropViewAtomMeshElement lastDisplayedZombieElement;

	private readonly Dictionary<string, DropViewAtomMeshElement> cachedZombieOverheadElements = new Dictionary<string, DropViewAtomMeshElement>();

	private readonly Dictionary<string, DropViewAtomMeshElement> cachedZombieDropElements = new Dictionary<string, DropViewAtomMeshElement>();

	private Material boxElementMat;

	private static readonly int replaceBlueColor = Shader.PropertyToID("_ReplaceBlueColor");

	public DropViewAtomMeshElement MeshElement => displayedElement;

	public override void Activate(string iconId)
	{
		zombieOverheadContainer.gameObject.SetActive(value: false);
		zombieDropContainer.gameObject.SetActive(value: false);
		DropViewAtomMeshElement dropViewAtomMeshElement = null;
		defaultElement.gameObject.SetActive(value: false);
		boxElement.gameObject.SetActive(value: false);
		foreach (DropViewAtomMeshElement element in elements)
		{
			if (element.name == iconId)
			{
				dropViewAtomMeshElement = element;
			}
			element.gameObject.SetActive(value: false);
		}
		if (dropViewAtomMeshElement == null)
		{
			dropViewAtomMeshElement = defaultElement;
			Debug.LogError("Not found element for item icon id: " + iconId + ", using placeholder");
		}
		if (dropViewAtomMeshElement != null)
		{
			displayedElement = dropViewAtomMeshElement;
			displayedElement.gameObject.SetActive(value: true);
		}
		base.Activate(iconId);
	}

	public override SpriteText GetSpriteText()
	{
		return displayedElement.spriteText;
	}

	public void ActivateZombie(string iconId, Texture2D lutTexture)
	{
		if (isOverhead)
		{
			ActivateZombieOverhead(iconId, lutTexture);
		}
		else
		{
			ActivateZombieDrop(iconId, lutTexture);
		}
	}

	private void ActivateZombieOverhead(string iconId, Texture2D lutTexture)
	{
		ActivateZombie(1, iconId, lutTexture, zombieWorkerOverheadPrefabs);
	}

	private void ActivateZombieDrop(string iconId, Texture2D lutTexture)
	{
		ActivateZombie(2, iconId, lutTexture, zombieWorkerDropPrefabs);
	}

	private void ActivateZombie(int poseId, string iconId, Texture2D lutTexture, List<DropViewAtomMeshElement> sourcePrefabs)
	{
		zombieOverheadContainer.gameObject.SetActive(value: false);
		zombieDropContainer.gameObject.SetActive(value: false);
		DropViewAtomMeshElement dropViewAtomMeshElement = null;
		string text = $"drops_item_zombie_pose{poseId}_t{iconId}";
		foreach (DropViewAtomMeshElement sourcePrefab in sourcePrefabs)
		{
			if (sourcePrefab.name == text)
			{
				dropViewAtomMeshElement = sourcePrefab;
				break;
			}
		}
		if (dropViewAtomMeshElement != null)
		{
			foreach (DropViewAtomMeshElement element in elements)
			{
				element.gameObject.SetActive(value: false);
			}
			boxElement.gameObject.SetActive(value: false);
			defaultElement.gameObject.SetActive(value: false);
			if (lastDisplayedZombieElement != null)
			{
				lastDisplayedZombieElement.gameObject.SetActive(value: false);
			}
			DropViewAtomMeshZombieContainer dropViewAtomMeshZombieContainer = ((poseId == 1) ? zombieOverheadContainer : zombieDropContainer);
			Dictionary<string, DropViewAtomMeshElement> cache = ((poseId == 1) ? cachedZombieOverheadElements : cachedZombieDropElements);
			dropViewAtomMeshZombieContainer.gameObject.SetActive(value: true);
			lastDisplayedZombieElement = GetCachedZombieElement(text, dropViewAtomMeshElement, dropViewAtomMeshZombieContainer, cache);
			lastDisplayedZombieElement.transform.localPosition = Vector3.zero;
			displayedElement = lastDisplayedZombieElement;
			lastDisplayedZombieElement.spriteText = dropViewAtomMeshZombieContainer.spriteText;
			lastDisplayedZombieElement.physicsCollider = ((poseId == 1) ? zombieOverheadCollider : zombieDropCollider);
			displayedElement.gameObject.SetActive(value: true);
			ApplyLutTexture(displayedElement, lutTexture);
		}
		base.Activate(iconId);
	}

	private DropViewAtomMeshElement GetCachedZombieElement(string zombieWorkerPrefabName, DropViewAtomMeshElement prefab, DropViewAtomMeshZombieContainer container, Dictionary<string, DropViewAtomMeshElement> cache)
	{
		if (!cache.TryGetValue(zombieWorkerPrefabName, out var value) || value == null)
		{
			value = Object.Instantiate(prefab, container.container, worldPositionStays: true);
			value.name = zombieWorkerPrefabName;
			cache[zombieWorkerPrefabName] = value;
		}
		return value;
	}

	public override void Deactivate()
	{
		SetInteractionState(isUnderInteraction: false);
		if (displayedElement != null)
		{
			displayedElement.gameObject.SetActive(value: false);
		}
		if (lastDisplayedZombieElement != null)
		{
			lastDisplayedZombieElement.gameObject.SetActive(value: false);
		}
		displayedElement = null;
		lastDisplayedZombieElement = null;
		base.Deactivate();
	}

	public override void SetInteractionState(bool isUnderInteraction)
	{
		if (displayedElement == null || displayedElement.object3D == null)
		{
			return;
		}
		foreach (Object3DMesh object3DMesh in displayedElement.object3D.Object3DMeshes)
		{
			if (isUnderInteraction)
			{
				object3DMesh.ReplaceBlueToColor();
			}
			else
			{
				object3DMesh.ReplaceBlueToTransparent();
			}
		}
	}

	private void ApplyLutTexture(DropViewAtomMeshElement element, Texture2D lutTexture)
	{
		if (element == null)
		{
			return;
		}
		string text = ((lutTexture != null) ? lutTexture.name : "null");
		if (element.object3D == null)
		{
			Debug.LogWarning("DropViewAtomMesh: Can't apply LUT [" + text + "] to [" + element.name + "], object3D is missing");
			return;
		}
		if (element.object3D.Object3DMeshes == null || element.object3D.Object3DMeshes.Count == 0)
		{
			Debug.LogWarning("DropViewAtomMesh: Can't apply LUT [" + text + "] to [" + element.name + "], Object3DMeshes is empty");
			return;
		}
		foreach (Object3DMesh object3DMesh in element.object3D.Object3DMeshes)
		{
			if (object3DMesh == null)
			{
				Debug.LogWarning("DropViewAtomMesh: Can't apply LUT [" + text + "] to [" + element.name + "], Object3DMesh is null");
			}
			else
			{
				object3DMesh.CustomInit();
				object3DMesh.SetLutTexture(lutTexture);
			}
		}
	}
}
