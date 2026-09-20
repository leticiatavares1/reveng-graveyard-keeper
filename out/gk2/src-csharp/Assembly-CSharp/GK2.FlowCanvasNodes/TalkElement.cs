using System;
using LazyBearTechnology;

namespace GK2.FlowCanvasNodes;

[Serializable]
public class TalkElement : Element
{
	public string text;

	public UIBasicBubble.ForceCornerPosition forceCornerPosition;

	public TalkElement(string wgoId, string text)
	{
		base.wgoId = wgoId;
		this.text = text;
	}
}
