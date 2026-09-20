using System.Collections.Generic;

public class BasicNpcSteppedRotationPreset : SteppedRotationPreset
{
	private static BasicNpcSteppedRotationPreset instance;

	private List<Sector> possibleSectors = new List<Sector>
	{
		new Sector(-180f, -90f, Sector.RoundType.GetLower, Direction.Left),
		new Sector(-90f, 0f, Sector.RoundType.GetGreater, Direction.Down),
		new Sector(0f, 90f, Sector.RoundType.GetLower, Direction.Right),
		new Sector(90f, 180f, Sector.RoundType.GetGreater, Direction.Up),
		new Sector(180f, 180f, Sector.RoundType.GetGreater, Direction.Left)
	};

	public static BasicNpcSteppedRotationPreset Instance => instance ?? (instance = new BasicNpcSteppedRotationPreset());

	protected override List<Sector> PossibleSectors => possibleSectors;
}
