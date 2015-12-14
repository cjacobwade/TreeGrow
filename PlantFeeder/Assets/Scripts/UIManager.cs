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

	[SerializeField]
	Image _backgroundImaged = null;
	Material _backgroundUIMaterial = null;

	Color _targetTopColor = Color.white;
	Color _targetBottomColor = Color.white;

	[SerializeField]
	float _colorShiftSpeed = 7f;

	[SerializeField]
	Slider[] _colorSliders = null;

	[SerializeField]
	Animator _colorPanelAnimator = null;

	bool _colorPanelShown = false;

	[SerializeField]
	Texture2D _plantExportTex = null;

	void Awake()
	{
		_overlayCanvas.gameObject.SetActive(false);

		_backgroundUIMaterial = _backgroundImaged.materialForRendering;

		_plantBoy = FindObjectOfType<PlantBoy>();
		_plantBoy.FinishedGrowingCallback += () =>
		{
			_overlayCanvas.gameObject.SetActive(true);
			StartCoroutine(ShowUIRoutine());
		};

		_targetTopColor = _backgroundUIMaterial.GetColor("_Color");
		_targetBottomColor = _backgroundUIMaterial.GetColor("_Color2");

		_colorSliders[0].value = (int)(_targetTopColor.r * 255f);
		_colorSliders[1].value = (int)(_targetTopColor.b * 255f);
		_colorSliders[2].value = (int)(_targetTopColor.g * 255f);

		_colorSliders[3].value = (int)(_targetBottomColor.r * 255f);
		_colorSliders[4].value = (int)(_targetBottomColor.b * 255f);
		_colorSliders[5].value = (int)(_targetBottomColor.g * 255f);
	}

	void Update()
	{
		Color currentTopColor = _backgroundUIMaterial.GetColor("_Color");
		_backgroundUIMaterial.SetColor("_Color", Color.Lerp(currentTopColor, _targetTopColor, Time.deltaTime * _colorShiftSpeed) );

		Color currentBottomColor = _backgroundUIMaterial.GetColor("_Color2");
		_backgroundUIMaterial.SetColor("_Color2", Color.Lerp(currentBottomColor, _targetBottomColor, Time.deltaTime * _colorShiftSpeed) );
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

		ExportScreenshot.instance.TakeShot();

		yield return null;
		_overlayCanvas.gameObject.SetActive(true);
	}

	public void OnClickMeshExport()
	{
		OBJExport.DoExport(_plantBoy.transform.parent.gameObject, _plantExportTex, false);
	}

	public void OnClickReset()
	{
		Application.LoadLevel(Application.loadedLevel);
	}

	public void OnClickColorWheel()
	{
		if(_colorPanelShown)
			_colorPanelAnimator.Play("Hide");
		else
			_colorPanelAnimator.Play("Show");

		_colorPanelShown = !_colorPanelShown;
	}

	public void OnChangeTopRed(float value)
	{
		_targetTopColor.r = value/255f;
	}

	public void OnChangeTopBlue(float value)
	{
		_targetTopColor.b = value/255f;
	}

	public void OnChangeTopGreen(float value)
	{
		_targetTopColor.g = value/255f;
	}

	public void OnChangeBottomRed(float value)
	{
		_targetBottomColor.r = value/255f;
	}

	public void OnChangeBottomGreen(float value)
	{
		_targetBottomColor.g = value/255f;
	}

	public void OnChangeBottomBlue(float value)
	{
		_targetBottomColor.b = value/255f;
	}
}
