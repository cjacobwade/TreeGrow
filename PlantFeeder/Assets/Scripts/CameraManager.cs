using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : WadeBehaviour 
{
	PlantBoy _plantBoy = null;

	[SerializeField]
	float _lerpLookAtSpeed = 7f;

	[SerializeField]
	Vector2 _rotateSpeed = Vector2.one;

	[SerializeField]
	float _maxBoundsMagnitude = 20f;

	[SerializeField]
	AnimationCurve _boundsSizeToViewDistanceMap = new AnimationCurve();

	[SerializeField]
	MinMaxF _viewDistanceRange = new MinMaxF(2.3f, 50f);

	[SerializeField]
	MinMaxF _cameraXRotationRange = new MinMaxF(-35f, 35f);

	[SerializeField]
	float _zoomLerpSpeed = 5f;

	Vector3 _targetPosition = Vector3.zero;

	void Awake()
	{
		_plantBoy = FindObjectOfType<PlantBoy>();
	}

	void Update()
	{
		// Figure out how far back we need to be to see the camera
		// Look at the center point of the tree
		// Let the player rotate horizontally and vertically around the tree

		Vector3 plantCenter = _plantBoy.Center;

		// Rotation

		Quaternion prevRotation = transform.rotation;
		transform.LookAt(plantCenter, Vector3.up);
		float desiredYRot = transform.eulerAngles.y;
		transform.rotation = prevRotation;
		transform.eulerAngles = transform.eulerAngles.SetY(desiredYRot);
		Quaternion desiredRotation = transform.rotation;
		transform.rotation = Quaternion.Lerp(prevRotation, desiredRotation, Time.deltaTime * _lerpLookAtSpeed);

		float clampEulerX = transform.eulerAngles.x;
		if(transform.eulerAngles.x > 180f)
			clampEulerX = Mathf.Max(365f + _cameraXRotationRange.min, clampEulerX);
		else
			clampEulerX = Mathf.Min(clampEulerX, _cameraXRotationRange.max);
		transform.eulerAngles = transform.eulerAngles.SetZ(0f).SetX(clampEulerX);
		// TODO: Clamp vertical rotation

		Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * Time.deltaTime;
		transform.RotateAround(plantCenter.SetY(transform.position.y), Vector3.up, _rotateSpeed.x * mouseInput.x);
		transform.RotateAround(plantCenter.SetY(transform.position.y), transform.right, _rotateSpeed.y * -mouseInput.y);

		// Zoom
		float normalizedBoundsSize = _plantBoy.renderer.bounds.size.magnitude/_maxBoundsMagnitude;
		float sizeToDistanceAlpha = _boundsSizeToViewDistanceMap.Evaluate(normalizedBoundsSize);
		Vector3 zoomDirection = (transform.position - plantCenter).SetY(0f).normalized;

		_targetPosition = plantCenter + zoomDirection * _viewDistanceRange.Lerp(sizeToDistanceAlpha);
		transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _zoomLerpSpeed);
	}
}
