using DarkTonic.MasterAudio;
using UnityEngine;

public class SmartSoundController : MonoBehaviour
{
	public string sound;

	public float min_delay;

	public float max_delay;

	public bool dont_stop_sound_on_disable;

	[Range(0f, 10f)]
	public float custom_distance;

	private Vector2 _pos;

	private float last_played_time;

	private float result_delay;

	private PlaySoundResult _ps;

	private void Start()
	{
		RandomDelay();
	}

	public void OnEnable()
	{
		_pos = base.transform.position;
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
		if (Time.time > last_played_time + result_delay)
		{
			_ps = Sounds.PlaySound(sound, _pos, force_play: false, custom_distance);
			last_played_time = Time.time;
			RandomDelay();
		}
		if (_ps != null)
		{
			float volumePercentage = Sounds.CalcSoundVolume(_pos, custom_distance);
			_ps.ActingVariation.AdjustVolume(volumePercentage);
		}
	}

	private void RandomDelay()
	{
		result_delay = Random.Range(min_delay, max_delay);
	}
}
