using Pathfinding;
using UnityEngine;

public class AstarPathTester : MonoBehaviour
{
	public GameObject to;

	[ContextMenu("Search path")]
	private void SearchPath()
	{
		Seeker seeker = base.gameObject.GetComponent<Seeker>();
		if (seeker == null)
		{
			seeker = base.gameObject.AddComponent<Seeker>();
		}
		seeker.StartPath(base.transform.position, to.transform.position, OnPathComplete);
	}

	private void OnPathComplete(Path p)
	{
		Debug.Log("On path complete. Error = " + p.error + ", state = " + p.CompleteState);
	}
}
