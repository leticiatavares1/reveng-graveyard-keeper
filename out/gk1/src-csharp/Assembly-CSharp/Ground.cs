using UnityEngine;

[ExecuteInEditMode]
public class Ground : MonoBehaviour
{
	public enum GroudType
	{
		None = 0,
		Grass = 1,
		Dirt = 2,
		Sand = 3,
		Swamp = 4,
		Rocks = 5,
		Shit = 6,
		Rug = 100,
		Wood = 101
	}

	public GroudType type;
}
