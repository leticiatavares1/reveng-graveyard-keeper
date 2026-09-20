using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class GroundDecalsCollection : MonoBehaviour
{
	public GroundDecal decalPrefab;

	public Transform decalsParent;

	[SerializeField]
	private DecalLayerBand layerBand;

	private Pool decalPool;

	private List<GroundDecal> activeDecals = new List<GroundDecal>();

	[HideInInspector]
	public SortedDecals sortedDecals = new SortedDecals();

	internal Pool DecalPool => decalPool;

	public DecalLayerBand LayerBand => layerBand;

	public void SetLayerBand(DecalLayerBand band)
	{
		layerBand = band;
	}

	public void SpawnDecal(Vector3 position, Direction orientation, string customDeathEffectId = "")
	{
		GroundDecal item = sortedDecals.AddDecal<GroundDecal>(position, orientation, decalPool, layerBand, this, customDeathEffectId);
		activeDecals.Add(item);
	}

	internal void RemoveFromActive(GroundDecal decal)
	{
		activeDecals.Remove(decal);
	}

	public void UpdateLifeTime(float deltaTime)
	{
		for (int i = 0; i < activeDecals.Count; i++)
		{
			GroundDecal groundDecal = activeDecals[i];
			groundDecal.Lifetime += Time.deltaTime;
			if (groundDecal.Lifetime > groundDecal.TimeToLive)
			{
				sortedDecals.RemoveDecal(groundDecal, decalPool);
				i--;
			}
		}
	}

	private void Awake()
	{
		decalPool = new Pool(decalPrefab, decalsParent, 5);
		decalPrefab.gameObject.SetActive(value: false);
	}
}
