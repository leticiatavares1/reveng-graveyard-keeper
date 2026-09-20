using UnityEngine;

public class LUTMorpher : MonoBehaviour
{
	public AmplifyColorEffect lut;

	public float speed = 0.2f;

	private int _cur_morph_dir;

	private Texture _lut_tx;

	private bool _lut;

	private Texture _lut_override_tx;

	private bool _lut_override;

	private Texture _next_lut_tx;

	private bool _next_lut;

	private const float DEFAULT_SPEED = 0.2f;

	public void SetMainLUT(Texture texture, float morph_speed = 0.2f)
	{
		Debug.Log("SetMainLUT, texture = " + ((texture == null) ? "null" : texture.name) + ", morph_speed = " + morph_speed);
		_lut_tx = texture;
		speed = morph_speed;
		if (!_lut_override)
		{
			TryStartMorph(texture);
		}
	}

	public void SetOverrideLUT(Texture texture, float morph_speed = 0.2f)
	{
		Debug.Log("SetOverrideLUT, texture = " + ((texture == null) ? "null" : texture.name) + ", morph_speed = " + morph_speed);
		_lut_override_tx = texture;
		_lut_override = texture != null;
		if (_cur_morph_dir == 0 && texture == null)
		{
			texture = _lut_tx;
		}
		speed = morph_speed;
		TryStartMorph(texture);
	}

	private void TryStartMorph(Texture texture)
	{
		if (_cur_morph_dir == 0)
		{
			StartMorph(texture, (!((double)lut.BlendAmount > 0.5)) ? 1 : (-1));
			return;
		}
		_next_lut_tx = texture;
		_next_lut = true;
	}

	public void Update()
	{
		if (_cur_morph_dir == 0)
		{
			return;
		}
		float num = lut.BlendAmount + speed * Time.deltaTime * (float)_cur_morph_dir;
		if (num > 1f)
		{
			num = 1f;
			_cur_morph_dir = 0;
			if (_next_lut)
			{
				_next_lut = false;
				StartMorph(_next_lut_tx, -1);
				_next_lut_tx = null;
			}
		}
		else if (num < 0f)
		{
			num = 0f;
			_cur_morph_dir = 0;
			if (_next_lut)
			{
				_next_lut = false;
				StartMorph(_next_lut_tx, 1);
				_next_lut_tx = null;
			}
		}
		lut.BlendAmount = num;
	}

	private void StartMorph(Texture target_tx, int dir)
	{
		Debug.Log("StartMorph " + ((target_tx == null) ? "null" : target_tx.name) + ", dir = " + dir);
		if (_cur_morph_dir != 0)
		{
			Debug.LogError("LUTMorpher: Can't start morph when previous is not over");
			return;
		}
		_cur_morph_dir = dir;
		if (dir > 0)
		{
			lut.LutBlendTexture = target_tx;
		}
		else
		{
			lut.LutTexture = target_tx;
		}
	}
}
