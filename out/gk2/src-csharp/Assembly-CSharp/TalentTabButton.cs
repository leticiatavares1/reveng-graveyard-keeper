using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class TalentTabButton : MonoBehaviour
{
	[SerializeField]
	protected LazyButton button;

	[SerializeField]
	protected TalentIcon icon;

	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected Image backgroundImage;

	[SerializeField]
	protected Sprite activeBackSprite;

	[SerializeField]
	protected Sprite inactiveBackSprite;

	[SerializeField]
	protected Sprite inactiveBackSpriteSelected;

	protected string talentId;

	protected Action onPressAction;

	protected bool isActive;

	public string TalentId => talentId;

	public void Init(string talentId, Action onPressAction)
	{
		this.talentId = talentId;
		this.onPressAction = onPressAction;
		button.onDown.AddListener(HandlePress);
		button.onExit.RemoveAllListeners();
		button.onExit.AddListener(OnDeselect);
		button.onEnter.RemoveAllListeners();
		button.onEnter.AddListener(OnSelect);
	}

	public virtual void UpdateState(bool isActive, Canvas parentCanvas)
	{
		this.isActive = isActive;
		if (isActive)
		{
			backgroundImage.sprite = activeBackSprite;
		}
		else
		{
			backgroundImage.sprite = inactiveBackSprite;
		}
		DrawTalentIcon();
		button.interactable = !isActive;
		UpdateSorting(parentCanvas);
	}

	protected virtual void DrawTalentIcon()
	{
		icon.Draw(talentId, isActive);
	}

	public virtual void SetActionIndicatorState(bool isActive)
	{
	}

	public virtual void DrawMasteryValue()
	{
	}

	public void UpdateSorting(Canvas parentCanvas)
	{
		canvas.sortingOrder = parentCanvas.sortingOrder + (isActive ? 1 : (-1));
	}

	private void HandlePress()
	{
		LazyAudio.PlayAndForget("tab_click");
		onPressAction?.Invoke();
	}

	protected virtual void OnDisable()
	{
		if (button.interactable)
		{
			OnDeselect();
		}
	}

	protected virtual void OnSelect()
	{
		backgroundImage.sprite = inactiveBackSpriteSelected;
	}

	protected virtual void OnDeselect()
	{
		backgroundImage.sprite = inactiveBackSprite;
	}
}
