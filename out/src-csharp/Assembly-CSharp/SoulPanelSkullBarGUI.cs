using System;
using System.Collections.Generic;
using UnityEngine;

public class SoulPanelSkullBarGUI : MonoBehaviour
{
	[Serializable]
	public enum Align
	{
		Left,
		Center
	}

	public int negative_capacity;

	public int positive_capacity;

	public int red_skulls_sin;

	public int green_skulls_sin;

	public int red_skulls_organ;

	public int green_skulls_organ;

	public UIWidget green_back;

	public UIWidget back;

	public UIProgressBar green_bar;

	public UIProgressBar red_bar;

	[Space]
	public UIProgressBar green_bar_potential;

	[Space]
	public UIProgressBar red_bar_potential;

	public GameObject skull_red;

	public GameObject skull_white;

	public UIGrid grid;

	private const float INACTIVE_ALPHA = 1f;

	private List<GameObject> _skulls_red;

	private List<GameObject> _skulls_white;

	public Align bar_align;

	public void SetSkullValues(int red_skulls_sin, int white_skulls_sin, int red_skulls_organ = 0, int white_skulls_organ = 0)
	{
		this.red_skulls_sin = red_skulls_sin;
		green_skulls_sin = white_skulls_sin;
		this.red_skulls_organ = red_skulls_organ;
		green_skulls_organ = white_skulls_organ;
		Redraw();
	}

	public float GetSkullsFillRate()
	{
		float num = 0f;
		float num2 = 0f;
		if (red_skulls_sin > 0)
		{
			num = (float)red_skulls_organ / (float)red_skulls_sin;
		}
		if (green_skulls_sin > 0)
		{
			num2 = (float)green_skulls_organ / (float)green_skulls_sin;
		}
		return (num + num2) / 2f;
	}

	private void Redraw()
	{
		skull_red.SetActive(value: false);
		skull_white.SetActive(value: false);
		int num = 0;
		while (grid.transform.childCount > 2)
		{
			for (int i = 0; i < grid.transform.childCount; i++)
			{
				GameObject gameObject = grid.transform.GetChild(i).gameObject;
				if (!(gameObject == skull_red) && !(gameObject == skull_white))
				{
					gameObject.transform.parent = null;
					NGUITools.Destroy(gameObject);
				}
			}
			if (++num > 100)
			{
				break;
			}
		}
		_skulls_red = new List<GameObject>();
		_skulls_white = new List<GameObject>();
		for (int j = 0; j < negative_capacity; j++)
		{
			_skulls_red.Add(skull_red.Copy());
		}
		for (int k = 0; k < positive_capacity; k++)
		{
			_skulls_white.Add(skull_white.Copy());
		}
		grid.Reposition();
		grid.repositionNow = true;
		int num2 = red_skulls_sin;
		int num3 = _skulls_red.Count - 1;
		while (num3 >= 0 && num2 > 0)
		{
			num2--;
			if (_skulls_red[num3].TryGetComponent<SkullIconContainerGUI>(out var component))
			{
				component.SetSkullActive(num2 >= 0);
			}
			num3--;
		}
		int num4 = green_skulls_sin;
		for (int l = 0; l < _skulls_white.Count; l++)
		{
			if (num4 <= 0)
			{
				break;
			}
			num4--;
			if (_skulls_white[l].TryGetComponent<SkullIconContainerGUI>(out var component2))
			{
				component2.SetSkullActive(num4 >= 0);
			}
		}
		if (green_skulls_organ > positive_capacity)
		{
			green_skulls_organ = positive_capacity;
		}
		int num5 = green_skulls_organ;
		green_bar_potential.gameObject.SetActive(num5 > 0);
		green_bar_potential.value = (1f + (float)num5 * 12f) / 182f;
		green_bar_potential.ForceUpdate();
		int num6 = Math.Min(green_skulls_organ, green_skulls_sin);
		green_bar.gameObject.SetActive(num6 > 0);
		green_bar.value = (1f + (float)num6 * 12f) / 182f;
		green_bar.ForceUpdate();
		if (red_skulls_organ > negative_capacity)
		{
			red_skulls_organ = negative_capacity;
		}
		int num7 = red_skulls_organ;
		red_bar_potential.gameObject.SetActive(num7 > 0);
		red_bar_potential.value = (1f + (float)num7 * 12f) / 182f;
		red_bar_potential.ForceUpdate();
		int num8 = Math.Min(red_skulls_organ, red_skulls_sin);
		red_bar.SetActive(num8 > 0);
		red_bar.value = (1f + (float)num8 * 12f) / 182f;
		red_bar.ForceUpdate();
		back.width = 3 + (positive_capacity + negative_capacity) * 12;
		switch (bar_align)
		{
		case Align.Left:
			base.transform.localPosition = new Vector3((negative_capacity - 1 + positive_capacity - 3) * 12, 0f, 0f);
			break;
		case Align.Center:
			base.transform.localPosition = new Vector3((negative_capacity - 1 + positive_capacity - 3 - (negative_capacity + positive_capacity) / 2) * 12, 0f, 0f);
			break;
		}
	}
}
