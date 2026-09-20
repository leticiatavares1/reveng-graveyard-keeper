using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[RequireComponent(typeof(GroundLayoutObjectWithTMP))]
public class DevZone : MonoBehaviour
{
	[SerializeField]
	private List<Wgo> wgos;

	private List<WorldZone> worldZones;

	public List<Wgo> Wgos
	{
		get
		{
			if (wgos == null || wgos.Count == 0)
			{
				wgos = GetComponentsInChildren<Wgo>(includeInactive: true).ToList();
			}
			return wgos;
		}
	}

	public List<WorldZone> WorldZones
	{
		get
		{
			if (worldZones == null || worldZones.Count == 0)
			{
				worldZones = GetComponentsInChildren<WorldZone>(includeInactive: true).ToList();
			}
			return worldZones;
		}
	}
}
