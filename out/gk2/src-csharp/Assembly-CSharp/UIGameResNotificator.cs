using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIGameResNotificator : LazyWidget<UIGameResNotificatorData>
{
	[SerializeField]
	private UIResElement uiResPrefab;

	[SerializeField]
	private float delayBetweenElements = 0.3f;

	private float lastDisplayedTime;

	private List<UIResElement> labelsPool = new List<UIResElement>();

	public override void Init()
	{
		uiResPrefab.gameObject.SetActive(value: false);
	}

	private void OnDisplayingCompleted(UIResElement uiResElement)
	{
		uiResElement.gameObject.SetActive(value: false);
		labelsPool.Add(uiResElement);
	}

	private UIResElement GetNew()
	{
		if (labelsPool.Count == 0)
		{
			UIResElement uIResElement = uiResPrefab.Copy(null, activate: false);
			uIResElement.Init(OnDisplayingCompleted);
			return uIResElement;
		}
		return labelsPool.PopLast();
	}

	private void Update()
	{
		foreach (UIResElementData value in data.ResElementsWithAccumulators.Values)
		{
			if (Mathf.Abs(value.Value).EqualsOrMore(1f) && Time.time - lastDisplayedTime > delayBetweenElements)
			{
				lastDisplayedTime = Time.time;
				UIResElement @new = GetNew();
				@new.gameObject.SetActive(value: true);
				@new.Draw(value);
				return;
			}
		}
		foreach (string key in data.ResElementsWithoutAccumulators.Keys)
		{
			if (Time.time - lastDisplayedTime > delayBetweenElements)
			{
				lastDisplayedTime = Time.time;
				UIResElement new2 = GetNew();
				new2.gameObject.SetActive(value: true);
				new2.Draw(data.ResElementsWithoutAccumulators[key]);
				data.ResElementsWithoutAccumulators.Remove(key);
				break;
			}
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIGameResNotificatorData());
	}
}
