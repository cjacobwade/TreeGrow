using UnityEngine;
using System.Collections;

public class AmbienceManager : SingletonBehaviour<AmbienceManager>
{
	[SerializeField] AudioSource _musicPrefab = null;

	void Start()
	{
		if(FindObjectOfType<AudioSource>() == null)
		{
			AudioSource spawnedMusic = Instantiate<AudioSource>(_musicPrefab);
			DontDestroyOnLoad(spawnedMusic.gameObject);
		}
	}
}
