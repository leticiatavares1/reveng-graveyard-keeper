using UnityEngine;

public class DropViewAtomMeshZombieContainer : MonoBehaviour
{
	public Transform container;

	public SpriteText spriteText;

	public bool isPhysical = true;

	public GameObject colliderContainer;

	private void Awake()
	{
		OnDisable();
	}

	private void OnEnable()
	{
		if (isPhysical && (bool)colliderContainer)
		{
			colliderContainer.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)colliderContainer)
		{
			colliderContainer.SetActive(value: false);
		}
	}
}
