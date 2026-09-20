using System;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimScrollButtons : MonoBehaviour
{
	[SerializeField]
	private UIAnimSimpleButton buttonPrefab;

	private List<UIAnimSimpleButton> activeBtnsList = new List<UIAnimSimpleButton>();

	private List<UIAnimSimpleButton> btnsPool = new List<UIAnimSimpleButton>();

	private bool isActivated;

	public void Activate(List<string> idsList, Action<string> clickedCallback)
	{
		if (base.gameObject.activeSelf)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < idsList.Count; i++)
		{
			UIAnimSimpleButton button = GetButton();
			string id = idsList[i];
			button.Activate(id, delegate
			{
				clickedCallback?.Invoke(id);
				Deactivate();
			});
			activeBtnsList.Add(button);
		}
	}

	public void Deactivate()
	{
		ResetToPool();
		base.gameObject.SetActive(value: false);
	}

	private UIAnimSimpleButton GetButton()
	{
		if (btnsPool.Count == 0)
		{
			btnsPool.Add(buttonPrefab.Copy());
		}
		return btnsPool.PopFirst();
	}

	private void ResetToPool()
	{
		activeBtnsList.ForEach(delegate(UIAnimSimpleButton btn)
		{
			btn.Deactivate();
			btnsPool.Add(btn);
		});
		activeBtnsList.Clear();
	}
}
