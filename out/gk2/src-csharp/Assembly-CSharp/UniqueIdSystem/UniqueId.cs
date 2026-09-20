using UnityEngine;

namespace UniqueIdSystem;

[DisallowMultipleComponent]
[ExecuteAlways]
public class UniqueId : MonoBehaviour
{
	[SerializeField]
	private string uid = "";

	[SerializeField]
	private long assignedTimestampUtc;

	private IUniqueIdUser uniqueIdUser;

	public string Uid => uid;

	public long AssignedTimestampUtc => assignedTimestampUtc;
}
