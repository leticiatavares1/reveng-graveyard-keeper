using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public abstract class TechTreeElementBaseWidget : LazyWidget<TechTreeElementBaseWidgetData>
{
	[SerializeField]
	protected Image selection;

	[SerializeField]
	protected GameObject hiddenObj;

	[SerializeField]
	protected GameObject visibleObj;

	[SerializeField]
	protected GameObject availableObj;

	[SerializeField]
	protected GameObject unlockedObj;

	public LazyButton button;

	public override void Init()
	{
		base.Init();
		OnDeselect();
	}

	public override void Redraw()
	{
		button.onExit.RemoveAllListeners();
		button.onExit.AddListener(OnDeselect);
		button.onEnter.RemoveAllListeners();
		button.onEnter.AddListener(OnSelect);
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(OnClicked);
		base.Redraw();
	}

	protected void OnClicked()
	{
		data.onTechClicked?.Invoke(data);
		OnDeselect();
		Redraw();
	}

	private void OnDisable()
	{
		OnDeselect();
	}

	private void OnSelect()
	{
		selection.gameObject.SetActive(value: true);
	}

	private void OnDeselect()
	{
		selection.gameObject.SetActive(value: false);
	}
}
