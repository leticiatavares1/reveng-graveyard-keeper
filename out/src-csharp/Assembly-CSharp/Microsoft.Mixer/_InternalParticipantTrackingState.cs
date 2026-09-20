namespace Microsoft.Mixer;

internal struct _InternalParticipantTrackingState
{
	internal InteractiveParticipant previousParticpant;

	internal InteractiveParticipant particpant;

	internal InteractiveParticipant nextParticpant;

	public _InternalParticipantTrackingState(InteractiveParticipant newParticipant)
	{
		nextParticpant = newParticipant;
		particpant = null;
		previousParticpant = null;
	}
}
