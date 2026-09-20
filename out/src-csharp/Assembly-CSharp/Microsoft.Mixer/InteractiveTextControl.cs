using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveTextControl : InteractiveControl
{
	public IList<InteractiveTextResult> TextResults => InteractivityManager.SingletonInstance._GetText(base.ControlID);

	public InteractiveTextControl(string controlID, InteractiveEventType type, bool disabled, string helpText, string eTag, string sceneID, Dictionary<string, object> metaproperties)
		: base(controlID, "textbox", type, disabled, helpText, eTag, sceneID, metaproperties)
	{
	}
}
