using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : WadeBehaviour 
{
	PlantBoy _plantBoy = null;

	[SerializeField]
	float _lerpLookAtSpeed = 7f;

	[SerializeField]
	float _rotateSpeed = 50f;

	[SerializeField]
	float _maxBoundsMagnitude = 20f;

	[SerializeField]
	AnimationCurve _boundsSizeToViewDistanceMap = new AnimationCurve();

	[SerializeField]
	MinMaxF _viewDistanceRange = new MinMaxF(2.3f, 50f);

	[SerializeField]
	float _zoomLerpSpeed = 5f;

	[SerializeField]
	float _heightOffset = 3f;

	[SerializeField]
	float _autoRotateSpeed = 15f;

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
		transform.eulerAngles = prevRotation.eulerAngles.SetY(transform.eulerAngles.y);
		Quaternion desiredRotation = transform.rotation;
		transform.rotation = Quaternion.Lerp(prevRotation, desiredRotation, Time.deltaTime * _lerpLookAtSpeed);

		float rotateSpeed = _plantBoy.IsGrowing ? _autoRotateSpeed : _rotateSpeed * Input.GetAxis("Horizontal");
		rotateSpeed *= Time.deltaTime;
		transform.RotateAround(plantCenter.SetY(transform.position.y), Vector3.up, rotateSpeed);

		// Zoom
		float normalizedBoundsSize = _plantBoy.renderer.bounds.size.magnitude/_maxBoundsMagnitude;
		float sizeToDistanceAlpha = _boundsSizeToViewDistanceMap.Evaluate(normalizedBoundsSize);
		Vector3 zoomDirection = (transform.position - plantCenter).SetY(_heightOffset).normalized;

		_targetPosition = plantCenter + zoomDirection * _viewDistanceRange.Lerp(sizeToDistanceAlpha);
		transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _zoomLerpSpeed);
	}
}
