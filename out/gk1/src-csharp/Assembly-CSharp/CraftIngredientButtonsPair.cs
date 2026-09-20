using System;
using UnityEngine;

public class CraftIngredientButtonsPair : MonoBehaviour
{
	public UIButton button_previous;

	public UIButton button_next;

	private Action<int> _on_ingredient_changed;

	public void Init(bool available, Action<int> on_ingredient_changed)
	{
		base.gameObject.SetActive(available);
		_on_ingredient_changed = on_ingredient_changed;
		UIButton uIButton = button_next;
		bool isEnabled = (button_previous.isEnabled = true);
		uIButton.isEnabled = isEnabled;
	}

	public void SetEnabled(bool previous, bool next)
	{
		if (base.gameObject.activeSelf)
		{
			button_previous.isEnabled = previous;
			button_next.isEnabled = next;
		}
	}

	public void OnPreviousPressed()
	{
		Sounds.OnGUIClick();
		_on_ingredient_changed(1);
	}

	public void OnNextPressed()
	{
		Sounds.OnGUIClick();
		_on_ingredient_changed(-1);
	}
}
