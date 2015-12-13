using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField]
	Canvas _overlayCanvas = null;

	PlantBoy _plantBoy = null;

	[SerializeField]
	Button[] _buttons = null;

	[SerializeField]
	float _buttonScaleTime = 1.5f;

	[SerializeField]
	AnimationCurve _buttonScaleCurve = new AnimationCurve();

	void Awake()
	{
		_overlayCanvas.gameObject.SetActive(false);

		_plantBoy = FindObjectOfType<PlantBoy>();
		_plantBoy.FinishedGrowingCallback += () =>
		{
			_overlayCanvas.gameObject.SetActive(true);
			StartCoroutine(ShowUIRoutine());
		};
	}

	IEnumerator ShowUIRoutine()
	{
		for(int i = 0 ; i < _buttons.Length; i++)
			_buttons[i].transform.localScale = Vector3.zero;

		float buttonScaleTimer = 0f;
		while(buttonScaleTimer < _buttonScaleTime)
		{
			Vector3 buttonScale = Vector3.one * _buttonScaleCurve.Evaluate(buttonScaleTimer/_buttonScaleTime);
			for(int i = 0 ; i < _buttons.Length; i++)
				_buttons[i].transform.localScale = buttonScale;

			buttonScaleTimer += Time.deltaTime;
			yield return null;
		}
	}

	public void OnClickScreenshot()
	{
		StartCoroutine(TakeScreenShotRoutine());
	}

	IEnumerator TakeScreenShotRoutine()
	{
		_overlayCanvas.gameObject.SetActive(false);
		yield return null;

		ExportScreenshot.TakeShot();

		yield return null;
		_overlayCanvas.gameObject.SetActive(true);
	}

	public void OnClickMeshExport()
	{
		OBJExport.DoExport(_plantBoy.transform.parent.gameObject, false);
	}

	public void OnClickReset()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
}
