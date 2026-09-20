using DG.Tweening;
using UnityEngine;

internal class PerspectiveTextGUI : BaseGUI
{
	public GameObject camera_start_go;

	public GameObject camera_end_go;

	[Range(0.001f, 1f)]
	public float slide_speed = 0.1f;

	[Range(-90f, 90f)]
	public float camera_rot_x;

	public GameObject dark_back;

	public GameObject fade_obj;

	public UILabel ui_label;

	public string music_id;

	private float _slide_speed;

	private bool _is_sliding;

	private Camera _sliding_camera;

	private float _y;

	private float _z;

	private bool _fade_is_played;

	private bool _is_talking_now;

	private GJCommons.VoidDelegate _on_finished;

	private readonly SmartSpeechEngine.VoiceID _voice = SmartSpeechEngine.VoiceID.Skull;

	private float _voice_volume = 0.3f;

	private const float _CAM_FIELD_OF_VEIW = 83f;

	private Vector3 _CAM_ROT = new Vector3(-50f, 0f, 0f);

	private float _TIME_SLIDE_COEFF = 0.16042781f;

	private float _CAM_FADE_TIME = 3f;

	private float _CAM_NEAR_CLIP_PLANE = 0.01f;

	private float _EASTERN_SPEED_COEFF = 0.25f;

	private int _syll_count;

	public void OpenSlidingText(GJCommons.VoidDelegate on_finished)
	{
		Open(play_open_sound: false);
		InstantiateCameraAtPoint(new Vector3(0f, 1000f, 0f));
		GJTimer.AddTimer(0.01f, delegate
		{
			_sliding_camera.transform.position = camera_start_go.transform.position;
			dark_back.gameObject.transform.eulerAngles = new Vector3(camera_rot_x, 0f, 0f);
			_y = _sliding_camera.transform.position.y;
			_z = _sliding_camera.transform.position.z;
			_on_finished = on_finished;
			_slide_speed = slide_speed;
			if (GJL.IsEastern())
			{
				slide_speed *= _EASTERN_SPEED_COEFF;
			}
			_is_sliding = true;
			_fade_is_played = false;
			int width = Screen.width;
			if (width <= 1024)
			{
				ui_label.width = 210;
			}
			else if (width <= 1280)
			{
				ui_label.width = 240;
			}
			GJTimer.AddTimer(2f, delegate
			{
				_is_talking_now = true;
			});
		});
	}

	private void InstantiateCameraAtPoint(Vector3 position)
	{
		Camera gui_cam = MainGame.me.gui_cam;
		_sliding_camera = Object.Instantiate(gui_cam);
		MainGame.me.gui_cam.gameObject.SetActive(value: false);
		_sliding_camera.orthographic = false;
		_sliding_camera.fieldOfView = 83f;
		_sliding_camera.gameObject.transform.position = position;
		_sliding_camera.gameObject.transform.eulerAngles = new Vector3(camera_rot_x, 0f, 0f);
		_sliding_camera.nearClipPlane = _CAM_NEAR_CLIP_PLANE;
		SmartAudioEngine.me.PlayOvrMusic(music_id);
	}

	public override void Update()
	{
		base.Update();
		if (_is_sliding)
		{
			_y -= Time.deltaTime * slide_speed;
			_sliding_camera.transform.position = new Vector3(0f, _y, _z);
			fade_obj.transform.position = new Vector3(0f, _y, 0f);
			if (!_fade_is_played && (double)Mathf.Abs(_y - camera_end_go.transform.position.y) < 0.1)
			{
				_fade_is_played = true;
				PlayCameraFade(delegate
				{
					_is_talking_now = false;
					_is_sliding = false;
					base.Hide(play_hide_sound: false);
					MainGame.me.gui_cam.gameObject.SetActive(value: true);
					_sliding_camera.gameObject.Destroy();
					SmartAudioEngine.me.StopOvrMusic(music_id);
					_on_finished.TryInvoke();
				});
			}
		}
		if (!_is_talking_now)
		{
			return;
		}
		SmartSpeechEngine.me.PlayVoiceSound(_voice, _voice_volume);
		int num = Random.Range(50, 120);
		_syll_count++;
		if (_syll_count >= num)
		{
			_syll_count = 0;
			_is_talking_now = false;
			GJTimer.AddTimer(Random.Range(0.4f, 1.2f), delegate
			{
				_is_talking_now = true;
			});
		}
	}

	private void PlayCameraFade(GJCommons.VoidDelegate on_finished_fade)
	{
		CameraTools.Fade(delegate
		{
			CameraTools.UnFade(on_finished_fade, _CAM_FADE_TIME);
		}, _CAM_FADE_TIME);
	}

	private void StartSlideCamera(GJCommons.VoidDelegate on_finished)
	{
		float num = Vector3.Magnitude(camera_start_go.transform.position - camera_end_go.transform.position) * _TIME_SLIDE_COEFF;
		_sliding_camera.gameObject.transform.DOMove(camera_end_go.transform.position, num);
		GJTimer.AddTimer(num - _CAM_FADE_TIME, delegate
		{
			CameraTools.Fade(delegate
			{
				CameraTools.UnFade(on_finished, _CAM_FADE_TIME);
			}, _CAM_FADE_TIME);
		});
	}
}
