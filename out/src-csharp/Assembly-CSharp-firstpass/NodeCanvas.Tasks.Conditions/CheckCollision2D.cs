using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[EventReceiver(new string[] { "OnCollisionEnter2D", "OnCollisionExit2D" })]
[Name("Check Collision 2D", 0)]
[Category("System Events")]
public class CheckCollision2D : ConditionTask<Collider2D>
{
	public CollisionTypes checkType;

	public bool specifiedTagOnly;

	[TagField]
	public string objectTag = "Untagged";

	[BlackboardOnly]
	public BBParameter<GameObject> saveGameObjectAs;

	[BlackboardOnly]
	public BBParameter<Vector3> saveContactPoint;

	[BlackboardOnly]
	public BBParameter<Vector3> saveContactNormal;

	private bool stay;

	protected override string info => checkType.ToString() + (specifiedTagOnly ? (" '" + objectTag + "' tag") : "");

	protected override bool OnCheck()
	{
		if (checkType == CollisionTypes.CollisionStay)
		{
			return stay;
		}
		return false;
	}

	public void OnCollisionEnter2D(Collision2D info)
	{
		if (!specifiedTagOnly || info.gameObject.tag == objectTag)
		{
			stay = true;
			if (checkType == CollisionTypes.CollisionEnter || checkType == CollisionTypes.CollisionStay)
			{
				saveGameObjectAs.value = info.gameObject;
				saveContactPoint.value = info.contacts[0].point;
				saveContactNormal.value = info.contacts[0].normal;
				YieldReturn(value: true);
			}
		}
	}

	public void OnCollisionExit2D(Collision2D info)
	{
		if (!specifiedTagOnly || info.gameObject.tag == objectTag)
		{
			stay = false;
			if (checkType == CollisionTypes.CollisionExit)
			{
				saveGameObjectAs.value = info.gameObject;
				YieldReturn(value: true);
			}
		}
	}
}
