using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class AnimationControlBehaviour : PlayableBehaviour
{
	public AnimationState animationState;

	public bool overrideDirection;

	public Direction direction;

	public string floatParameterName;

	public float floatParameterValue;

	private TimelineAnimator savedTrackBinding;

	private bool isInitialStateSaved;

	private AnimationState initialAnimationState;

	private float initialDirectionAngle;

	private float initialFloatValue;

	private bool hasInitialFloatValue;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		TimelineAnimator timelineAnimator = playerData as TimelineAnimator;
		if (timelineAnimator == null)
		{
			return;
		}
		if (!isInitialStateSaved)
		{
			savedTrackBinding = timelineAnimator;
			isInitialStateSaved = true;
			initialAnimationState = savedTrackBinding.GetState();
			initialDirectionAngle = savedTrackBinding.GetDirectionAngle();
			if (!string.IsNullOrEmpty(floatParameterName) && savedTrackBinding.Animator != null)
			{
				hasInitialFloatValue = true;
				initialFloatValue = savedTrackBinding.Animator.GetFloat(floatParameterName);
			}
		}
		timelineAnimator.SetAnimationState(animationState);
		if (overrideDirection)
		{
			timelineAnimator.SetDirection(direction);
		}
		if (!string.IsNullOrEmpty(floatParameterName))
		{
			timelineAnimator.SetAnimatorFloat(floatParameterName, floatParameterValue);
		}
		timelineAnimator.UpdateAnimator(info.deltaTime);
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		if (!Application.isPlaying && !(savedTrackBinding == null) && isInitialStateSaved)
		{
			savedTrackBinding.SetAnimationState(initialAnimationState);
			savedTrackBinding.SetDirectionAngle(initialDirectionAngle);
			if (hasInitialFloatValue)
			{
				savedTrackBinding.SetAnimatorFloat(floatParameterName, initialFloatValue);
			}
			savedTrackBinding.UpdateAnimator(0f);
			isInitialStateSaved = false;
			savedTrackBinding = null;
		}
	}
}
