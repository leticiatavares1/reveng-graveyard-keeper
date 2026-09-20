using Cinemachine;
using UnityEngine;

public class UIInteractingItem : MonoBehaviour
{
	[SerializeField]
	private UIItemCell interactingItem;

	private static UIInteractingItem instance;

	private Transform targetTransform;

	public void Init()
	{
		instance = this;
		base.gameObject.SetActive(value: false);
	}

	public void DisableBubble()
	{
		Object.Destroy(base.gameObject);
	}

	public static UIInteractingItem ShowInteractingItem(Item item, Transform targetTransform)
	{
		if (instance == null)
		{
			Debug.LogError("InteractingItem.ShowInteractingitem error: instance is null");
			return null;
		}
		UIInteractingItem uIInteractingItem = instance.Copy();
		uIInteractingItem.targetTransform = targetTransform;
		uIInteractingItem.ShowItem(item);
		return uIInteractingItem;
	}

	private void ShowItem(Item item)
	{
		interactingItem.Draw(item);
		base.gameObject.SetActive(value: true);
		CinemachineCore.CameraUpdatedEvent.AddListener(UpdatePosition);
	}

	private void UpdatePosition(CinemachineBrain brain)
	{
		base.transform.position = CameraSystem.WorldToScreenPoint(targetTransform.position);
	}

	private void OnDestroy()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePosition);
	}
}
