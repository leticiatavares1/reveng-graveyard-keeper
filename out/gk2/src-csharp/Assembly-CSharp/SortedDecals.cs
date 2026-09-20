using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class SortedDecals
{
	private class CellDecalState
	{
		public int nextBloodSlot;

		public int nextGoreSlot;

		public int nextGutsSlot;

		public GroundDecal[] bloodSlots = new GroundDecal[31];

		public GroundDecal[] goreSlots = new GroundDecal[31];

		public GroundDecal[] gutsSlots = new GroundDecal[31];
	}

	private const float ONE_DIMENSION_THRESHOLD = 1.76f;

	private const int BLOOD_SLOT_COUNT = 31;

	private const int GORE_SLOT_COUNT = 31;

	private const int GUTS_SLOT_COUNT = 31;

	private Dictionary<int, Dictionary<int, CellDecalState>> decalsXZ = new Dictionary<int, Dictionary<int, CellDecalState>>();

	public GroundDecal AddDecal<T>(Vector3 position, Direction orientation, Pool decalPool, DecalLayerBand layerBand, GroundDecalsCollection ownerCollection, string customDeathEffectId = "") where T : GroundDecal
	{
		Vector3 position2 = RaycastUtils.TrySnapToTheGround(position, 1f, 10f);
		position2 += -Vector3.up * -0.005f;
		int num = Mathf.RoundToInt(position2.x / 1.76f);
		int num2 = Mathf.RoundToInt(position2.z / 1.76f);
		if (!decalsXZ.TryGetValue(num, out var value))
		{
			value = new Dictionary<int, CellDecalState>();
			decalsXZ[num] = value;
		}
		if (!value.TryGetValue(num2, out var value2))
		{
			value2 = (value[num2] = new CellDecalState());
		}
		int layerMin = GetLayerMin(layerBand);
		int num3 = layerBand switch
		{
			DecalLayerBand.Blood => value2.nextBloodSlot++ % 31, 
			DecalLayerBand.Guts => value2.nextGutsSlot++ % 31, 
			DecalLayerBand.Gore => value2.nextGoreSlot++ % 31, 
			_ => throw new ArgumentException("Invalid layer band"), 
		};
		int num4 = layerMin + num3;
		GroundDecal[] array = layerBand switch
		{
			DecalLayerBand.Blood => value2.bloodSlots, 
			DecalLayerBand.Guts => value2.gutsSlots, 
			DecalLayerBand.Gore => value2.goreSlots, 
			_ => throw new ArgumentException("Invalid layer band"), 
		};
		if (array[num3] != null)
		{
			GroundDecal groundDecal = array[num3];
			RemoveDecal(groundDecal, groundDecal.OwnerCollection?.DecalPool ?? decalPool);
		}
		position2 += VisualConsts.GetFightDecalLayerOffset(num4);
		T orCreateObject = decalPool.GetOrCreateObject<T>();
		orCreateObject.SetPlacementMetadata(num, num2, num4, layerBand, ownerCollection);
		orCreateObject.SpawnDecal(position2, orientation, customDeathEffectId);
		array[num3] = orCreateObject;
		return orCreateObject;
	}

	public void RemoveDecal(GroundDecal decal, Pool decalPool)
	{
		ClearSlot(decal);
		decal.OwnerCollection?.RemoveFromActive(decal);
		decal.ClearPlacementMetadata();
		decalPool.ReleaseObject(decal);
	}

	private void ClearSlot(GroundDecal decal)
	{
		if (decal.DepthLayer == 0)
		{
			return;
		}
		int cellX = decal.CellX;
		int cellZ = decal.CellZ;
		if (!decalsXZ.TryGetValue(cellX, out var value) || !value.TryGetValue(cellZ, out var value2))
		{
			return;
		}
		int num = decal.DepthLayer - GetLayerMin(decal.LayerBand);
		if (num >= 0)
		{
			GroundDecal[] array = decal.LayerBand switch
			{
				DecalLayerBand.Blood => value2.bloodSlots, 
				DecalLayerBand.Guts => value2.gutsSlots, 
				DecalLayerBand.Gore => value2.goreSlots, 
				_ => throw new ArgumentException("Invalid layer band"), 
			};
			if (num < array.Length && array[num] == decal)
			{
				array[num] = null;
			}
		}
	}

	private static int GetLayerMin(DecalLayerBand layerBand)
	{
		return layerBand switch
		{
			DecalLayerBand.Blood => 10, 
			DecalLayerBand.Guts => 40, 
			DecalLayerBand.Gore => 70, 
			_ => throw new ArgumentException("Invalid layer band"), 
		};
	}
}
