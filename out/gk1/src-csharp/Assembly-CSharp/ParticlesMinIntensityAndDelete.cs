using System.Collections.Generic;
using UnityEngine;

public class ParticlesMinIntensityAndDelete : MonoBehaviour
{
	[SerializeField]
	private List<ParticleSystem> particles;

	public bool do_delete;

	public float time_to_del;

	public void DoDelete(float time_to_del)
	{
		do_delete = true;
		this.time_to_del = time_to_del;
		foreach (ParticleSystem particle in particles)
		{
			ParticleSystem.MainModule main = particle.main;
			main.loop = false;
		}
	}

	public void Update()
	{
		if (!do_delete)
		{
			return;
		}
		time_to_del -= Time.deltaTime;
		if (time_to_del < 0f)
		{
			WorldGameObject componentInParent = GetComponentInParent<WorldGameObject>();
			if (componentInParent != null)
			{
				componentInParent.DestroyMe();
			}
		}
	}
}
