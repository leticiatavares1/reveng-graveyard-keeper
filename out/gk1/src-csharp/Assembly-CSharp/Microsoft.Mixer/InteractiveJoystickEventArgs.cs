namespace Microsoft.Mixer;

public class InteractiveJoystickEventArgs : InteractiveEventArgs
{
	public string ControlID { get; private set; }

	public double X { get; private set; }

	public double Y { get; private set; }

	public InteractiveParticipant Participant { get; private set; }

	internal InteractiveJoystickEventArgs(InteractiveEventType type, string id, InteractiveParticipant participant, double x, double y)
		: base(type)
	{
		ControlID = id;
		Participant = participant;
		X = x;
		Y = -1.0 * y;
	}
}
