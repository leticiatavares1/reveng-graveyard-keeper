using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIContextMenuWindowWidget : LazyWidget<UIContextMenuWindowWidgetData>
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private GameObject selector;

	private void Awake()
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(OnClick);
		button.onEnter.AddListener(OnOver);
		button.onExit.AddListener(OnOut);
	}

	public override void Redraw()
	{
		base.Redraw();
		label.text = data.name;
		selector.SetActive(value: false);
		button.interactable = data.enabled;
	}

	private void OnClick()
	{
		data.callback?.Invoke();
	}

	private void OnOver()
	{
		selector.SetActive(value: true);
	}

	private void OnOut()
	{
		selector.SetActive(value: false);
	}

	private void OnDisable()
	{
		selector.SetActive(value: false);
	}

	protected override void TestDraw()
	{
	}
}
