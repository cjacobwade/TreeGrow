using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlantBoy : WadeBehaviour 
{
	// Procedural mesh need to create this guy
	// Will be a circle of vertices that scale outwards to give thickness to the plant

	[System.Serializable]
	class PlantJoint
	{
		public PlantJoint(Vector3 inPosition, Vector3 inForward, Vector3 inRight)
		{
			position = inPosition;
			forward = inForward;
			right = inRight;
			scale = 0f;
			timeAlive = 0f;
		}

		public static implicit operator bool(PlantJoint pj)
		{ return pj != null; }

		public Vector3 position;
		public Vector3 forward;
		public Vector3 right;
		public float scale;
		public float timeAlive; // Use to check for scaling
	}

	List<PlantJoint> _mainBranchJoints = new List<PlantJoint>();

	public Vector3 Center
	{
		get 
		{
			Vector3 averagePos = _mainBranchCurrentJoint.position;
			foreach(PlantJoint pj in _mainBranchJoints)
				averagePos += pj.position;

			return averagePos/(float)(_mainBranchJoints.Count + 1);
		}
	}

	PlantJoint _mainBranchCurrentJoint = null;

	#region Movement
	Vector3 _initPlantPos = Vector3.zero;

	// We guide the plant with a transform so we affect it's rotation in
	// a sensical way
	Transform _moveGuide = null;

	[SerializeField]
	float _defaultMoveSpeed = 25f;
	float _moveSpeedMod = 1f;

	[SerializeField]
	float _pitchSpeed = 5f;

	[SerializeField]
	float _yawSpeed = 5f;

	public float MoveSpeed
	{ get { return _defaultMoveSpeed * _moveSpeedMod * Time.deltaTime; } }

	[SerializeField]
	float _jointDropDistance = 0.1f;
	#endregion

	#region Joint Scaling
	[SerializeField]
	AnimationCurve _jointGrowthCurve = new AnimationCurve();

	[SerializeField]
	float _jointMaxGrowth = 3f;

	[SerializeField]
	float _jointMaxGrowthTime = 7f;
	#endregion

	#region Mesh Drawing
	Mesh _mesh = null;
	MeshFilter _meshFilter = null;

	[SerializeField]
	int _vertsPerJoint = 10;

	List<Vector3> _debugVerts = new List<Vector3>();
	#endregion

	void Awake()
	{
		_mesh = new Mesh();
		_meshFilter = GetComponent<MeshFilter>();

		_moveGuide = new GameObject("MoveGuide").transform;
		_moveGuide.position = transform.position;
		_moveGuide.LookAt(_moveGuide.position + Vector3.up);

		_initPlantPos = transform.position;

		_mainBranchJoints.Add(new PlantJoint(transform.position, _moveGuide.forward, Vector3.right));
		_mainBranchCurrentJoint = new PlantJoint(transform.position, _moveGuide.forward, Vector3.right);
	}

	void Update()
	{
		Vector3 moveVec = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0f);
		moveVec.x *= _pitchSpeed * Time.deltaTime;
		moveVec.y *= _yawSpeed * Time.deltaTime;
		_moveGuide.Rotate(moveVec);

		// Always move in the direction of the light
		_moveGuide.position += _moveGuide.forward * MoveSpeed;
	}

	void FixedUpdate()
	{
		_mainBranchCurrentJoint.position = _moveGuide.position;
		_mainBranchCurrentJoint.timeAlive += Time.deltaTime;

		// Update each joint in the plant
		for(int i = 0; i < _mainBranchJoints.Count; i++)
		{
			PlantJoint pj = _mainBranchJoints[i];
			if(pj.timeAlive < _jointMaxGrowthTime)
			{
				pj.timeAlive += Time.deltaTime;
				pj.timeAlive = Mathf.Clamp(pj.timeAlive, float.MinValue, _jointMaxGrowthTime);

				pj.scale = _jointGrowthCurve.Evaluate(pj.timeAlive/_jointMaxGrowthTime) * _jointMaxGrowth;
			}
		}

		// If we haven't dropped a joint yet, compare with our starting position
		Vector3 prevPos = _mainBranchJoints.Last() ? _mainBranchJoints.Last().position : _initPlantPos;

		float distanceSincePrevJoint = Vector3.Distance(_mainBranchCurrentJoint.position, prevPos);
		if(distanceSincePrevJoint > _jointDropDistance)
		{
			int catchupJointsNeed = Mathf.FloorToInt(distanceSincePrevJoint/_jointDropDistance);
			for(int i = 0; i < catchupJointsNeed; i++)
			{
				Vector3 dirFromLastJoint = (_mainBranchCurrentJoint.position - prevPos).normalized;
				Vector3 newJointPos = prevPos + dirFromLastJoint * _jointDropDistance;

				PlantJoint newJoint = new PlantJoint(newJointPos, dirFromLastJoint, _moveGuide.right);
				newJoint.scale = _jointGrowthCurve.Evaluate(0f) * _jointMaxGrowth;

				_mainBranchJoints.Add(newJoint);
			}

			_mainBranchCurrentJoint.timeAlive = 0f;
		}
	}

	void LateUpdate()
	{
		// DrawMesh!
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		//List<Vector2> uvs = new List<Vector2>(); This can come later
		List<int> triangles = new List<int>();

		// Add the current plant joint into our plantjoints list just for the mesh calculation!
		_mainBranchJoints.Add(_mainBranchCurrentJoint);
		CalculateBranch(_mainBranchJoints, vertices, normals, triangles);
		_mainBranchJoints.Remove(_mainBranchCurrentJoint);

		_mesh.vertices = vertices.ToArray();
		_mesh.triangles = triangles.ToArray();
		_mesh.normals = normals.ToArray();

		_meshFilter.mesh = _mesh;

		// Debug
		_debugVerts = vertices;
	}

	void CalculateBranch(List<PlantJoint> plantJoints, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
	{
		// Iterate through each of the plant joints
		for(int i = 0; i < plantJoints.Count; i++)
		{
			PlantJoint pj = plantJoints[i];

			// If main branch current plantjoint is at 0f time, we've just added a new plantjoint
			// Don't draw the last set of triangles otherwise there will be Z-fighting
			if(pj == _mainBranchCurrentJoint && WadeUtils.IsZero(pj.timeAlive))
				continue;

			for(int j = 0; j < _vertsPerJoint; j++)
			{
				Vector3 jointUp = Vector3.Cross(pj.forward, pj.right);
				float percentAroundCircle = j/(float)_vertsPerJoint;

				Vector3 vertPos = pj.position;
				Vector3 circleUp = Mathf.Sin(percentAroundCircle * Mathf.PI * 2f) * jointUp;
				Vector3 circleRight = Mathf.Cos(percentAroundCircle * Mathf.PI * 2f) * pj.right;

				vertPos += circleUp * pj.scale;
				vertPos += circleRight * pj.scale;
				vertices.Add(vertPos);

				Vector3 basicOffset = vertPos + circleUp + circleRight;
				Vector3 normal = (basicOffset - pj.position).normalized;

				normals.Add(normal);

				if(i != 0 && j != 0)
				{
					PlantJoint prevPj = plantJoints[i - 1];
					int currentTri = vertices.Count - 1;
					int prevTri = currentTri - 1; // Previous vert on same joint
					int prevJointTri = currentTri - _vertsPerJoint; // Same vert previous joint
					int prevJointPrevTri = prevTri - _vertsPerJoint; // Previous vert on previous joint

					triangles.Add(prevJointPrevTri);
					triangles.Add(currentTri);
					triangles.Add(prevTri);

					triangles.Add(prevJointPrevTri);
					triangles.Add(prevJointTri);
					triangles.Add(currentTri);

					if(j == _vertsPerJoint - 1)
					{
						// Need to complete the cylinder!
						// Treat each joints 0 verts as another new vert around the cylinder as before

						// Jump prevs up to the current and prevJoints
						prevTri = currentTri; // Previous vert on same joint
						prevJointPrevTri = prevJointTri; // Previous vert on previous joint

						currentTri = vertices.Count - _vertsPerJoint;
						prevJointTri = currentTri - _vertsPerJoint; // Same vert previous joint

						triangles.Add(prevJointPrevTri);
						triangles.Add(currentTri);
						triangles.Add(prevTri);

						triangles.Add(prevJointPrevTri);
						triangles.Add(prevJointTri);
						triangles.Add(currentTri);
					}
				}
			}
		}
	}

	IEnumerator StartBranchAtJoint(PlantJoint plantJoint)
	{
		// Spawn a move guide and randomly aim it up-ish
		// Keep track of it's plantjoints here and draw them from here also
		yield return null;
	}

	void OnDrawGizmos()
	{
		if(_mainBranchCurrentJoint)
		{
			Gizmos.color = Color.green - new Color(0f, 0f, 0f, 0.7f);
			Gizmos.DrawSphere(_mainBranchCurrentJoint.position, 0.1f);
		}

		for(int i = 0; i < _debugVerts.Count; i++)
		{
			Gizmos.DrawSphere(transform.position + _debugVerts[i], 0.01f);
		}

		if(_mainBranchJoints.Count > 0)
			Gizmos.DrawSphere(Center, 0.3f);
	}
}
