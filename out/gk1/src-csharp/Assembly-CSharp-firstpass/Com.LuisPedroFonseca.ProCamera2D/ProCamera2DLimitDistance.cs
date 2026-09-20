using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-limit-distance/")]
public class ProCamera2DLimitDistance : BasePC2D, IPositionDeltaChanger
{
	public static string ExtensionName = "Limit Distance";

	public bool LimitHorizontalCameraDistance = true;

	public float MaxHorizontalTargetDistance = 0.8f;

	public bool LimitVerticalCameraDistance = true;

	public float MaxVerticalTargetDistance = 0.8f;

	private int _pdcOrder = 2000;

	public int PDCOrder
	{
		get
		{
			return _pdcOrder;
		}
		set
		{
			_pdcOrder = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		ProCamera2D.Instance.AddPositionDeltaChanger(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemovePositionDeltaChanger(this);
	}

	public Vector3 AdjustDelta(float deltaTime, Vector3 originalDelta)
	{
		if (!base.enabled)
		{
			return originalDelta;
		}
		float num = Vector3H(originalDelta);
		bool flag = false;
		if (LimitHorizontalCameraDistance)
		{
			float num2 = ProCamera2D.ScreenSizeInWorldCoordinates.x / 2f * MaxHorizontalTargetDistance;
			if (ProCamera2D.CameraTargetPosition.x > num + Vector3H(ProCamera2D.LocalPosition) + num2)
			{
				num = ProCamera2D.CameraTargetPosition.x - (Vector3H(ProCamera2D.LocalPosition) + num2);
				flag = true;
			}
			else if (ProCamera2D.CameraTargetPosition.x < num + Vector3H(ProCamera2D.LocalPosition) - num2)
			{
				num = ProCamera2D.CameraTargetPosition.x - (Vector3H(ProCamera2D.LocalPosition) - num2);
				flag = true;
			}
		}
		float num3 = Vector3V(originalDelta);
		bool flag2 = false;
		if (LimitVerticalCameraDistance)
		{
			float num4 = ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f * MaxVerticalTargetDistance;
			if (ProCamera2D.CameraTargetPosition.y > num3 + Vector3V(ProCamera2D.LocalPosition) + num4)
			{
				num3 = ProCamera2D.CameraTargetPosition.y - (Vector3V(ProCamera2D.LocalPosition) + num4);
				flag2 = true;
			}
			else if (ProCamera2D.CameraTargetPosition.y < num3 + Vector3V(ProCamera2D.LocalPosition) - num4)
			{
				num3 = ProCamera2D.CameraTargetPosition.y - (Vector3V(ProCamera2D.LocalPosition) - num4);
				flag2 = true;
			}
		}
		ProCamera2D.CameraTargetPositionSmoothed = new Vector2(flag ? (Vector3H(ProCamera2D.LocalPosition) + num) : ProCamera2D.CameraTargetPositionSmoothed.x, flag2 ? (Vector3V(ProCamera2D.LocalPosition) + num3) : ProCamera2D.CameraTargetPositionSmoothed.y);
		return VectorHV(num, num3);
	}
}
