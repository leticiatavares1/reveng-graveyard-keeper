using Unity.Multiplayer.Tools.NetStats;
using UnityEngine.Scripting;

public class _003CNetStats_TypeRegistration_003E
{
	[Preserve]
	static void Run()
	{
		MetricIdTypeLibrary.RegisterType<CustomMetric>();
	}
}
