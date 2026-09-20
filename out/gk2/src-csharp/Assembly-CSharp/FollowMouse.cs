using UnityEngine;

public class FollowMouse : MonoBehaviour
{
	private void OnEnable()
	{
		Cursor.visible = false;
		base.transform.position = Input.mousePosition;
	}

	private void OnDisable()
	{
		Cursor.visible = true;
	}

	private void Update()
	{
		base.transform.position = Input.mousePosition;
	}
}
