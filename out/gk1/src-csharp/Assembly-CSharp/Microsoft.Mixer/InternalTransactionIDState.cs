namespace Microsoft.Mixer;

internal struct InternalTransactionIDState
{
	internal string previousTransactionID;

	internal string transactionID;

	internal string nextTransactionID;

	public InternalTransactionIDState(string newTransactionID)
	{
		nextTransactionID = newTransactionID;
		transactionID = null;
		previousTransactionID = null;
	}
}
