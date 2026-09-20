using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIFadeWithText : UIBasicFade
{
	[SerializeField]
	private TextMeshProUGUI textMesh;

	[SerializeField]
	private TextStyle defaultTextStyle;

	private Canvas canvas;

	public override void Init()
	{
		base.Init();
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 800;
	}

	public void ScreenTextFade(string text, float screenFadeTime = 1f, float textFadeTime = 1f, float pauseTime = 1f, float textHoldTime = 2f, Action onTextShown = null, Action onScreenFadeOut = null, TextStyle overrideTextStyle = null)
	{
		if (overrideTextStyle != null)
		{
			overrideTextStyle.ApplyStyle(textMesh);
		}
		else
		{
			defaultTextStyle.ApplyStyle(textMesh);
		}
		Color textColor = textMesh.color;
		textMesh.text = LLBase.L(text);
		Color color = textColor;
		color.a = 0f;
		textMesh.color = color;
		textMesh.gameObject.SetActive(value: true);
		FadeIn(screenFadeTime, delegate
		{
			LazyTimer.AddTimer(pauseTime, delegate
			{
				textMesh.DOColor(textColor, textFadeTime).SetEase(Ease.Linear).OnComplete(delegate
				{
					LazyTimer.AddTimer(textHoldTime, delegate
					{
						textMesh.DOColor(Color.clear, textFadeTime).SetEase(Ease.Linear).OnComplete(delegate
						{
							onTextShown?.Invoke();
							FadeOut(screenFadeTime, delegate
							{
								textMesh.gameObject.SetActive(value: false);
								onScreenFadeOut?.Invoke();
							});
						});
					});
				});
			});
		});
	}

	public void ScreenTextFade(List<string> ids, float screenFadeTime = 1f, float textFadeTime = 1f, float pauseTime = 1f, float textHoldTime = 2f, Action onTextShown = null, Action onScreenFadeOut = null, TextStyle overrideTextStyle = null)
	{
		if (overrideTextStyle != null)
		{
			overrideTextStyle.ApplyStyle(textMesh);
		}
		else
		{
			defaultTextStyle.ApplyStyle(textMesh);
		}
		Color textColor = textMesh.color;
		textMesh.text = LLBase.L(ids[0]);
		Color color = textColor;
		color.a = 0f;
		textMesh.color = color;
		textMesh.gameObject.SetActive(value: true);
		List<string> remainingPhrases = new List<string>();
		remainingPhrases.AddRange(ids);
		remainingPhrases.RemoveAt(0);
		FadeIn(screenFadeTime, delegate
		{
			LazyTimer.AddTimer(pauseTime, ShowText);
		});
		void ShowText()
		{
			textMesh.DOColor(textColor, textFadeTime).SetEase(Ease.Linear).OnComplete(delegate
			{
				LazyTimer.AddTimer(textHoldTime, delegate
				{
					if (remainingPhrases.Count > 0)
					{
						textMesh.DOColor(Color.clear, textFadeTime).SetEase(Ease.Linear).OnComplete(delegate
						{
							textMesh.text = LLBase.L(remainingPhrases[0]);
							remainingPhrases.RemoveAt(0);
							ShowText();
						});
					}
					else
					{
						textMesh.DOColor(Color.clear, textFadeTime).SetEase(Ease.Linear).OnComplete(delegate
						{
							onTextShown?.Invoke();
							FadeOut(screenFadeTime, delegate
							{
								textMesh.gameObject.SetActive(value: false);
								onScreenFadeOut?.Invoke();
							});
						});
					}
				});
			});
		}
	}
}
