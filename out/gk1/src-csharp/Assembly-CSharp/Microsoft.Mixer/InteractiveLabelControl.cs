using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveLabelControl : InteractiveControl
{
	public string Text { get; private set; }

	public void SetText(string text)
	{
		InteractivityManager singletonInstance = InteractivityManager.SingletonInstance;
		singletonInstance._QueuePropertyUpdate(_sceneID, base.ControlID, singletonInstance._InteractiveControlPropertyToString(InteractiveControlProperty.Text), text);
	}

	public InteractiveLabelControl(string controlID, string text, string sceneID)
		: base(controlID, "label", InteractiveEventType.Unknown, disabled: false, "", "", sceneID, new Dictionary<string, object>())
	{
		Text = text;
	}
}
