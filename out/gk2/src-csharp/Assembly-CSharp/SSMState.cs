public abstract class SSMState
{
	protected PlayerController playerController;

	public virtual bool CanEnter => false;

	public virtual bool IsActive => true;

	public SSMState(PlayerController playerController)
	{
		this.playerController = playerController;
	}

	public virtual void Update()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}
}
