using UnityEngine;
using System.Collections;
using System.IO;

public class ExportScreenshot : SingletonBehaviour<ExportScreenshot> 
{
	// You need to register your game or application in Twitter to get cosumer key and secret.
	// Go to this page for registration: http://dev.twitter.com/apps/new
	string CONSUMER_KEY = "GpHVfpb6Ki0bZGihFnQZ7pkWx";
	string CONSUMER_SECRET = "qWjiCnkJdJMAseLi3Bnk2FlDVzfzvM1HCzME7mCnHlvRSgUsgt";

	string ACCESS_TOKEN = "4552294213-bJFXp08DasmAMVSR7UA57hgNlGoHCtEc45FUl8T";
	string ACCESS_TOKEN_SECRET = "0We3hyZV50pz8M0NNZ0X6hSDLxqGu3uUkoIgcY2mrT1P6";

	public void TakeShot()
	{
		string fileName = "BeautyShot_" + Random.Range(0, 1000000).ToString() + ".png";
		Application.CaptureScreenshot(fileName);

		StartCoroutine(Twitter.API.PostTweet(GetScreenshotBytes(), CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET, new Twitter.PostTweetCallback(this.OnPostTweet)));
	}

	byte[] GetScreenshotBytes()
	{
		Camera cam = Camera.main;
		int resWidth = cam.pixelWidth;
		int resHeight = cam.pixelHeight;

		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		cam.targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
		cam.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		cam.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		return screenShot.EncodeToPNG();
	}

	void OnPostTweet(bool success)
	{
		print("OnPostTweet - " + (success ? "succedded." : "failed."));
	}
}
