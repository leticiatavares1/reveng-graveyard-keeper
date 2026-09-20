using LazyBearTechnology;
using UnityEngine.UI;

public class Dev_ButtonsPressTestWindow : LazyWindow<LazyWidgetDataBase>
{
	public LazyButton makeAllInteractable;

	public bool isAllInteractable = true;

	public override void Init()
	{
		base.Init();
		makeAllInteractable.onClick.AddListener(delegate
		{
			if (isAllInteractable)
			{
				Button[] componentsInChildren = GetComponentsInChildren<Button>(includeInactive: true);
				foreach (Button button in componentsInChildren)
				{
					if (!(button == makeAllInteractable))
					{
						button.interactable = false;
					}
				}
			}
			else
			{
				Button[] componentsInChildren = GetComponentsInChildren<Button>(includeInactive: true);
				foreach (Button button2 in componentsInChildren)
				{
					if (!(button2 == makeAllInteractable))
					{
						button2.interactable = true;
					}
				}
			}
			isAllInteractable = !isAllInteractable;
		});
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Open(null);
	}
}
