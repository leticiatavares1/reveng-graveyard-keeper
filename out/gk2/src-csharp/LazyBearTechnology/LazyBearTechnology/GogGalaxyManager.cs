using UnityEngine;

namespace LazyBearTechnology;

[DisallowMultipleComponent]
public class GogGalaxyManager : LazySingleton<GogGalaxyManager>
{
	[SerializeField]
	private string clientID;

	[SerializeField]
	private string clientSecret;
}
