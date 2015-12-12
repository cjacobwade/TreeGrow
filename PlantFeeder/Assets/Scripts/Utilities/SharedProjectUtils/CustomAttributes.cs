using UnityEngine;
using System.Collections;

public class ReadOnlyAttribute : PropertyAttribute 
{
	public string displayName { get; private set; }

	public ReadOnlyAttribute( string _displayName = "" )
	{
		displayName = _displayName;
	}
}
