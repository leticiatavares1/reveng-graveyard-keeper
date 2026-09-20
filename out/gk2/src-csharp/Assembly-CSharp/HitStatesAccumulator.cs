using System.Collections.Generic;
using UnityEngine;

public class HitStatesAccumulator : MonoBehaviour
{
	public Dictionary<ICombatEntity, WeaponHitState> hitStates = new Dictionary<ICombatEntity, WeaponHitState>();

	public HashSet<Collider> hitColliders = new HashSet<Collider>();

	public void Clear()
	{
		hitStates.Clear();
		hitColliders.Clear();
	}

	private void OnEnable()
	{
		Clear();
	}

	private void OnDisable()
	{
		Clear();
	}
}
