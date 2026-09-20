using System.Collections.Generic;
using LinqTools;
using Sirenix.Utilities;
using UnityEngine;

public class UIMultiAnswerIconGroup : MonoBehaviour
{
	[SerializeField]
	private List<UIMultiAnswerIcon> icons;

	public List<UIMultiAnswerIcon> Icons => icons;

	private void Start()
	{
		icons = GetComponentsInChildren<UIMultiAnswerIcon>(includeInactive: true).ToList();
	}

	public void Show(int number)
	{
		if (icons.IsNullOrEmpty())
		{
			icons = GetComponentsInChildren<UIMultiAnswerIcon>(includeInactive: true).ToList();
		}
		if (number > 2)
		{
			Debug.LogError("Trying to display more than max icon count");
			number = 2;
		}
		if (number != 0)
		{
			HideAll();
			base.gameObject.SetActive(value: true);
			for (int i = 0; i < number; i++)
			{
				icons[i].Show();
			}
		}
	}

	public void HideAll()
	{
		foreach (UIMultiAnswerIcon icon in icons)
		{
			icon.Hide();
		}
		base.gameObject.SetActive(value: false);
	}
}
