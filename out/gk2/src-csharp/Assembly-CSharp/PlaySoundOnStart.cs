using LazyBearTechnology;

public class PlaySoundOnStart : PlaySoundOnEnable
{
	private void Start()
	{
		soundHandler = LazyAudio.PlayAtGameObject(soundId, base.transform, SpatialType.sound3D);
	}

	protected override void OnEnable()
	{
	}
}
