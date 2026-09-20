using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GUIElementsVisibilityFixer : MonoBehaviour
{
	public List<GameObject> dont_disable;

	public List<GameObject> force_disable = new List<GameObject>();

	public void Awake()
	{
		BaseGUI[] componentsInChildren = GetComponentsInChildren<BaseGUI>();
		foreach (BaseGUI baseGUI in componentsInChildren)
		{
			if (!dont_disable.Contains(baseGUI.gameObject))
			{
				baseGUI.gameObject.SetActive(value: false);
			}
		}
		foreach (GameObject item in force_disable)
		{
			item.SetActive(value: false);
		}
		GUIElements.me.speech_bubble.gameObject.SetActive(value: false);
	}
}
