using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICustomTooltipComponent : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private Action<UICustomTooltipComponent> onImageTooltipOver;

	private Action<UICustomTooltipComponent> onImageTooltipOut;

	[SerializeField]
	private string langToken;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private Image selfImage;

	[SerializeField]
	private Sprite selectedSprite;

	[SerializeField]
	private Sprite notSelectedSprite;

	public void OnPointerEnter(PointerEventData eventData)
	{
		Show();
		LazyAudio.PlayAndForget("gui_hover_light");
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (eventData.fullyExited)
		{
			Hide(immediately: false);
		}
	}

	public void SetCallbacks(Action<UICustomTooltipComponent> onImageTooltipOver = null, Action<UICustomTooltipComponent> onImageTooltipOut = null)
	{
		this.onImageTooltipOver = onImageTooltipOver;
		this.onImageTooltipOut = onImageTooltipOut;
	}

	public void Show()
	{
		if (!string.IsNullOrEmpty(langToken))
		{
			UITooltip.ShowSimpleInfo(rectTransform, LLBase.L(langToken));
			if (selectedImage != null)
			{
				selectedImage.gameObject.SetActive(value: true);
			}
			if (selectedSprite != null)
			{
				selfImage.sprite = selectedSprite;
			}
			onImageTooltipOver?.Invoke(this);
		}
	}

	public void Hide(bool immediately)
	{
		if (string.IsNullOrEmpty(langToken))
		{
			return;
		}
		if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			if (immediately)
			{
				UITooltip.HideImmediately();
			}
			else
			{
				UITooltip.Hide();
			}
		}
		if (selectedImage != null)
		{
			selectedImage.gameObject.SetActive(value: false);
		}
		if (notSelectedSprite != null)
		{
			selfImage.sprite = notSelectedSprite;
		}
		onImageTooltipOut?.Invoke(this);
	}

	private void OnDisable()
	{
		Hide(immediately: true);
	}

	public void SetLocale(string locale)
	{
		langToken = locale;
	}
}
