using System;
using LazyBearTechnology;
using UnityEngine;

public class LadderClimbController : MonoBehaviour
{
	private const string climbAnimName = "Climb";

	[SerializeField]
	[Range(0.5f, 5f)]
	private float speedMultiplier = 1f;

	[SerializeField]
	[Range(0.1f, 5f)]
	private float climbMovementSpeed = 1f;

	[SerializeField]
	[Range(0.5f, 5f)]
	private float climbAnimSpeed = 1f;

	[SerializeField]
	private bool autoLeaveLadderEnabled;

	private AnimationComponentBase animationComponent;

	private float progress;

	private int animationLayerIndex;

	private Func<bool> climbUpControl;

	private Func<bool> climbDownControl;

	private bool isValid;

	private string stateNameHashed;

	private Ladder ladderUnderUse;

	private Ladder ladderUnderInteraction;

	private Rigidbody rigidbodyToMove;

	private bool isClimbActive;

	public bool AutoLeaveLadderEnabled => autoLeaveLadderEnabled;

	public bool CanUse => ladderUnderInteraction != null;

	public Ladder LadderUnderInteraction
	{
		get
		{
			return ladderUnderInteraction;
		}
		set
		{
			ladderUnderInteraction = value;
		}
	}

	public Ladder LadderUnderUse => ladderUnderUse;

	public bool IsClimbActive => isClimbActive;

	public bool CanLeaveLadder
	{
		get
		{
			Vector3 position = rigidbodyToMove.position;
			if (!ladderUnderUse.TopPart.IsInLeaveRange(position))
			{
				return ladderUnderUse.BotPart.IsInLeaveRange(position);
			}
			return true;
		}
	}

	public void Init(AnimationComponentBase animationComponent)
	{
		this.animationComponent = animationComponent;
	}

	public void StartClimb(float initPos, Func<bool> climbUpControl, Func<bool> climbDownControl, Ladder ladder, Rigidbody rigidbodyToMove)
	{
		if (isClimbActive)
		{
			Debug.Log("Climb is already active");
			return;
		}
		progress = initPos;
		this.climbUpControl = climbUpControl;
		this.climbDownControl = climbDownControl;
		stateNameHashed = animationComponent.Animator.GetLayerName(animationLayerIndex) + ".Climb";
		isValid = climbUpControl != null && climbDownControl != null;
		ladderUnderUse = ladder;
		this.rigidbodyToMove = rigidbodyToMove;
		SetPosOnClimbStart();
		isClimbActive = true;
		LazyAudio.PlayAndForget("ladder_climb_start");
	}

	public void StopClimb()
	{
		isClimbActive = false;
		SetPosOnClimbEnd();
		progress = 0f;
		isValid = false;
		climbUpControl = null;
		climbDownControl = null;
		ladderUnderUse = null;
		rigidbodyToMove = null;
		LazyAudio.PlayAndForget("ladder_climb_finish");
	}

	private void Update()
	{
		if (!isValid || !isClimbActive || MainGame.IsGamePaused)
		{
			return;
		}
		bool flag = climbUpControl();
		bool flag2 = climbDownControl();
		Vector3 position = rigidbodyToMove.position;
		bool flag3 = false;
		if (flag || flag2)
		{
			LadderEdgePart ladderEdgePart = (flag ? ladderUnderUse.TopPart : ladderUnderUse.BotPart);
			int num = (flag ? 1 : (-1));
			int num2 = (IsAbleToMoveToTarget(rigidbodyToMove.position, ladderEdgePart.StartPoint.position) ? 1 : 0);
			if (num2 == 1)
			{
				position += (ladderEdgePart.StartPoint.position - rigidbodyToMove.position).normalized * (Time.unscaledDeltaTime * climbMovementSpeed * speedMultiplier);
			}
			progress += Time.unscaledDeltaTime * climbAnimSpeed * speedMultiplier * (float)num2 * (float)num;
			flag3 = autoLeaveLadderEnabled && num2 == 0;
		}
		Vector3 position2 = ladderUnderUse.BotPart.StartPoint.position;
		Vector3 position3 = ladderUnderUse.TopPart.StartPoint.position;
		rigidbodyToMove.MovePosition(new Vector3(Mathf.Clamp(position.x, position2.x, position3.x), Mathf.Clamp(position.y, position2.y, position3.y), Mathf.Clamp(position.z, position2.z, position3.z)));
		animationComponent.Animator.Play(stateNameHashed, animationLayerIndex, progress);
		if (flag3)
		{
			StopClimb();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<LadderEdgePart>(out var component) && (bool)component.Ladder)
		{
			ladderUnderInteraction = component.Ladder;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<LadderEdgePart>(out var component) && (bool)component.Ladder && component.Ladder == ladderUnderInteraction)
		{
			ladderUnderInteraction = null;
		}
	}

	private bool IsAbleToMoveToTarget(Vector3 currentPos, Vector3 target)
	{
		if (!ladderUnderUse)
		{
			return false;
		}
		if (!(currentPos - target).magnitude.EqualsTo(0f, 0.001f))
		{
			return true;
		}
		return false;
	}

	private void SetPosOnClimbStart()
	{
		LadderEdgePart nearestLadderPart = ladderUnderUse.GetNearestLadderPart(rigidbodyToMove.position);
		if (!(nearestLadderPart == null))
		{
			rigidbodyToMove.position = nearestLadderPart.StartPoint.position;
		}
	}

	private void SetPosOnClimbEnd()
	{
		LadderEdgePart nearestLadderPart = ladderUnderUse.GetNearestLadderPart(rigidbodyToMove.position);
		if (!(nearestLadderPart == null))
		{
			rigidbodyToMove.position = nearestLadderPart.TpPoint.position;
			animationComponent.SetDirection(Direction.Up);
		}
	}
}
