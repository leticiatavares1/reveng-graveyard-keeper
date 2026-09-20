using System;

namespace Microsoft.Mixer;

public class InteractiveEventArgs : EventArgs
{
	public DateTime Time { get; private set; }

	public int ErrorCode { get; private set; }

	public string ErrorMessage { get; private set; }

	public InteractiveEventType EventType { get; private set; }

	public InteractiveEventArgs()
	{
		Time = DateTime.UtcNow;
		ErrorCode = 0;
		ErrorMessage = string.Empty;
	}

	internal InteractiveEventArgs(InteractiveEventType type)
	{
		Time = DateTime.UtcNow;
		ErrorCode = 0;
		ErrorMessage = string.Empty;
		EventType = type;
	}

	internal InteractiveEventArgs(InteractiveEventType type, int errorCode, string errorMessage)
	{
		Time = DateTime.UtcNow;
		ErrorCode = errorCode;
		ErrorMessage = errorMessage;
		EventType = type;
	}
}
