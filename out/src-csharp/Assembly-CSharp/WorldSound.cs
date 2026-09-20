using DarkTonic.MasterAudio;
using UnityEngine;

public class WorldSound : MonoBehaviour
{
	public string sound;

	public bool dont_stop_sound_on_disable;

	public bool force_play;

	[Range(0f, 10f)]
	public float custom_distance;

	private PlaySoundResult _ps;

	private Vector2 _pos;

	public void OnEnable()
	{
		_pos = base.transform.position;
		_ps = Sounds.PlaySound(sound, _pos, force_play, custom_distance);
		Update();
	}

	public void OnDisable()
	{
		if (_ps != null && !dont_stop_sound_on_disable)
		{
			_ps.ActingVariation.Stop();
			_ps = null;
		}
	}

	public void Update()
	{
		if (_ps != null)
		{
			float volumePercentage = Sounds.CalcSoundVolume(_pos, custom_distance);
			_ps.ActingVariation.AdjustVolume(volumePercentage);
		}
	}
}
