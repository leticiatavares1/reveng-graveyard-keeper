using LazyBearTechnology;
using UnityEngine;

[ExecuteInEditMode]
public class NpcFishingContainer : MonoBehaviour
{
	[SerializeField]
	private RopeRenderer rope;

	[SerializeField]
	private Transform castStartPoint;

	[SerializeField]
	private RopePoint bob;

	[SerializeField]
	private AnimationComponent animationComponent;

	[SerializeField]
	private Transform target;

	[Header("Bob Movement")]
	[SerializeField]
	private float bobMovementDuration = 0.1f;

	[SerializeField]
	private float bobDivingAmount = 0.3f;

	[SerializeField]
	private AnimationCurve bobMovementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve bobDivingCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, -1f), new Keyframe(1f, 0f));

	private float moveProgress = 1f;

	private Vector3 bobStartPos;

	private bool hasActivatedAtPeak;

	private void OnEnable()
	{
		bobStartPos = target.position;
		moveProgress = 0f;
		hasActivatedAtPeak = false;
		animationComponent.SetTrigger("fish_cast");
	}

	private void OnDisable()
	{
	}

	private void LateUpdate()
	{
		if (moveProgress < 1f)
		{
			BringBobToStart();
		}
	}

	private void BringBobToStart()
	{
		moveProgress += Time.deltaTime / bobMovementDuration;
		float time = Mathf.Clamp01(moveProgress);
		float num = bobMovementCurve.Evaluate(time);
		Vector3 vector = Vector3.Lerp(castStartPoint.position, bobStartPos, num);
		float num2 = bobDivingCurve.Evaluate(time) * bobDivingAmount;
		bob.transform.position = vector + Vector3.up * num2;
		if (num >= 0.2f && !hasActivatedAtPeak)
		{
			if (animationComponent.Animator.TryGetComponent<FXContainerEventListener>(out var component))
			{
				component.PlayFx("fishing_catch");
				LazyAudio.PlayAtGameObject("fishing_blop", base.transform, SpatialType.sound3D);
			}
			hasActivatedAtPeak = true;
		}
	}
}
