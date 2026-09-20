using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIContextMenuWindowData : LazyWidgetDataBase
{
	public List<UIContextMenuWindowWidgetData> Options { get; set; }

	public Vector2 Position { get; set; }
}
