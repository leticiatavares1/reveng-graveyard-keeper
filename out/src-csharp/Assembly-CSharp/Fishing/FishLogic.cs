using UnityEngine;

namespace Fishing;

public class FishLogic
{
	public struct Result
	{
		public float progress;

		public float fish_pos;

		public bool fail;

		public bool success;
	}

	private const float OFFSET = 5f;

	private FishPreset _preset;

	private float _fish_pos = 5f;

	private float _progress;

	private float _fail_time;

	private bool _moving_up;

	private const string _FISH_PULL_MLTPLR_BUFF = "buff_pulling_fish_mltplr";

	public FishLogic(FishPreset preset)
	{
		_preset = preset;
		_moving_up = true;
		_preset.InitTimeCalculation();
	}

	public Result CalculateFishPos(float pos, float rod_zone_size)
	{
		bool flag = _fish_pos >= pos && _fish_pos <= pos + rod_zone_size;
		float param = MainGame.me.player.data.GetParam("buff_pulling_fish_mltplr");
		float num = ((Mathf.Abs(param) > 0.01f) ? param : 1f);
		_fish_pos = _preset.CalculateFishPos(Time.deltaTime, normalize: true) * 100f;
		_progress += (flag ? _preset.progress_k_in_zone : ((0f - _preset.progress_k_out_of_zone) / num)) * Time.deltaTime;
		if (_progress <= 0f)
		{
			_progress = 0f;
		}
		else if (_progress > 1f)
		{
			_progress = 1f;
		}
		_fail_time = (_progress.EqualsTo(0f) ? (_fail_time + Time.deltaTime) : 0f);
		Result result = default(Result);
		result.fish_pos = _fish_pos;
		result.progress = _progress;
		result.success = _progress >= 1f;
		result.fail = _fail_time >= _preset.zero_progress_fail_time;
		return result;
	}
}
