public class VerticalFogGlobalK
{
	public static float GlobalFogCoefficient => 1f - VerticalFogDisableZone.GetFogDisableAmount();
}
