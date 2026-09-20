using UnityEngine;

public class AlwaysActiveOrInactive : MonoBehaviour
{
	public bool should_be_active = true;

	private void OnEnable()
	{
		UpdateState();
	}

	private void Update()
	{
		UpdateState();
	}

	public void UpdateState()
	{
		if (base.gameObject.activeSelf != should_be_active)
		{
			base.gameObject.SetActive(should_be_active);
		}
	}
}
