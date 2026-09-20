using UnityEngine;

public class MA_DestroyFinishedParticle : MonoBehaviour
{
	private ParticleSystem particles;

	private void Awake()
	{
		base.useGUILayout = false;
		particles = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (!particles.IsAlive())
		{
			Object.Destroy(base.gameObject);
		}
	}
}
