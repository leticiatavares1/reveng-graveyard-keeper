using System;

namespace DLCRefugees;

[Serializable]
public class SimplifiedItem
{
	public string id;

	public int count;

	public float give_energy;

	public SimplifiedItem(string id, int count, float give_energy)
	{
		this.id = id;
		this.count = count;
		this.give_energy = give_energy;
	}
}
