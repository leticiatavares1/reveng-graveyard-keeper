using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ColliderTriggerComponentBase : MonoBehaviour
{
	private ColliderTriggerDataBase onEnterBase;

	private ColliderTriggerDataBase onExitBase;

	private bool isPlayerInside;

	private Collider collider;

	protected void Init(ColliderTriggerDataBase onEnter, ColliderTriggerDataBase onExit)
	{
		onEnterBase = onEnter;
		onExitBase = onExit;
		collider = GetComponent<Collider>();
		collider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isPlayerInside)
		{
			isPlayerInside = true;
			onEnterBase.TrySetTrigger();
			onExitBase.TryResetTrigger();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (isPlayerInside)
		{
			isPlayerInside = false;
			onExitBase.TrySetTrigger();
			onEnterBase.TryResetTrigger();
		}
	}
}
