using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialListItemWidget : LazyWidget<UITutorialListItemWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Image selectionFrame;

	[SerializeField]
	private LazyButton button;

	private void Awake()
	{
		if (button != null)
		{
			button.onClick.RemoveAllListeners();
			button.onEnter.RemoveAllListeners();
			button.onExit.RemoveAllListeners();
			button.onClick.AddListener(OnPressed);
			button.onEnter.AddListener(OnOver);
			button.onExit.AddListener(OnOut);
			button.SetCallbacksIntoGamepadNavigationItem();
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		if (label != null)
		{
			label.text = LLBase.L(data.TutorialId);
		}
	}

	private void OnPressed()
	{
		data.OnPressed?.Invoke(data.TutorialId);
	}

	private void OnOver()
	{
		if (selectionFrame != null)
		{
			selectionFrame.gameObject.SetActive(value: true);
		}
	}

	private void OnOut()
	{
		if (selectionFrame != null)
		{
			selectionFrame.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (selectionFrame != null)
		{
			selectionFrame.gameObject.SetActive(value: false);
		}
	}

	protected override void TestDraw()
	{
	}
}
