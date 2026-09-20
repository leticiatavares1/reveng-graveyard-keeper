using UnityEngine;

[ExecuteInEditMode]
public class DynamicSprite : MonoBehaviour
{
	private bool _inited;

	private SpriteRenderer[] _sprs;

	private Color[] _sprs_colors;

	private ParticleSystem[] _prtcl_systems;

	private Color[] _prtcl_sys_colors;

	public DynamicSpritePreset preset;

	public bool inside_value_set;

	public float inside_value;

	public void Update()
	{
		if (!_inited)
		{
			_inited = true;
			_sprs = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
			_sprs_colors = new Color[_sprs.Length];
			for (int i = 0; i < _sprs.Length; i++)
			{
				_sprs_colors[i] = _sprs[i].color;
			}
			_prtcl_systems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			_prtcl_sys_colors = new Color[_prtcl_systems.Length];
			for (int j = 0; j < _prtcl_systems.Length; j++)
			{
				_prtcl_sys_colors[j] = _prtcl_systems[j].main.startColor.color;
			}
		}
		for (int k = 0; k < _sprs.Length; k++)
		{
			SpriteRenderer obj = _sprs[k];
			Color color = obj.color;
			if (inside_value_set && EnvironmentEngine.me.data.state == EnvironmentEngine.State.Inside)
			{
				color.a = inside_value * _sprs_colors[k].a;
			}
			else
			{
				color.a = preset.EvaluateAlpha() * _sprs_colors[k].a;
			}
			obj.color = color;
		}
		for (int l = 0; l < _prtcl_systems.Length; l++)
		{
			ParticleSystem obj2 = _prtcl_systems[l];
			Color color2 = obj2.main.startColor.color;
			if (inside_value_set && EnvironmentEngine.me.data.state == EnvironmentEngine.State.Inside)
			{
				color2.a = inside_value * _prtcl_sys_colors[l].a;
			}
			else
			{
				color2.a = preset.EvaluateAlpha() * _prtcl_sys_colors[l].a;
			}
			ParticleSystem.MainModule main = obj2.main;
			main.startColor = color2;
		}
	}
}
