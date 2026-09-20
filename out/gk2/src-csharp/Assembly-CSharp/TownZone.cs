using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TownZone : MonoBehaviour
{
	private Collider coll;

	public Collider Collider
	{
		get
		{
			if (coll == null)
			{
				coll = GetComponent<Collider>();
			}
			return coll;
		}
	}
}
