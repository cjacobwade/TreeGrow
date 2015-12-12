using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GizmoManager 
{
	public enum GizmoTypes
	{
		None,
		PlayerSpawn
	}

	public static void DrawGizmoIcon(Vector3 position, GizmoTypes gizmoType)
	{
		if(gizmoType == GizmoTypes.None)
			return;

		string assetName = gizmoType.ToString() + "Gizmo.png";
		Gizmos.DrawIcon(position, assetName );
	}
}
