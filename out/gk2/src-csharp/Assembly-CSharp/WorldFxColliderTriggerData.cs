using System;
using UnityEngine;

[Serializable]
public class WorldFxColliderTriggerData : ColliderTriggerDataBase
{
	[SerializeField]
	private string fxName;

	[SerializeField]
	private Vector3 size = Vector3.one;

	public Transform PlayTarget { get; set; }

	protected override bool IsSetupCompleted()
	{
		return !string.IsNullOrEmpty(fxName);
	}

	protected override void TriggerSetAction()
	{
		Debug.Log("WorldFxColliderTriggerData play FX:[" + fxName + "]");
		WorldFX.Spawn(PlayTarget.position, fxName, null, size);
	}

	protected override void TriggerResetAction()
	{
	}
}
