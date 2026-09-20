using System.Collections.Generic;
using UnityEngine;

public class ReservoirView : MonoBehaviour
{
	[SerializeField]
	private ReservoirConfig config;

	[SerializeField]
	private Transform vfxParent;

	[SerializeField]
	private float vfxDepthOffsetY = 0.8f;

	private List<FishingDef> fishingDefs = new List<FishingDef>();

	private WgoData wgoData;

	private ParticleSystem particles;

	private DockPoint dockPoint;

	private Vector3 direction;

	private Vector3 spawnPos;

	private bool isInitialized;

	public ReservoirConfig Config => config;

	public void Init(Wgo wgo)
	{
		if (!isInitialized && (bool)wgo && (bool)wgo.MainWgoPart)
		{
			wgoData = wgo.Data;
			wgoData.OnGameResChanged += UpdateFishesInPond;
			fishingDefs = FishingDef.GetAllForReservoir(wgoData.id);
			particles = vfxParent.GetComponentInChildren<ParticleSystem>();
			dockPoint = wgo.DockPoints[0];
			direction = dockPoint.Direction.ConvertToVector2XZ();
			direction = new Vector3(direction.x, 0f, direction.y);
			spawnPos = dockPoint.transform.position + direction.normalized * config.fishSpawnHorOffset + Vector3.down * config.fishSpawnVertOffset;
			UpdateFishesInPond();
			isInitialized = true;
		}
	}

	private void OnDestroy()
	{
		if (wgoData != null)
		{
			wgoData.OnGameResChanged -= UpdateFishesInPond;
		}
	}

	private void UpdateFishesInPond(string fishGameRes = "")
	{
		ParticleSystem.EmissionModule emission = particles.emission;
		int fishCount = 0;
		vfxParent.position = spawnPos + Vector3.down * vfxDepthOffsetY;
		fishingDefs.ForEach(delegate(FishingDef x)
		{
			fishCount += wgoData.GetGameResInt(x.fishId);
		});
		if (fishCount <= 5)
		{
			emission.rateOverTime = (float)fishCount / 10f;
		}
		else
		{
			emission.rateOverTime = 0.5f;
		}
	}
}
