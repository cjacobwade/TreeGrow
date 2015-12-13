using UnityEngine;
using System.Collections;

public class Jiggle : WadeBehaviour 
{
	[SerializeField]
	bool _canJiggle = true;

	[SerializeField]
	AnimationCurve _scaleJiggleCurve = new AnimationCurve();

	Vector3 _initScale = Vector3.zero;

	[SerializeField]
	float _jiggleTime = 0.7f;

	[SerializeField]
	MinMaxF _maxJiggleScaleRange = new MinMaxF(1.05f, 1.2f);

	[SerializeField]
	float _maxJiggleStrength = 10f;

	Coroutine _jiggleRoutine = null;

	void Awake()
	{
		_initScale = transform.localScale;
	}

	public void SetCanJiggle(bool setCanJiggle)
	{
		_canJiggle = setCanJiggle;
	}

	void OnMouseDown()
	{
		if(_canJiggle)
		{
			if(_jiggleRoutine != null)
				StopCoroutine(_jiggleRoutine);
			_jiggleRoutine = StartCoroutine(JiggleRoutine());
		}
	}

	IEnumerator JiggleRoutine()
	{
		Vector3 startJiggleScale = transform.localScale;
		Vector3 maxJiggleScale = transform.localScale * _maxJiggleScaleRange.Random;
		maxJiggleScale = Vector3.ClampMagnitude(maxJiggleScale, _maxJiggleStrength);

		float halfLife = _jiggleTime/2f;
		float jiggleTimer = 0f;
		while(jiggleTimer < _jiggleTime)
		{
			// Use this applied scale idea so we can keep clicking on the thing and it will eventually get back to the size it was meant to be
			Vector3 appliedScale = Vector3.Lerp(startJiggleScale, _initScale, jiggleTimer/_jiggleTime);
			transform.localScale = Vector3.LerpUnclamped(appliedScale, maxJiggleScale, _scaleJiggleCurve.Evaluate(jiggleTimer/_jiggleTime));

			jiggleTimer += Time.deltaTime;
			yield return null;
		}
		transform.localScale = _initScale;
		_jiggleRoutine = null;
	}
}
