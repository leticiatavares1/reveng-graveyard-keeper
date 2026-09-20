using LazyBearTechnology;
using UnityEngine;

public class ConveyorPlaySoundOnEnable : PlaySoundOnEnable
{
	[SerializeField]
	private float massMultiplier = 1f;

	public float MassMultiplier => massMultiplier;

	protected override void OnEnable()
	{
		LazySingleton<ConveyorSoundSystem>.Instance.Register(this);
	}

	protected override void OnDisable()
	{
		LazySingleton<ConveyorSoundSystem>.Instance.Unregister(this);
	}

	protected override void OnDestroy()
	{
		LazySingleton<ConveyorSoundSystem>.Instance.Unregister(this);
	}
}
