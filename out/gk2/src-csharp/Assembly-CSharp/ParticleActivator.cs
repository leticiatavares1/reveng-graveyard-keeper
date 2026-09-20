using UnityEngine;

public class ParticleActivator : MonoBehaviour
{
	private ParticleSystem[] particleSystems;

	private void Awake()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
	}

	private void OnEnable()
	{
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Play();
		}
	}

	private void OnDisable()
	{
		ParticleSystem[] array = particleSystems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Stop();
		}
	}
}
