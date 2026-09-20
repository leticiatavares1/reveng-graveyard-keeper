using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesIntensity : MonoBehaviour
{
	[Serializable]
	private class EditableParticleEntity
	{
		[HideInInspector]
		public ParticleSystem.EmissionModule emission;

		[HideInInspector]
		public float initRateOverTime;

		public float minDistance = 100f;

		public ParticleSystem particleSystem;
	}

	private WorldGameObject player;

	[SerializeField]
	private float maxDistanceToObject = 900f;

	[SerializeField]
	private List<EditableParticleEntity> entities;

	private void Start()
	{
		player = MainGame.me.player;
		foreach (EditableParticleEntity entity in entities)
		{
			entity.initRateOverTime = entity.particleSystem.emission.rateOverTime.constant;
			entity.emission = entity.particleSystem.emission;
		}
	}

	private void Update()
	{
		for (int i = 0; i < entities.Count; i++)
		{
			float num = Vector2.Distance(base.transform.position, player.transform.position);
			if (num > maxDistanceToObject)
			{
				entities[i].emission.rateOverTime = 0f;
				continue;
			}
			if (num <= entities[i].minDistance)
			{
				entities[i].emission.rateOverTime = entities[i].initRateOverTime;
				continue;
			}
			float num2 = (maxDistanceToObject - num) / maxDistanceToObject;
			entities[i].emission.rateOverTime = entities[i].initRateOverTime * num2;
		}
	}
}
