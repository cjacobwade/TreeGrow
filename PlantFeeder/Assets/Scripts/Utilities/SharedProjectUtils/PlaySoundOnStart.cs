using UnityEngine;
using System.Collections;

public class PlaySoundOnStart : MonoBehaviour
{
	public AudioSource _audio = null;

	void Start()
	{
		SoundManager.PlaySound( _audio );
	}
}
