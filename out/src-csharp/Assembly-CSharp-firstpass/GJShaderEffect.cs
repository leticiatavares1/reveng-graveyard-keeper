using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GJShaderEffect : MonoBehaviour
{
	public Shader shader;

	protected Material _material;

	protected Material material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!(shader == null))
		{
			Material material = this.material;
			SetValues(material);
			Graphics.Blit(source, destination, material);
		}
	}

	protected virtual void SetValues(Material mat)
	{
	}

	private void OnDisable()
	{
		if ((bool)_material)
		{
			Object.DestroyImmediate(_material);
		}
	}
}
