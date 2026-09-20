using UnityEngine;

public class DropViewAtomMeshElement : MonoBehaviour
{
	public Object3D object3D;

	public SpriteText spriteText;

	public bool isPhysical = true;

	public GameObject colliderContainer;

	public Collider physicsCollider;

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
