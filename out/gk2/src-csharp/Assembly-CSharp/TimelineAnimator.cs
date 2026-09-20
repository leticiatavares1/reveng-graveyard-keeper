using UnityEngine;

[ExecuteAlways]
public class TimelineAnimator : MonoBehaviour
{
	private AnimationComponentBase animationComponent;

	public Animator Animator
	{
		get
		{
			if (!(animationComponent != null))
			{
				return null;
			}
			return animationComponent.Animator;
		}
	}

	private void OnEnable()
	{
		FindComponents();
	}

	private void FindComponents()
	{
		animationComponent = GetComponent<AnimationComponentBase>();
		if (animationComponent == null)
		{
			animationComponent = GetComponentInChildren<AnimationComponentBase>();
		}
	}

	public void SetAnimationState(AnimationState state)
	{
		if (animationComponent == null)
		{
			FindComponents();
		}
		if (!(animationComponent == null) && animationComponent.GetState() != state)
		{
			animationComponent.SetState(state);
		}
	}

	public AnimationState GetState()
	{
		if (animationComponent == null)
		{
			FindComponents();
		}
		if (!(animationComponent != null))
		{
			return AnimationState.Idle;
		}
		return animationComponent.GetState();
	}

	public void SetDirection(Direction direction)
	{
		if (animationComponent == null)
		{
			FindComponents();
		}
		if (!(animationComponent == null))
		{
			animationComponent.SetDirection(direction);
		}
	}

	public float GetDirectionAngle()
	{
		if (animationComponent == null || animationComponent.Animator == null)
		{
			return 0f;
		}
		return animationComponent.Animator.GetFloat(AnimationComponentBase.idDirectionAnimator);
	}

	public void SetDirectionAngle(float angle)
	{
		if (!(animationComponent == null) && !(animationComponent.Animator == null))
		{
			animationComponent.Animator.SetFloat(AnimationComponentBase.idDirectionAnimator, angle);
		}
	}

	public void SetAnimatorFloat(string floatName, float value)
	{
		if (animationComponent == null)
		{
			FindComponents();
		}
		if (!(animationComponent == null) && !(animationComponent.Animator == null) && animationComponent.Animator.GetFloat(floatName) != value)
		{
			animationComponent.Animator.SetFloat(floatName, value);
		}
	}

	public void UpdateAnimator(float deltaTime)
	{
		if (animationComponent == null)
		{
			FindComponents();
		}
		if (animationComponent != null && animationComponent.Animator != null && !Application.isPlaying)
		{
			animationComponent.Animator.Update(deltaTime);
		}
	}
}
