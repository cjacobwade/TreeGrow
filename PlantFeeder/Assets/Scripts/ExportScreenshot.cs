using UnityEngine;
using System.Collections;

public class ExportScreenshot : MonoBehaviour 
{
	public static void TakeShot()
	{
		Application.CaptureScreenshot("BeautyShot_" + System.DateTime.Now.ToString("s"));
	}
}
