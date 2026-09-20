using UnityEngine;

public class QuestInfoDecor : MonoBehaviour
{
	public GameObject noCenter;

	public GameObject centerDown;

	public GameObject centerUp;

	public GameObject centerUpAndDown;

	public void DisableAll()
	{
		noCenter.gameObject.SetActive(value: false);
		centerDown.gameObject.SetActive(value: false);
		centerUp.gameObject.SetActive(value: false);
		centerUpAndDown.gameObject.SetActive(value: false);
	}
}
