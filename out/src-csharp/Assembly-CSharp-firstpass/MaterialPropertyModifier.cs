using UnityEngine;

[ExecuteInEditMode]
public class MaterialPropertyModifier : MonoBehaviour
{
	protected delegate void PropertyModifier(MaterialPropertyBlock mat);

	private MaterialPropertyBlock _properties;

	private MaterialPropertyBlock _tmp_properties;

	protected void DoUpdateRenderer(Renderer r, PropertyModifier modify_delegate)
	{
		if (_properties == null)
		{
			_properties = new MaterialPropertyBlock();
			_tmp_properties = new MaterialPropertyBlock();
		}
		if (!(r == null))
		{
			_properties.Clear();
			r.GetPropertyBlock(_tmp_properties);
			Texture texture = _tmp_properties.GetTexture("_MainTex");
			if (texture != null)
			{
				_properties.SetTexture("_MainTex", texture);
			}
			else
			{
				Debug.Log("Main tex is null");
			}
			modify_delegate?.Invoke(_properties);
			r.SetPropertyBlock(_properties);
		}
	}

	public virtual void UpdateRenderer()
	{
	}

	public virtual void OnBecameVisible()
	{
		UpdateRenderer();
	}
}
