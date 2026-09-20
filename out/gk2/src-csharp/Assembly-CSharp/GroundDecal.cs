using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

public class GroundDecal : MonoBehaviour, IFightDecal
{
	private const float X_SCALE = 1.25f;

	private const float Z_SCALE = 1f;

	public List<GameObject> decalObjs = new List<GameObject>();

	public bool animateY;

	public float maxYOffset = 0.5f;

	public float animDuration = 1f;

	public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public int sortingGroup;

	public float groupYOffset;

	private Vector3 spawnPosition;

	public float TimeToLive { get; private set; }

	public float Lifetime { get; set; }

	internal int CellX { get; private set; }

	internal int CellZ { get; private set; }

	internal int DepthLayer { get; private set; }

	internal DecalLayerBand LayerBand { get; private set; }

	internal GroundDecalsCollection OwnerCollection { get; private set; }

	public int SortingGroup => sortingGroup;

	public float GroupYOffset => groupYOffset;

	internal void SetPlacementMetadata(int cellX, int cellZ, int depthLayer, DecalLayerBand layerBand, GroundDecalsCollection owner)
	{
		CellX = cellX;
		CellZ = cellZ;
		DepthLayer = depthLayer;
		LayerBand = layerBand;
		OwnerCollection = owner;
	}

	internal void ClearPlacementMetadata()
	{
		CellX = 0;
		CellZ = 0;
		DepthLayer = 0;
		LayerBand = DecalLayerBand.Blood;
		OwnerCollection = null;
	}

	public IFightDecal SpawnDecal(Vector3 position, Direction orientation, string customDeathEffectId = "")
	{
		base.transform.position = position;
		spawnPosition = position;
		Rotate(orientation);
		if (!string.IsNullOrEmpty(customDeathEffectId))
		{
			ActivateDecalByName(customDeathEffectId);
		}
		else
		{
			ActivateRandomDecal();
		}
		Lifetime = 0f;
		if (animateY)
		{
			float animProgress = 0f;
			DOTween.To(() => animProgress, delegate(float x)
			{
				animProgress = x;
				base.transform.position = spawnPosition + Vector3.up * (scaleCurve.Evaluate(animProgress) * maxYOffset);
			}, animDuration, animDuration);
		}
		return this;
	}

	private void Rotate(Direction direction)
	{
		if (direction != 0)
		{
			direction.ConvertToVector2XZ();
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction.ConvertToVector3());
			switch (direction)
			{
			case Direction.Up:
			case Direction.Down:
				base.transform.localScale = Vector3.one;
				break;
			case Direction.Right:
			case Direction.Left:
				base.transform.localScale = new Vector3(1.25f, 1f, 1f);
				break;
			}
		}
	}

	private void ActivateDecalByName(string customDeathEffectId)
	{
		if (!string.IsNullOrEmpty(customDeathEffectId))
		{
			GameObject gameObject = decalObjs.Find((GameObject item) => item.name == customDeathEffectId);
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	private void ActivateRandomDecal()
	{
		decalObjs.ForEach(delegate(GameObject item)
		{
			item.SetActive(value: false);
		});
		int index = Random.Range(0, decalObjs.Count);
		decalObjs[index].SetActive(value: true);
	}

	private void Awake()
	{
		TimeToLive = LazySingletonSO<GlobalResources>.Instance.fighting.decalsLifeTime;
	}
}
