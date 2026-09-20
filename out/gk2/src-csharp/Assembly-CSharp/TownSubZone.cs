using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TownSubZone : MonoBehaviour
{
	public string id;

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
