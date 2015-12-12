using UnityEngine;

/// <summary>
/// A utility class for creating a singleton object that will automatically spawn.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AutoSingletonBehaviour< T > : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance = null;

	public static T instance
	{
		get
		{
			if ( _instance == null )
			{
				_instance = GameObject.FindObjectOfType< T >();

				if ( _instance == null )
				{
					var obj = new GameObject( typeof( T ).ToString() );
					_instance = obj.AddComponent< T >();
				}
			}

			return _instance;
		}
	}

	public static bool DoesExist()
	{
		return _instance != null;
	}

	protected bool isInstance
	{
		get
		{
			return instance == this;
		}
	}
}
