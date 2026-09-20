using UnityEngine;

public class VirtualCursor : MonoBehaviour
{
	public Collider2D collider2D;

	public float speed = 7f;

	private const float Z_POSITION = 0f;

	public Transform left;

	public Transform right;

	public Transform up;

	public Transform down;

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		Vector2 direction = LazyInput.GetDirection2();
		float num = GameSettings.current_resolution.pixel_size;
		Vector2 vector = (Vector2)base.transform.position + speed * direction * num;
		if (vector.x < left.position.x)
		{
			vector.x = left.position.x;
		}
		if (vector.x > right.position.x)
		{
			vector.x = right.position.x;
		}
		if (vector.y < down.position.y)
		{
			vector.y = down.position.y;
		}
		if (vector.y > up.position.y)
		{
			vector.y = up.position.y;
		}
		base.transform.position = new Vector3(vector.x, vector.y, 0f);
	}

	public void EnableAsMapCursor(Transform left, Transform right, Transform up, Transform down)
	{
		this.left = left;
		this.right = right;
		this.up = up;
		this.down = down;
		base.gameObject.SetActive(value: true);
	}
}
