using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/ConstructorPartBoundsConfig", fileName = "ConstructorPartBoundsConfig")]
public class ConstructorPartBoundsConfig : LazySingletonSO<ConstructorPartBoundsConfig>
{
	[SerializeField]
	private ConstructorPartBoundsCollection boundsCollection = new ConstructorPartBoundsCollection();

	public ConstructorPartBoundsCollection BoundsCollection => boundsCollection;
}
