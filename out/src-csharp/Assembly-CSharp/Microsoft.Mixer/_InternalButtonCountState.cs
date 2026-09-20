namespace Microsoft.Mixer;

internal struct _InternalButtonCountState
{
	internal uint PreviousCountOfButtonDownEvents;

	internal uint PreviousCountOfButtonPressEvents;

	internal uint PreviousCountOfButtonUpEvents;

	internal uint CountOfButtonDownEvents;

	internal uint CountOfButtonPressEvents;

	internal uint CountOfButtonUpEvents;

	internal uint NextCountOfButtonDownEvents;

	internal uint NextCountOfButtonPressEvents;

	internal uint NextCountOfButtonUpEvents;

	internal string PreviousTransactionID;

	internal string TransactionID;

	internal string NextTransactionID;
}
