using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Wgo))]
public class SpawnComponent : MonoBehaviour
{
	private Wgo wgo;

	public SpawnConfiguration SpawnConfiguration
	{
		get
		{
			return Wgo.MainWgoPart.SpawnConfiguration;
		}
		set
		{
			Wgo.MainWgoPart.SpawnConfiguration = value;
		}
	}

	public Wgo Wgo
	{
		get
		{
			if (wgo == null)
			{
				wgo = GetComponent<Wgo>();
			}
			return wgo;
		}
	}

	public SpawnWgoComponent SpawnWgoComponent => Wgo.Data.SpawnWGOComponent;
}
