namespace Microsoft.Mixer;

public class InteractiveMessageEventArgs : InteractiveEventArgs
{
	public string Message { get; private set; }

	internal InteractiveMessageEventArgs(string message)
	{
		Message = message;
	}
}
