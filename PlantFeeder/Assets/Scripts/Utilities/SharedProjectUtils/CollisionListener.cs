using UnityEngine;
using System.Collections;

public class CollisionListener : MonoBehaviour 
{
	public System.Action<Collision> CollisionEnterCallback = delegate {};
	public System.Action<Collision> CollisionExitCallback = delegate {};
	public System.Action<Collision> CollisionStayCallback = delegate {};

	public System.Action<Collider> TriggerEnterCallback = delegate {};
	public System.Action<Collider> TriggerExitCallback = delegate {};
	public System.Action<Collider> TriggerStayCallback = delegate {};

	void OnCollisionEnter( Collision col )
	{
		CollisionEnterCallback( col );
	}

	void OnCollisionExit( Collision col )
	{
		CollisionExitCallback( col );
	}

	void OnCollisionStay( Collision col )
	{
		CollisionStayCallback( col );
	}

	void OnTriggerEnter( Collider col )
	{
		TriggerEnterCallback( col );
	}

	void OnTriggerExit( Collider col )
	{
		TriggerExitCallback( col );
	}

	void OnTriggerStay( Collider col )
	{
		TriggerStayCallback( col );
	}
}
