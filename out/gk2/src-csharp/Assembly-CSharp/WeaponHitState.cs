using System.Collections.Generic;

public class WeaponHitState
{
	private readonly HashSet<int> passedZones = new HashSet<int>();

	public bool IsInsideProtectedZone { get; private set; }

	public bool DamageDealt { get; private set; }

	public void MarkDamageDealt()
	{
		DamageDealt = true;
	}

	public void MarkZonePassed(HitZone zone)
	{
		passedZones.Add(zone.GetInstanceID());
		if (zone.ZoneType == HitZoneType.DoorFrontArea)
		{
			IsInsideProtectedZone = true;
		}
	}

	public bool HasPassedZone(HitZone zone)
	{
		return passedZones.Contains(zone.GetInstanceID());
	}

	public void Reset()
	{
		passedZones.Clear();
		IsInsideProtectedZone = false;
		DamageDealt = false;
	}
}
