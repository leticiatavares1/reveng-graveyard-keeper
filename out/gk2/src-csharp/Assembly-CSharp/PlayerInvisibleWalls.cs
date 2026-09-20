using System;
using UnityEngine;

public class PlayerInvisibleWalls : MonoBehaviour
{
	public const float INVISIBLE_WALL_COLLIDER_HALF_WIDTH = 0.5f;

	private const int COLLIDERS_LIMIT_COUNT = 10;

	private const float CAST_SPHERE_RADIUS = 0.01f;

	private const float PLAYER_COLLIDER_RADIUS_X = 0.25f;

	private const float PLAYER_COLLIDER_RADIUS_Z_PLUS = 0.2f;

	private const float PLAYER_COLLIDER_RADIUS_Z_MINUS = 0.1f;

	[SerializeField]
	private float edgeCheckDistanceZPlus = 0.4f;

	[SerializeField]
	private float edgeCheckDistanceZMinus = 0.1f;

	[SerializeField]
	private float edgeCheckDistanceX = 0.25f;

	[SerializeField]
	private float edgeCheckDistanceDiagonalZPlus = 0.4f;

	[SerializeField]
	private float edgeCheckDistanceDiagonalZMinus = 0.1f;

	[SerializeField]
	private float edgeCheckDistanceDiagonalX = 0.25f;

	[SerializeField]
	private float edgeRaycastLength = 0.6f;

	[SerializeField]
	private float raycastYOffset = 0.05f;

	public Transform xPlusWall;

	public Transform xMinusWall;

	public Transform zPlusWall;

	public Transform zMinusWall;

	public Transform xPlusZMinusWall;

	public Transform xPlusZPlusWall;

	public Transform xMinusZMinusWall;

	public Transform xMinusZPlusWall;

	private static RaycastHit[] raycastResults = new RaycastHit[10];

	private bool isWallsCheckEnabled;

	public bool IsWallsCheckEnabled
	{
		get
		{
			return isWallsCheckEnabled;
		}
		set
		{
			isWallsCheckEnabled = value;
		}
	}

	public void UpdateEdgeWallsState(Vector3 playerPosition)
	{
		if (isWallsCheckEnabled)
		{
			Vector3 rayDirection = new Vector3(0f, 0f - edgeRaycastLength);
			Vector3 vector = playerPosition + Vector3.up * raycastYOffset;
			Vector3 roundedPosY = VisualConsts.GetRoundedPosY(vector);
			float num = 0.75f;
			float z = 0.7f;
			float num2 = 0.6f;
			float num3 = Mathf.Sqrt(Mathf.Pow(0.25f, 2f) / 2f) + 0.5f;
			float z2 = Mathf.Sqrt(Mathf.Pow(0.2f, 2f) / 2f) + 0.5f;
			float num4 = Mathf.Sqrt(Mathf.Pow(0.1f, 2f) / 2f) + 0.5f;
			xPlusWall.position = roundedPosY + new Vector3(num, 0f);
			xMinusWall.position = roundedPosY + new Vector3(0f - num, 0f);
			zPlusWall.position = roundedPosY + new Vector3(0f, 0f, z);
			zMinusWall.position = roundedPosY + new Vector3(0f, 0f, 0f - num2);
			xPlusZMinusWall.position = roundedPosY + new Vector3(num3, 0f, 0f - num4);
			xPlusZPlusWall.position = roundedPosY + new Vector3(num3, 0f, z2);
			xMinusZPlusWall.position = roundedPosY + new Vector3(0f - num3, 0f, z2);
			xMinusZMinusWall.position = roundedPosY + new Vector3(0f - num3, 0f, 0f - num4);
			xPlusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(edgeCheckDistanceX, 0f), rayDirection));
			xMinusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(0f - edgeCheckDistanceX, 0f), rayDirection));
			zPlusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(0f, 0f, edgeCheckDistanceZPlus), rayDirection));
			zMinusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(0f, 0f, 0f - edgeCheckDistanceZMinus), rayDirection));
			xPlusZMinusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(edgeCheckDistanceDiagonalX, 0f, 0f - edgeCheckDistanceDiagonalZMinus), rayDirection));
			xPlusZPlusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(edgeCheckDistanceDiagonalX, 0f, edgeCheckDistanceDiagonalZPlus), rayDirection));
			xMinusZPlusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(0f - edgeCheckDistanceDiagonalX, 0f, edgeCheckDistanceDiagonalZPlus), rayDirection));
			xMinusZMinusWall.gameObject.SetActive(!IsRaycastHitWalkableGround(vector + new Vector3(0f - edgeCheckDistanceDiagonalX, 0f, 0f - edgeCheckDistanceDiagonalZMinus), rayDirection));
		}
	}

	public void ChangeWallsVisibility(bool visible)
	{
		xPlusWall.gameObject.SetActive(visible);
		xMinusWall.gameObject.SetActive(visible);
		zPlusWall.gameObject.SetActive(visible);
		zMinusWall.gameObject.SetActive(visible);
		xPlusZMinusWall.gameObject.SetActive(visible);
		xPlusZPlusWall.gameObject.SetActive(visible);
		xMinusZPlusWall.gameObject.SetActive(visible);
		xMinusZMinusWall.gameObject.SetActive(visible);
	}

	private bool IsRaycastHitWalkableGround(Vector3 startRayPoint, Vector3 rayDirection)
	{
		Array.Clear(raycastResults, 0, 10);
		bool num = Physics.SphereCastNonAlloc(startRayPoint, 0.01f, rayDirection, raycastResults, edgeRaycastLength, 6144) > 0;
		Color color = (num ? Color.white : Color.red);
		Debug.DrawRay(startRayPoint, rayDirection, color);
		return num;
	}
}
