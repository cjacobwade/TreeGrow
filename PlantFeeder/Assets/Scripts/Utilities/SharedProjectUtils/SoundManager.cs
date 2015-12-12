using UnityEngine;
using System.Collections;

public class SoundManager : SingletonBehaviour<SoundManager>
{
	public static AudioSource PlaySound( AudioSource audioSource )
	{
		// Each AudioSource prefab gets its own pool of objects that match its settings.
		audioSource.CreatePool();
		AudioSource spawnedAudio = audioSource.Spawn<AudioSource>();

		// We mark the audio source as don't destroy on load so we can reuse our audio source
		// pool between scenes. This also allows sounds to continue to be played during scene loads.
		DontDestroyOnLoad( spawnedAudio.gameObject );

		instance.StartCoroutine( instance.PlayAudioRoutine( spawnedAudio ) );
		return spawnedAudio;
	}

	IEnumerator PlayAudioRoutine( AudioSource audioSource )
	{
		audioSource.Play();
		yield return new WaitForSeconds( audioSource.clip.length );
		audioSource.Stop();
		audioSource.Recycle();
	}
}
