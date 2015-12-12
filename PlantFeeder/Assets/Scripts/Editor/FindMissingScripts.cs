using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
	static int gameObjectCount;
	static int componentsCount;
	static int missingCount;
	
	[MenuItem("Utilities/Find missing scripts")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(FindMissingScripts));
	}
	
	public void OnGUI()
	{
		if(GUILayout.Button("Find missing scripts in selected"))
		{
			FindInSelected();
		}
	}
	
	static void FindInSelected()
	{
		gameObjectCount = componentsCount = missingCount = 0;
		
		GameObject[] selectedGameObjects = Selection.gameObjects;
		foreach(var gameObject in selectedGameObjects)
		{
			RecurseGameObject(gameObject);
		}
		
		Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", gameObjectCount, componentsCount, missingCount));
	}
	
	static void RecurseGameObject(GameObject gameObject)
	{
		if(gameObject == null)
			return;
		
		gameObjectCount++;
		Component[] components = gameObject.GetComponents<Component>();
		for(int i = 0; i < components.Length; i++)
		{
			componentsCount++;
			if(components[i] == null)
			{
				missingCount++;
				
				string fullPath = gameObject.name;
				Transform transform = gameObject.transform;
				while(transform.parent != null)
				{
					fullPath = transform.parent.name + "/" + fullPath;
					transform = transform.parent;
				}
				
				Debug.Log(fullPath + " has an empty script attached in position: " + i, gameObject);
			}
		}
		
		foreach(Transform child in gameObject.transform)
			RecurseGameObject(child.gameObject);
	}
}