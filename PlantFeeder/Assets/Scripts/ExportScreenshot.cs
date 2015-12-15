using UnityEngine;
using System.Collections;
using System.IO;

public class ExportScreenshot : SingletonBehaviour<ExportScreenshot> 
{
	// You need to register your game or application in Twitter to get cosumer key and secret.
	// Go to this page for registration: http://dev.twitter.com/apps/new
	public string CONSUMER_KEY = "6uXkKpF4cOgSHfVzgQdr7i4iS";
	public string CONSUMER_SECRET = "oKas9491lRUX98IYqJI38YodgVWJuZhGMH2gFWpRqPsBTTjiOo";

	// You need to save access token and secret for later use.
	// You can keep using them whenever you need to access the user's Twitter account. 
	// They will be always valid until the user revokes the access to your application.
	const string PLAYER_PREFS_TWITTER_USER_ID           = "TwitterUserID";
	const string PLAYER_PREFS_TWITTER_USER_SCREEN_NAME  = "TwitterUserScreenName";
	const string PLAYER_PREFS_TWITTER_USER_TOKEN        = "TwitterUserToken";
	const string PLAYER_PREFS_TWITTER_USER_TOKEN_SECRET = "TwitterUserTokenSecret";

	Twitter.RequestTokenResponse m_RequestTokenResponse;
	Twitter.AccessTokenResponse m_AccessTokenResponse;

	string m_PIN = "3078734";

	IEnumerator Start()
	{
//		LoadTwitterUserInfo();
//
//		yield return StartCoroutine(Twitter.API.GetRequestToken(CONSUMER_KEY, CONSUMER_SECRET,
//			new Twitter.RequestTokenCallback(this.OnRequestTokenCallback)));
//
//		StartCoroutine(Twitter.API.GetAccessToken(CONSUMER_KEY, CONSUMER_SECRET, m_RequestTokenResponse.Token, m_PIN,
//			new Twitter.AccessTokenCallback(this.OnAccessTokenCallback)));

		yield return null;
	}

	public void TakeShot()
	{
		string fileName = "BeautyShot_" + Random.Range(0, 1000000).ToString() + ".png";
		Application.CaptureScreenshot(fileName);

//		StartCoroutine(Twitter.API.PostTweet(File.ReadAllBytes(fileName), CONSUMER_KEY, CONSUMER_SECRET, m_AccessTokenResponse,
//			new Twitter.PostTweetCallback(this.OnPostTweet)));
	}

	void LoadTwitterUserInfo()
	{
		m_AccessTokenResponse = new Twitter.AccessTokenResponse();

		m_AccessTokenResponse.UserId        = PlayerPrefs.GetString(PLAYER_PREFS_TWITTER_USER_ID);
		m_AccessTokenResponse.ScreenName    = PlayerPrefs.GetString(PLAYER_PREFS_TWITTER_USER_SCREEN_NAME);
		m_AccessTokenResponse.Token         = PlayerPrefs.GetString(PLAYER_PREFS_TWITTER_USER_TOKEN);
		m_AccessTokenResponse.TokenSecret   = PlayerPrefs.GetString(PLAYER_PREFS_TWITTER_USER_TOKEN_SECRET);

		if (!string.IsNullOrEmpty(m_AccessTokenResponse.Token) &&
			!string.IsNullOrEmpty(m_AccessTokenResponse.ScreenName) &&
			!string.IsNullOrEmpty(m_AccessTokenResponse.Token) &&
			!string.IsNullOrEmpty(m_AccessTokenResponse.TokenSecret))
		{
			string log = "LoadTwitterUserInfo - succeeded";
			log += "\n    UserId : " + m_AccessTokenResponse.UserId;
			log += "\n    ScreenName : " + m_AccessTokenResponse.ScreenName;
			log += "\n    Token : " + m_AccessTokenResponse.Token;
			log += "\n    TokenSecret : " + m_AccessTokenResponse.TokenSecret;
			print(log);
		}
	}

	void OnRequestTokenCallback(bool success, Twitter.RequestTokenResponse response)
	{
		if (success)
		{
			string log = "OnRequestTokenCallback - succeeded";
			log += "\n    Token : " + response.Token;
			log += "\n    TokenSecret : " + response.TokenSecret;
			print(log);

			m_RequestTokenResponse = response;

			Twitter.API.OpenAuthorizationPage(response.Token);
		}
		else
		{
			print("OnRequestTokenCallback - failed.");
		}
	}

	void OnAccessTokenCallback(bool success, Twitter.AccessTokenResponse response)
	{
		if (success)
		{
			string log = "OnAccessTokenCallback - succeeded";
			log += "\n    UserId : " + response.UserId;
			log += "\n    ScreenName : " + response.ScreenName;
			log += "\n    Token : " + response.Token;
			log += "\n    TokenSecret : " + response.TokenSecret;
			print(log);

			m_AccessTokenResponse = response;

			PlayerPrefs.SetString(PLAYER_PREFS_TWITTER_USER_ID, response.UserId);
			PlayerPrefs.SetString(PLAYER_PREFS_TWITTER_USER_SCREEN_NAME, response.ScreenName);
			PlayerPrefs.SetString(PLAYER_PREFS_TWITTER_USER_TOKEN, response.Token);
			PlayerPrefs.SetString(PLAYER_PREFS_TWITTER_USER_TOKEN_SECRET, response.TokenSecret);
		}
		else
		{
			print("OnAccessTokenCallback - failed.");
		}
	}

	void OnPostTweet(bool success)
	{
		print("OnPostTweet - " + (success ? "succedded." : "failed."));
	}
}
