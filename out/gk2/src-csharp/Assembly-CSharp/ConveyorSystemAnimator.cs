using UnityEngine;

public abstract class ConveyorSystemAnimator : MonoBehaviour
{
	[SerializeField]
	protected Animator animator;

	protected bool isItemIdleLocked;

	protected bool isAdditionalItemIdleLocked;

	private string currentState = "Idle";

	public string CurrentState
	{
		get
		{
			return currentState;
		}
		set
		{
			if (!(currentState == value))
			{
				currentState = value;
				MarkPlaybackDirty();
			}
		}
	}

	public virtual void OnOrchestratorRegistered(string globalState, float phase)
	{
	}

	public virtual void MarkPlaybackDirty()
	{
	}

	public abstract void CustomUpdate();

	public abstract void Play(float phase);

	public abstract void UpdateView();
}
