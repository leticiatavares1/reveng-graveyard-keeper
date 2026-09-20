using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

public class TextAnimator
{
	private TextMeshProUGUI label;

	private string text;

	private float startTime;

	private float totalShowTime;

	private float letterAnimAppearTime;

	private bool animating;

	public bool IsAnimating => animating;

	public float StartTime
	{
		get
		{
			return startTime;
		}
		set
		{
			startTime = value;
		}
	}

	public float TotalShowTime => totalShowTime;

	public void ShowMessage(TextMeshProUGUI label, string text, float letterAnimAppearTime = 0.02f)
	{
		this.label = label;
		this.letterAnimAppearTime = letterAnimAppearTime;
		this.text = text;
		startTime = Time.time;
		totalShowTime = letterAnimAppearTime * (float)ReplaceTagsAndSprites(this.text).Length;
		animating = true;
		ApplyAlphaToTextSinceIndex(0);
	}

	public void CustomUpdate()
	{
		if (animating)
		{
			AnimateText();
		}
	}

	public void Complete()
	{
		animating = false;
		ApplyAlphaToTextSinceIndex(text.Length);
	}

	public float GetRemainingTime()
	{
		return TotalShowTime - (Time.time - startTime);
	}

	private void AnimateText()
	{
		int num = Mathf.FloorToInt((Time.time - startTime) / letterAnimAppearTime);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case '<':
				num3++;
				continue;
			case '>':
				num3--;
				continue;
			}
			if (num3 <= 0 && num2++ >= num)
			{
				ApplyAlphaToTextSinceIndex(i);
				return;
			}
		}
		ApplyAlphaToTextSinceIndex(text.Length);
		animating = false;
	}

	private void ApplyAlphaToTextSinceIndex(int currentTextIndex)
	{
		string text = this.text.Substring(0, currentTextIndex);
		string str = this.text.Substring(currentTextIndex);
		str = ReplaceTagsAndSprites(str);
		label.text = text + "<color=#00000000>" + str + "</color>";
	}

	private string ReplaceTagsAndSprites(string str)
	{
		str = Regex.Replace(str, "<color[^>].+?>", "").Replace("</color>", "");
		str = Regex.Replace(str, "(<sprite[^>]*)>", "$1 color=#00000000>");
		return str;
	}
}
