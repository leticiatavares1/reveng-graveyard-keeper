using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class VerticalFogDisableZone : MonoBehaviour
{
	private BoxCollider boxCollider;

	private bool isPlayerInZone;

	public float gradientRange = 5f;

	private static VerticalFogDisableZone playerZone;

	private static float normalizedDistance;

	private BoxCollider BoxCollider
	{
		get
		{
			if (!boxCollider)
			{
				boxCollider = GetComponent<BoxCollider>();
			}
			return boxCollider;
		}
	}

	private void Awake()
	{
		Collider[] array = Physics.OverlapBox(BoxCollider.bounds.center, BoxCollider.bounds.extents);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent<PlayerController>() != null)
			{
				OnPlayerEnterZone();
				break;
			}
		}
	}

	private void OnValidate()
	{
		BoxCollider.isTrigger = true;
	}

	private void OnDrawGizmosSelected()
	{
		OnValidate();
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)other.gameObject.GetComponent<PlayerController>())
		{
			OnPlayerEnterZone();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((bool)other.gameObject.GetComponent<PlayerController>())
		{
			OnPlayerExitZone();
		}
	}

	private void OnPlayerEnterZone()
	{
		isPlayerInZone = true;
		playerZone = this;
	}

	private void OnPlayerExitZone()
	{
		isPlayerInZone = false;
		playerZone = null;
	}

	private void OnDisable()
	{
		OnPlayerExitZone();
	}

	private float FindDistanceToClosestBorder()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return float.MaxValue;
		}
		Vector3 position = MainGame.PlayerController.transform.position;
		Bounds bounds = BoxCollider.bounds;
		float num = Mathf.Abs(bounds.max.x - position.x);
		float num2 = Mathf.Abs(position.x - bounds.min.x);
		float num3 = Mathf.Abs(bounds.max.z - position.z);
		float num4 = Mathf.Abs(position.z - bounds.min.z);
		return Mathf.Min(num, num2, num3, num4);
	}

	public static float GetFogDisableAmount()
	{
		if (playerZone == null)
		{
			return 0f;
		}
		normalizedDistance = Mathf.Clamp01(playerZone.FindDistanceToClosestBorder() / playerZone.gradientRange);
		return normalizedDistance;
	}
}
