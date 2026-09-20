using UnityEngine;

[DisallowMultipleComponent]
public class UISortComponent : MonoBehaviour
{
	[SerializeField]
	private float floorLine;

	public float FloorLine => floorLine;

	private void OnDrawGizmosSelected()
	{
		Vector3 position = base.transform.position;
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		float num = 100f;
		if (component != null)
		{
			num = component.sprite.bounds.size.x / 2f;
		}
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(new Vector3(position.x - num, position.y + floorLine, position.z), new Vector3(position.x + num, position.y + floorLine, position.z));
	}
}
