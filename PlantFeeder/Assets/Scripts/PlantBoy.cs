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
	List<List<PlantJoint>> _finishedBranches = new List<List<PlantJoint>>();

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

	Camera _mainCamera = null;

	[SerializeField]
	float _mainBranchLifespan = 15f;

	[SerializeField]
	float _defaultMoveSpeed = 25f;
	float _moveSpeedMod = 1f;

	[SerializeField]
	float _lookAtMouseSpeed = 2f;

//	[SerializeField]
//	float _pitchSpeed = 5f;
//
//	[SerializeField]
//	float _yawSpeed = 5f;

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
	List<Vector4> _tangents = new List<Vector4>();
	List<Vector2> _uv = new List<Vector2>();
	List<int> _triangles = new List<int>();
	#endregion

	#region Branching
	[SerializeField]
	int _minJointsBeforeBranching = 15;

	[SerializeField]
	float _chanceToSpawnBranch = 0.07f;

	[SerializeField]
	int _minJointsBetweenBranches = 3;
	int _mainBranchJointsSinceBranch = 0;

	[SerializeField]
	MinMaxF _sideBranchLifespan = new MinMaxF(1.7f, 2.5f);

	[SerializeField]
	int _maxBranchFractures = 2;
	#endregion

	#region Leaves
	[SerializeField]
	float _chanceToGrowLeaves = 0.6f;

	[SerializeField]
	Jiggle _leafChunkPrefab = null;

	[SerializeField]
	float _growLeafTime = 4f;

	[SerializeField]
	MinMaxF _leafRotationOffsetRange = new MinMaxF(-20f, 20f);

	[SerializeField]
	AnimationCurve _leafScaleCurve = new AnimationCurve();

	[SerializeField]
	AnimationCurve _leafMoveCurve = new AnimationCurve();

	[SerializeField]
	float _mainLeafScaleMod = 1.5f;

	[SerializeField]
	float _leafHeightOffset = 0.15f;
	#endregion

	public System.Action FinishedGrowingCallback = delegate {};

	void Awake()
	{
		_mesh = new Mesh();
		_meshFilter = GetComponent<MeshFilter>();

		_mainCamera = Camera.main;

		_mainBranchMoveGuide = new GameObject("MoveGuide").transform;
		_mainBranchMoveGuide.parent = transform;
		_mainBranchMoveGuide.position = transform.position;
		_mainBranchMoveGuide.LookAt(_mainBranchMoveGuide.position + Vector3.up);

		_initPlantPos = transform.position;

		_mainBranchJoints.Add(new PlantJoint(transform.position, _mainBranchMoveGuide.forward, Vector3.right));
		_mainBranchCurrentJoint = new PlantJoint(transform.position, _mainBranchMoveGuide.forward, Vector3.right);

		StartCoroutine(GrowMainBranchRoutine());
	}

	IEnumerator GrowMainBranchRoutine()
	{
		float lifeTimer = 0f;
		while(lifeTimer < _mainBranchLifespan)
		{
			Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			moveDirection = _mainCamera.transform.TransformDirection(moveDirection);
			Vector3 moveToPosition = _mainBranchCurrentJoint.position + moveDirection;

			float lookAtHeightMod = Input.GetMouseButton(0) ? -1f : 1f;
			moveToPosition.y = _mainBranchCurrentJoint.position.y + lookAtHeightMod;

			float finishMod = 1f;
			float tenthLife = _mainBranchLifespan/10f;
			if(lifeTimer > _mainBranchLifespan - tenthLife)
				finishMod -= Mathf.InverseLerp(lifeTimer, _mainBranchLifespan - tenthLife, _mainBranchLifespan);

			_mainBranchMoveGuide.LerpLookAt(moveToPosition, Vector3.up, Time.deltaTime * _lookAtMouseSpeed * finishMod);

			int jointsBefore = _mainBranchJoints.Count;
			UpdateBranch(
				_mainBranchMoveGuide,
				_mainBranchJoints,
				_mainBranchCurrentJoint,
				_initPlantPos,
				_mainBranchJointsSinceBranch,
				_maxBranchFractures - 1);
			_mainBranchJointsSinceBranch += _mainBranchJoints.Count - jointsBefore;

			lifeTimer += Time.deltaTime;
			yield return null;
		}

		yield return new WaitForSeconds(1f);

		gameObject.AddComponent<MeshCollider>();

		PlantJoint leafJoint = _mainBranchJoints[_mainBranchJoints.Count - 5];
		StartCoroutine(GrowLeafRoutine(leafJoint.position, leafJoint.forward, _mainLeafScaleMod));

		FinishedGrowingCallback();
	}

	void LateUpdate()
	{
		// Add the current plant joint into our plantjoints list just for the mesh calculation!
		_mainBranchJoints.Add(_mainBranchCurrentJoint);
		CalculateBranchMesh(_mainBranchJoints);
		_mainBranchJoints.Remove(_mainBranchCurrentJoint);

		// Now that these branches have finished growing,
		// we take care of them here instead of leaving the coroutine running forever
		for(int i = 0; i < _finishedBranches.Count; i++)
			CalculateBranchMesh(_finishedBranches[i]);

		_mesh.vertices = _vertices.ToArray();
		_mesh.triangles = _triangles.ToArray();
		_mesh.uv = _uv.ToArray();
		_mesh.normals = _normals.ToArray();
		_mesh.tangents = _tangents.ToArray();

		_mesh.RecalculateBounds();
		_meshFilter.mesh = _mesh;

		_vertices.Clear();
		_normals.Clear();
		_triangles.Clear();
		_tangents.Clear();
		_uv.Clear();
	}

	void UpdateBranch( 
		Transform moveGuide,
		List<PlantJoint> plantJoints,
		PlantJoint currentJoint,
		Vector3 initJointPos,
		int jointsSinceBranch,
		int fracturesRemaining)
	{
		moveGuide.position += moveGuide.forward * MoveSpeed;

		currentJoint.position = moveGuide.position;
		currentJoint.timeAlive += Time.deltaTime;

		// Update each joint in the plant
		for(int i = 0; i < plantJoints.Count; i++)
		{
			PlantJoint pj = plantJoints[i];
			if(pj.timeAlive < _jointMaxGrowthTime)
			{
				pj.timeAlive += Time.deltaTime;
				pj.scale = _jointGrowthCurve.Evaluate(pj.timeAlive/_jointMaxGrowthTime) * _jointMaxGrowth;
			}

			if(!pj.attemptedBranch && pj.timeAlive > _jointMaxGrowthTime/3f)
			{
				pj.attemptedBranch = true;

				if( (currentJoint != _mainBranchCurrentJoint || i > _minJointsBeforeBranching) && 
					jointsSinceBranch > _minJointsBetweenBranches && 
					Random.value < _chanceToSpawnBranch)
				{
					jointsSinceBranch = 0;
					StartCoroutine(StartBranchAtJoint(pj, _sideBranchLifespan.Random, fracturesRemaining));
				}
			} 
		}

		// If we haven't dropped a joint yet, compare with our starting position
		Vector3 prevPos = plantJoints.Last() ? plantJoints.Last().position : initJointPos;

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
		float uvHeight = 0f;

		// Iterate through each of the plant joints
		for(int i = 0; i < plantJoints.Count; i++)
		{
			PlantJoint pj = plantJoints[i];
			uvHeight += _jointDropDistance;

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

				if(uvHeight > 0.5f)
					uvHeight = uvHeight % 0.5f;

				_normals.Add(normal);
				_uv.Add(new Vector2((j + 1)/(float)_vertsPerJoint, uvHeight) * 0.5f);

				// Calculate tangent in a dumb way
				Vector3 tanPos = pj.position;
				if(j == 0)
					circleRight = Mathf.Cos(percentAroundCircle - 0.001f * Mathf.PI * 2f) * -pj.right;
				else
					circleRight = Mathf.Cos(percentAroundCircle + 0.001f * Mathf.PI * 2f) * pj.right;
					
				tanPos += circleRight;
				tanPos += circleUp;

				_tangents.Add(((Vector4)(tanPos - vertPos).normalized).SetW(1f));

				if(i != 0 && j != 0)
				{
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
		moveGuide.position = branchMainJoint.position;
		moveGuide.parent = transform;

		moveGuide.LookAt( branchMainJoint.position, branchMainJoint.forward );
		moveGuide.rotation *= Quaternion.Euler(Random.Range(0f, 360f), 0f, 0f);

		// If our branch starts point downward, keep rerolling until it isn't
		float downDot = Vector3.Dot(Vector3.down, moveGuide.forward);
		while(downDot > 0.4f || downDot < -0.8f )
		{
			moveGuide.rotation *= Quaternion.Euler(Random.Range(0f, 360f), 0f, 0f);
			downDot = Vector3.Dot(Vector3.down, moveGuide.forward);
		}

		Quaternion startRotation = moveGuide.rotation;
		float yRotation = Random.Range(-90f, -30f);
		Quaternion midRotation = Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(-45f, 45f), 0f);
		Quaternion endRotation = Quaternion.Euler((Random.insideUnitSphere * 360f).SetX(yRotation));

		Vector3 initJointPos = branchMainJoint.position;
		int jointsSinceBranch = 0;
		bool attemptedLeafGrowth = false;

		float lifeTimer = 0f;
		while(lifeTimer < lifespan)
		{
			int jointsBefore = branchJoints.Count;
			UpdateBranch(
				moveGuide,
				branchJoints,
				branchMainJoint,
				initJointPos,
				jointsSinceBranch,
				fracturesRemaining - 1);
			jointsSinceBranch += branchJoints.Count - jointsBefore;

			branchJoints.Add(branchMainJoint);
			CalculateBranchMesh(branchJoints);
			branchJoints.Remove(branchMainJoint);

			float halfLife = lifespan/2f;
			if(lifeTimer > halfLife && !attemptedLeafGrowth)
			{
				attemptedLeafGrowth = true;
				if(Random.value < _chanceToGrowLeaves)
					StartCoroutine(GrowLeafRoutine(initJointPos, startingJoint.forward));
			}
			
			if(lifeTimer < halfLife)
				moveGuide.rotation = Quaternion.Lerp(startRotation, midRotation, lifeTimer/halfLife );
			else
				moveGuide.rotation = Quaternion.Lerp(midRotation, endRotation, (lifeTimer - halfLife)/halfLife ); 

			lifeTimer += Time.deltaTime;
			yield return null;
		}

		Destroy(moveGuide.gameObject);
		branchJoints.Add(branchMainJoint);
		_finishedBranches.Add(branchJoints);
	}

	IEnumerator GrowLeafRoutine(Vector3 position, Vector3 forward, float scaleMod = 1f)
	{
		Jiggle newLeafChunk = Instantiate<Jiggle>(_leafChunkPrefab);
		newLeafChunk.transform.SetParent(transform);
		newLeafChunk.transform.position = position;
		newLeafChunk.transform.LookAt(position + forward);

		newLeafChunk.transform.rotation *= Quaternion.Euler(
			_leafRotationOffsetRange.Random,
			_leafRotationOffsetRange.Random,
			_leafRotationOffsetRange.Random);

		float clampedXRotation = _leafRotationOffsetRange.Clamp(newLeafChunk.transform.eulerAngles.x);
		newLeafChunk.transform.eulerAngles = newLeafChunk.transform.eulerAngles.SetX(clampedXRotation);

		Vector3 startScale = newLeafChunk.transform.localScale = Vector3.zero;
		Vector3 endScale = Vector3.one * scaleMod;

		Vector3 startPos = newLeafChunk.transform.position;
		Vector3 endPos = startPos + Vector3.up * _leafHeightOffset;

		float growLeafTimer = 0f;
		while(growLeafTimer < _growLeafTime)
		{
			newLeafChunk.transform.localScale = Vector3.Lerp(startScale, endScale, _leafScaleCurve.Evaluate(growLeafTimer/_growLeafTime));
			newLeafChunk.transform.position = Vector3.Lerp(startPos, endPos, _leafMoveCurve.Evaluate(growLeafTimer/_growLeafTime));
			
			growLeafTimer += Time.deltaTime;
			yield return null;
		}

		newLeafChunk.SetCanJiggle(true);
	}
}
