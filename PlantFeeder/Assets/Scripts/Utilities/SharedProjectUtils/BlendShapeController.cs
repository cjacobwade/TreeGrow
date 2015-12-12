using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendShapeController : MonoBehaviour
{
	SkinnedMeshRenderer _skinnedMeshRenderer = null;
	public SkinnedMeshRenderer skinnedMeshRenderer
	{
		get 
		{
			if ( !_skinnedMeshRenderer )
			{
				_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			}
			return _skinnedMeshRenderer;
		}
	}
}