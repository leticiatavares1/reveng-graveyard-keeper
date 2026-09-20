namespace Microsoft.Mixer;

public class InteractiveTextEventArgs : InteractiveEventArgs
{
	public string ControlID { get; private set; }

	public InteractiveParticipant Participant { get; private set; }

	public string Text { get; private set; }

	public string TransactionID { get; private set; }

	public void CaptureTransaction()
	{
		InteractivityManager.SingletonInstance.CaptureTransaction(TransactionID);
	}

	internal InteractiveTextEventArgs(InteractiveEventType type, string id, InteractiveParticipant participant, string text, string transactionID)
		: base(type)
	{
		ControlID = id;
		Participant = participant;
		Text = text;
		TransactionID = transactionID;
	}
}
