namespace Microsoft.Mixer;

public class InteractiveButtonEventArgs : InteractiveEventArgs
{
	public string ControlID { get; private set; }

	public InteractiveParticipant Participant { get; private set; }

	public bool IsPressed { get; private set; }

	public string TransactionID { get; private set; }

	public uint Cost { get; private set; }

	public void CaptureTransaction()
	{
		InteractivityManager.SingletonInstance.CaptureTransaction(TransactionID);
	}

	internal InteractiveButtonEventArgs(InteractiveEventType type, string id, InteractiveParticipant participant, bool isPressed, uint cost, string transactionID)
		: base(type)
	{
		ControlID = id;
		Participant = participant;
		Cost = cost;
		IsPressed = isPressed;
		TransactionID = transactionID;
	}
}
