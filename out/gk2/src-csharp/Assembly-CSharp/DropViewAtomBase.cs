using UnityEngine;

public abstract class DropViewAtomBase : MonoBehaviour
{
	private DropView parentDropGameObject;

	private string iconId;

	public DropView ParentDropGameObject => parentDropGameObject;

	public string IconId => iconId;

	public virtual void Activate(string iconId)
	{
		base.gameObject.SetActive(value: true);
		this.iconId = iconId;
	}

	public abstract SpriteText GetSpriteText();

	public virtual void Deactivate()
	{
		base.gameObject.SetActive(value: false);
		iconId = null;
	}

	public virtual void SetInteractionState(bool isUnderInteraction)
	{
	}

	private void Awake()
	{
		parentDropGameObject = GetComponentInParent<DropView>();
	}
}
