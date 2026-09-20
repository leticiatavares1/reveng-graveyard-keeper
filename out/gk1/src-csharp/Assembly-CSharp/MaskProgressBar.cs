using UnityEngine;

public class MaskProgressBar : MonoBehaviour
{
	public SpriteMask mask;

	public IngameProgressBar.ScaleType scale_type;

	public float min_scale;

	public float max_scale;

	public string game_res;

	private WorldGameObject _wgo;

	public void UpdateBar()
	{
		if (HasWGO())
		{
			float param = _wgo.GetParam(game_res);
			float num = min_scale + (max_scale - min_scale) * param;
			Vector3 localScale = mask.transform.localScale;
			switch (scale_type)
			{
			case IngameProgressBar.ScaleType.Vertical:
				mask.transform.localScale = new Vector3(localScale.x, num, localScale.z);
				break;
			case IngameProgressBar.ScaleType.Horizontal:
				mask.transform.localScale = new Vector3(num, localScale.y, localScale.z);
				break;
			}
		}
	}

	private bool HasWGO()
	{
		if (_wgo == null)
		{
			InitWGO();
			if (_wgo == null)
			{
				return false;
			}
		}
		return true;
	}

	private void InitWGO()
	{
		WorldObjectPart component = GetComponent<WorldObjectPart>();
		if (!(component == null))
		{
			_wgo = component.parent;
		}
	}
}
