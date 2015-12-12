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
			attemptedBranch = false;
		}

		public static implicit operator bool(PlantJoint pj)
		{ return pj != null; }

		public Vector3 position;
		public Vector3 forward;
		public Vector3 right;
		public float scale;
		public float timeAlive; // Use to check for scaling
		public bool attemptedBranch;
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
	Transform _mainBranchMoveGuide = null;

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

	List<Vector3> _vertices = new List<Vector3>();
	List<Vector3> _normals = new List<Vector3>();
	List<int> _triangles = new List<int>();
	#endregion

	#region Branching
	[SerializeField]
	float _chanceToSpawnBranch = 0.07f;

	[SerializeField]
	int _minJointsBetweenBranches = 3;
	int _jointsSinceBranch = int.MaxValue;

	[SerializeField]
	float _sideBranchLifespan = 5f;

	[SerializeField]
	int _maxBranchFractures = 2;
	#endregion

	void Awake()
	{
		_mesh = new Mesh();
		_meshFilter = GetComponent<MeshFilter>();

		_mainBranchMoveGuide = new GameObject("MoveGuide").transform;
		_mainBranchMoveGuide.parent = transform;
		_mainBranchMoveGuide.position = transform.position;
		_mainBranchMoveGuide.LookAt(_mainBranchMoveGuide.position + Vector3.up);

		_initPlantPos = transform.position;

		_mainBranchJoints.Add(new PlantJoint(transform.position, _mainBranchMoveGuide.forward, Vector3.right));
		_mainBranchCurrentJoint = new PlantJoint(transform.position, _mainBranchMoveGuide.forward, Vector3.right);
	}

	void Update()
	{
		Vector3 moveVec = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0f);
		moveVec.x *= _pitchSpeed * Time.deltaTime;
		moveVec.y *= _yawSpeed * Time.deltaTime;
		_mainBranchMoveGuide.Rotate(moveVec);

		UpdateBranch(_mainBranchMoveGuide, _mainBranchJoints, _mainBranchCurrentJoint, _maxBranchFractures - 1);
	}

	void LateUpdate()
	{
		// Add the current plant joint into our plantjoints list just for the mesh calculation!
		_mainBranchJoints.Add(_mainBranchCurrentJoint);
		CalculateBranchMesh(_mainBranchJoints);
		_mainBranchJoints.Remove(_mainBranchCurrentJoint);

		_mesh.vertices = _vertices.ToArray();
		_mesh.normals = _normals.ToArray();
		_mesh.triangles = _triangles.ToArray();

		_meshFilter.mesh = _mesh;

		_vertices.Clear();
		_normals.Clear();
		_triangles.Clear();
	}

	void UpdateBranch(Transform moveGuide, List<PlantJoint> plantJoints, PlantJoint currentJoint, int fracturesRemaining)
	{
		moveGuide.position += moveGuide.forward * MoveSpeed;

		currentJoint.position = moveGuide.position;
		currentJoint.timeAlive += Time.deltaTime;

		// Update each joint in the plant
		for(int i = 0; i < plantJoints.Count; i++)
		{
			PlantJoint pj = plantJoints[i];
			if(!pj.attemptedBranch)
			{
				pj.attemptedBranch = true;

				// TODO: Joints since sidebranch should be local to the branch
				if(_jointsSinceBranch > _minJointsBetweenBranches && Random.value < _chanceToSpawnBranch)
				{
					_jointsSinceBranch = 0;
					StartCoroutine(StartBranchAtJoint(pj, _sideBranchLifespan, fracturesRemaining));
				}
				else
					_jointsSinceBranch++;
			}

			if(pj.timeAlive < _jointMaxGrowthTime)
			{
				pj.timeAlive += Time.deltaTime;

				float fractureScaleMod = fracturesRemaining/(float)_maxBranchFractures;
				pj.scale = _jointGrowthCurve.Evaluate(pj.timeAlive/_jointMaxGrowthTime) * _jointMaxGrowth;
			}
		}

		// If we haven't dropped a joint yet, compare with our starting position
		Vector3 prevPos = plantJoints.Last() ? plantJoints.Last().position : _initPlantPos;

		float distanceSincePrevJoint = Vector3.Distance(currentJoint.position, prevPos);
		if(distanceSincePrevJoint > _jointDropDistance)
		{
			int catchupJointsNeed = Mathf.FloorToInt(distanceSincePrevJoint/_jointDropDistance);
			for(int i = 0; i < catchupJointsNeed; i++)
			{
				Vector3 dirFromLastJoint = (currentJoint.position - prevPos).normalized;
				Vector3 newJointPos = prevPos + dirFromLastJoint * _jointDropDistance;

				PlantJoint newJoint = new PlantJoint(newJointPos, dirFromLastJoint, moveGuide.right);
				newJoint.scale = _jointGrowthCurve.Evaluate(0f) * _jointMaxGrowth;

				plantJoints.Add(newJoint);
			}

			currentJoint.timeAlive = 0f;
		}
	}

	void CalculateBranchMesh(List<PlantJoint> plantJoints)
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
				_vertices.Add(vertPos);

				Vector3 basicOffset = vertPos + circleUp + circleRight;
				Vector3 normal = (basicOffset - pj.position).normalized;

				_normals.Add(normal);

				if(i != 0 && j != 0)
				{
					PlantJoint prevPj = plantJoints[i - 1];
					int currentTri = _vertices.Count - 1;
					int prevTri = currentTri - 1; // Previous vert on same joint
					int prevJointTri = currentTri - _vertsPerJoint; // Same vert previous joint
					int prevJointPrevTri = prevTri - _vertsPerJoint; // Previous vert on previous joint

					_triangles.Add(prevJointPrevTri);
					_triangles.Add(currentTri);
					_triangles.Add(prevTri);

					_triangles.Add(prevJointPrevTri);
					_triangles.Add(prevJointTri);
					_triangles.Add(currentTri);

					if(j == _vertsPerJoint - 1)
					{
						// Need to complete the cylinder!
						// Treat each joints 0 verts as another new vert around the cylinder as before

						// Jump prevs up to the current and prevJoints
						prevTri = currentTri; // Previous vert on same joint
						prevJointPrevTri = prevJointTri; // Previous vert on previous joint

						currentTri = _vertices.Count - _vertsPerJoint;
						prevJointTri = currentTri - _vertsPerJoint; // Same vert previous joint

						_triangles.Add(prevJointPrevTri);
						_triangles.Add(currentTri);
						_triangles.Add(prevTri);

						_triangles.Add(prevJointPrevTri);
						_triangles.Add(prevJointTri);
						_triangles.Add(currentTri);
					}
				}
			}
		}
	}

	IEnumerator StartBranchAtJoint(PlantJoint startingJoint, float lifespan, int fracturesRemaining)
	{
		List<PlantJoint> branchJoints = new List<PlantJoint>();
		Vector3 newForward = (startingJoint.right + startingJoint.forward)/2f;
		Vector3 newRight = Vector3.Cross(startingJoint.forward, startingJoint.right);
		PlantJoint branchMainJoint = new PlantJoint( startingJoint.position, newForward, newRight );

		Transform moveGuide = new GameObject("TempMoveGuide").transform;
		moveGuide.transform.position = branchMainJoint.position;
		moveGuide.transform.LookAt( branchMainJoint.position, branchMainJoint.forward );
		moveGuide.parent = transform;

		Quaternion startRotation = moveGuide.rotation;
		float yRotation = Random.Range(-90f, -30f);
		Quaternion endRotation = Quaternion.Euler((Random.insideUnitSphere * 360f).SetX(yRotation));

		float lifeTimer = 0f;
		while(lifeTimer < lifespan)
		{
			UpdateBranch(moveGuide, branchJoints, branchMainJoint, fracturesRemaining - 1);

			branchJoints.Add(branchMainJoint);
			CalculateBranchMesh(branchJoints);
			branchJoints.Remove(branchMainJoint);

			moveGuide.rotation = Quaternion.Lerp(startRotation, endRotation, lifeTimer/lifespan );

			lifeTimer += Time.deltaTime;
			yield return null;
		}

		Destroy(moveGuide.gameObject);

		branchJoints.Add(branchMainJoint);
		while(true)
		{
			CalculateBranchMesh(branchJoints);
			yield return null;
		}
	}

	void OnDrawGizmos()
	{
		if(_mainBranchCurrentJoint)
		{
			Gizmos.color = Color.green - new Color(0f, 0f, 0f, 0.7f);
			Gizmos.DrawSphere(_mainBranchCurrentJoint.position, 0.1f);
		}

		for(int i = 0; i < _vertices.Count; i++)
		{
			Gizmos.DrawSphere(transform.position + _vertices[i], 0.01f);
		}

		if(_mainBranchJoints.Count > 0)
			Gizmos.DrawSphere(Center, 0.3f);
	}
}
