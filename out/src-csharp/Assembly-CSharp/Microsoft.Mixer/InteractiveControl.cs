using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveControl
{
	internal string _eTag;

	internal string _sceneID;

	internal string _kind;

	internal InteractiveEventType _type;

	internal int participantID;

	public string ControlID { get; private set; }

	public bool Disabled { get; private set; }

	public string HelpText { get; private set; }

	public IDictionary<string, object> MetaProperties { get; private set; }

	public void SetDisabled(bool disabled)
	{
		Disabled = disabled;
		InteractivityManager.SingletonInstance._SendSetButtonControlProperties(ControlID, "disabled", disabled, 0f, string.Empty, 0u);
	}

	public void SetProperty(InteractiveControlProperty name, bool value)
	{
		SetPropertyImpl(InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name), value);
	}

	public void SetProperty(InteractiveControlProperty name, double value)
	{
		SetPropertyImpl(InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name), value);
	}

	public void SetProperty(InteractiveControlProperty name, string value)
	{
		SetPropertyImpl(InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name), value);
	}

	public void SetProperty(InteractiveControlProperty name, object value)
	{
		SetPropertyImpl(InteractivityManager.SingletonInstance._InteractiveControlPropertyToString(name), value);
	}

	public void SetProperty(string name, bool value)
	{
		SetPropertyImpl(name, value);
	}

	public void SetProperty(string name, double value)
	{
		SetPropertyImpl(name, value);
	}

	public void SetProperty(string name, string value)
	{
		SetPropertyImpl(name, value);
	}

	public void SetProperty(string name, object value)
	{
		SetPropertyImpl(name, value);
	}

	private void SetPropertyImpl(string name, bool value)
	{
		InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
	}

	private void SetPropertyImpl(string name, double value)
	{
		InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
	}

	private void SetPropertyImpl(string name, string value)
	{
		InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
	}

	private void SetPropertyImpl(string name, object value)
	{
		InteractivityManager.SingletonInstance._QueuePropertyUpdate(_sceneID, ControlID, name, value);
	}

	internal InteractiveControl(string controlID, string kind, InteractiveEventType type, bool disabled, string helpText, string eTag, string sceneID, Dictionary<string, object> metaProperties)
	{
		ControlID = controlID;
		_kind = kind;
		_type = type;
		Disabled = disabled;
		HelpText = helpText;
		_eTag = eTag;
		_sceneID = sceneID;
		participantID = -1;
		MetaProperties = metaProperties;
	}
}
