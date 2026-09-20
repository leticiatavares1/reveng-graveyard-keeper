using System.Collections.Generic;

public class DonkeySteppedRotationPreset : SteppedRotationPreset
{
	private static DonkeySteppedRotationPreset instance;

	private List<Sector> possibleSectors = new List<Sector>
	{
		new Sector(-180f, 0f, Sector.RoundType.GetLower, Direction.Left),
		new Sector(0f, 180f, Sector.RoundType.GetGreater, Direction.Right)
	};

	public static DonkeySteppedRotationPreset Instance => instance ?? (instance = new DonkeySteppedRotationPreset());

	protected override List<Sector> PossibleSectors => possibleSectors;
}
