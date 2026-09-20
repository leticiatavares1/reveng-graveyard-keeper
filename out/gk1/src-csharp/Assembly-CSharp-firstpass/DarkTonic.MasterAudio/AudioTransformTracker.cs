using UnityEngine;

namespace DarkTonic.MasterAudio;

public class AudioTransformTracker : MonoBehaviour
{
	public int _frames;

	private Transform _trans;

	public Transform Trans
	{
		get
		{
			if (_trans == null)
			{
				_trans = base.transform;
			}
			return _trans;
		}
	}

	private void Update()
	{
		_frames++;
	}
}
