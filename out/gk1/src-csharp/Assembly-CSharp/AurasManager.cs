using UnityEngine;

public class AurasManager : MonoBehaviour
{
	public void Update()
	{
		DoAurasCalculation();
	}

	private void DoAurasCalculation()
	{
		AuraEmitter.ProcessAurasCalculation();
	}
}
