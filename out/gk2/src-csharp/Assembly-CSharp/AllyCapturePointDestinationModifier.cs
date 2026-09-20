public class AllyCapturePointDestinationModifier : ControlPointDestinationModifier
{
	public override bool ShouldAnchorOnArrival => false;

	public AllyCapturePointDestinationModifier(FightingCapturePoint capturePoint)
		: base(capturePoint)
	{
	}
}
