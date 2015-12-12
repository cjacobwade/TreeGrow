using UnityEngine;
using System.Collections;
using System;

public class WadeBehaviour : MonoBehaviour
{
	[HideInInspector] protected Transform _transform = null;
	public virtual new Transform transform
	{
		get
		{
			if (!_transform)
			{
				_transform = GetComponent<Transform>();
			}

			return _transform;
		}
	}

	[HideInInspector] protected Renderer _renderer = null;
	public virtual new Renderer renderer
	{
		get
		{
			if (!_renderer)
			{
				_renderer = GetComponent<Renderer>();
			}

			return _renderer;
		}
	}

	[HideInInspector] protected Camera _camera = null;
	public virtual new Camera camera
	{
		get
		{
			if (!_camera)
			{
				_camera = GetComponent<Camera>();
			}

			return _camera;
		}
	}

	[HideInInspector] protected Collider _collider = null;
	public virtual new Collider collider
	{
		get
		{
			if (!_collider)
			{
				_collider = GetComponent<Collider>();
			}

			return _collider;
		}
	}

	[HideInInspector] protected Collider2D _collider2D = null;
	public virtual new Collider2D collider2D
	{
		get
		{
			if (!_collider2D)
			{
				_collider2D = GetComponent<Collider2D>();
			}

			return _collider2D;
		}
	}

	[HideInInspector] protected Rigidbody _rigidbody = null;
	public virtual new Rigidbody rigidbody
	{
		get
		{
			if (!_rigidbody)
			{
				_rigidbody = GetComponent<Rigidbody>();
			}

			return _rigidbody;
		}
	}

	[HideInInspector] protected Rigidbody2D _rigidbody2D = null;
	public virtual new Rigidbody2D rigidbody2D
	{
		get
		{
			if (!_rigidbody2D)
			{
				_rigidbody2D = GetComponent<Rigidbody2D>();
			}

			return _rigidbody2D;
		}
	}

	[HideInInspector] protected Animator _animator = null;
	public virtual Animator animator
	{
		get
		{
			if (!_animator)
			{
				_animator = GetComponent<Animator>();
			}

			return _animator;
		}
	}

	protected Coroutine WaitForTruth(Func<bool> condition)
	{
		return StartCoroutine(WaitForTruthRoutine(condition));
	}

	IEnumerator WaitForTruthRoutine(Func<bool> condition)
	{
		while (!condition())
			yield return null;
	}

	protected bool _hasQuit = false;

	protected virtual void OnApplicationQuit()
	{
		_hasQuit = true;
	}
}
