using LazyBearTechnology;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class Microphone : LazySingleton<Microphone>
{
	[SerializeField]
	private Transform target;

	public void SetTarget(Transform target)
	{
		this.target = target;
	}

	private void Update()
	{
		if (target != null)
		{
			base.transform.position = target.position;
		}
	}
}
