using UnityEngine;

public abstract class CustomScript : MonoBehaviour
{
	public bool started;

	public string script_name;

	protected bool is_global;

	public WorldGameObject current_interractor;

	public virtual void TerminateMe()
	{
		base.enabled = false;
		NGUITools.Destroy(base.gameObject);
		Debug.Log("<color=orange>Terminating Script:</color> " + script_name + ", is_global = " + is_global, this);
	}

	public void ForceStart()
	{
		started = true;
	}
}
