using UnityEngine;

public class DecoyComponent : MonoBehaviour
{
	private Wgo attachedWgo;

	[Range(0f, 10f)]
	public float enemyRetargetRange = 5f;

	public void Initialize(Wgo wgo)
	{
		attachedWgo = wgo;
		attachedWgo.AttackPriority = 20;
		wgo.Data.HpComponent.IsImmuneToDamage = true;
	}
}
