using System.Collections.Generic;
using System.Text;
using LinqTools;
using UnityEngine;

public class ButtonTipsStr : MonoBehaviour
{
	public UILabel label;

	private void Start()
	{
		if (label == null)
		{
			label = GetComponent<UILabel>();
		}
	}

	public void Clear()
	{
		label.text = "";
	}

	public void Print(params GameKeyTip[] tips)
	{
		Print(tips.ToList());
	}

	public void Print(string separator, params GameKeyTip[] tips)
	{
		Print(tips.ToList(), separator);
	}

	public void Print(List<GameKeyTip> tips, string separator = "   ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < tips.Count; i++)
		{
			string text = tips[i].ToString();
			stringBuilder.Append(text);
			if (text.Length > 0 && i < tips.Count - 1)
			{
				stringBuilder.Append(separator);
			}
		}
		if (label != null)
		{
			label.text = stringBuilder.ToString();
		}
	}

	public void Print(GameKeyTip tip)
	{
		label.text = tip.ToString();
	}

	public void PrintClose(bool gamepad_only = true)
	{
	}

	public void PrintBack(bool gamepad_only = true)
	{
	}
}
