using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MapZone : MonoBehaviour
{
	[SerializeField]
	private string id;

	private Collider coll;

	public string Id => id;

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
