using UnityEngine;

public class SkullIconContainerGUI : MonoBehaviour
{
	[SerializeField]
	private GameObject skull_obj;

	public void SetSkullActive(bool is_active)
	{
		if (skull_obj != null)
		{
			skull_obj.SetActive(is_active);
		}
	}
}
