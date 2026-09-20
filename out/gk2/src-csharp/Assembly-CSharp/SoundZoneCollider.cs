using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SoundZoneCollider : MonoBehaviour
{
	[SerializeField]
	private int index;

	private bool isSoundZoneCached;

	private SoundZone cachedSoundZone;

	public SoundZone SoundZone
	{
		get
		{
			if (!isSoundZoneCached)
			{
				cachedSoundZone = GetComponentInParent<SoundZone>();
				isSoundZoneCached = true;
			}
			return cachedSoundZone;
		}
	}

	public void Initialize(int index)
	{
		this.index = index;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponentInParent<ISoundZoneRecognizable>() != null)
		{
			SoundZone.OnPlayerEnter(index);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<ISoundZoneRecognizable>() != null)
		{
			SoundZone.OnPlayerExit(index);
		}
	}
}
