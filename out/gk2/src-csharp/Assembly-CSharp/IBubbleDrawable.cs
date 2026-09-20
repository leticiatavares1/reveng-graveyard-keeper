using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public interface IBubbleDrawable
{
	SGuid BubbleDrawableUniqueId { get; }

	List<LazyWidgetDataBase> BubbleDrawableWidgets { get; }

	Vector3 BubbleDrawablePosition { get; }
}
