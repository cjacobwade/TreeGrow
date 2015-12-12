using UnityEngine;
using System.Collections;

public class DestroySelf : MonoBehaviour
{
	[SerializeField] bool _destroyOnStart = true;
	public float lifetime = 1.0f;

	void Awake()
	{
		if ( _destroyOnStart )
		{
			Destroy( gameObject, lifetime );
		}
	}

	void Kill()
	{
		Destroy( gameObject );
	}

	void TimedKill( float waitTime )
	{
		Destroy( gameObject, waitTime );
	}
}
