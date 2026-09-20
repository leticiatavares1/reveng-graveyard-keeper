using UnityEngine;

public class RecastFloor : MonoBehaviour
{
	public void UpdateParameters(Vector3 position, Vector2 size)
	{
		base.transform.position = position;
		base.transform.localScale = new Vector3(size.x, 0f, size.y);
	}
}
