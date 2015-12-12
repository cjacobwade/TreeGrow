using UnityEngine;
using System;
using System.Collections;

public class Invoker : AutoSingletonBehaviour<Invoker>
{
	/// <summary>
	/// Calls a function after a delay.
	/// </summary>
	/// <param name="delay">The amount of time (in seconds) to wait before invoking action.</param>
	/// <param name="action">The Action to be invoked. Must be a function that takes no paremeters or return value.</param>
	/// <remarks>
	/// This is a type-safe replacement for Unity's Invoke() mechanism. Unlike Invoke() which is stringly typed, this
	/// can be type-checked by the compiler, so you don't have to wait until runtime to find out if you typed in the
	/// function name wrong.
	/// </remarks>
	public static void Once( float delay, Action action )
	{
		instance._Once( delay, action );
	}

	private void _Once( float delay, Action action )
	{
		StartCoroutine( OnceRoutine( delay, action ) );
	}

	private IEnumerator OnceRoutine( float delay, Action action )
	{
		yield return new WaitForSeconds( delay );
		action();
	}
}
