using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcPerlinGroundChange : MonoBehaviour {

	//The width and length of each segment:
	public float m_Width = 1.0f;
	public float m_Length = 1.0f;

	//The maximum height of the mesh:
	public float m_Height = 10.0f;

	//The number of segments in each dimension (the plane will be m_SegmentCount * m_SegmentCount in area):
	public int m_SegmentCount = 10;

	//The scale to be used for the Perlin Noise
	public float scale = 3;

	//How many times the Perlin noise should be stacked (for additional roughness);
	public int m_LayerCount = 1;

	//Is this the first run of the mesh creation? (ie. will there be a 'prior' mesh to change from?)
	private bool firstMesh = true;

	//Is the mesh in the process of changing?
	private bool changeInProgress = false;

	//The string to be used as the seed for the Perlin Noise sampling
	private string priorSeed;
	public string seed = "I wandered lonely as a cloud";

	//The time the change starts, for the lerp;
	private float startTime;

	//The distance to be travelled, for the lerp;
	public float[,] journeyLengthArray;

	//The speed at which the lerp should take place, in units/sec;
	public float speed = 1.0f;

	//Mesh Arrays; 
	public Vector3[,] priorMeshArray;
	public Vector3[,] desiredMeshArray;
	public Vector3[,] currentMeshArray;

	private void Start()
	{
		InitRefreshMesh();
	}

	void Update()
	{
		//Is there a change occurring?;
		if (changeInProgress) {
			//Debug.Log ("Change is in progress; mesh refreshing");
			RefreshMesh();
		}

		if (Input.GetKeyDown("r")) {
			//Debug.Log("r pressed");
			priorSeed = seed;
			seed = "Testing testing testing stuff";
			//Debug.Log("Initialising new Mesh: seed has changed from " + priorSeed + " to " + seed);
			InitRefreshMesh();
		}
	}

	/// <summary>
	/// Initialisation. Build the mesh and assigns it to the object's MeshFilter
	/// </summary>
	private void InitRefreshMesh()
	{
		
		startTime = Time.time; 


		//Get desired structure for mesh:
		desiredMeshArray = GetDesiredMeshStructure ();

		//If this isn't the first mesh created, get the prior mesh as the baseline, and get the distances between the prior mesh and the desired mesh;
		if (!firstMesh) {
			priorMeshArray = GetPriorMeshStructure ();
			//Create one further array which contains the distance each of the vertices will have to travel from prior to desired mesh;
			journeyLengthArray = GetJourneyLengths(priorMeshArray, desiredMeshArray);
		}

		//Flag that the mesh is changing; 
		changeInProgress = true;

	}

	private void RefreshMesh()
	{
		//If this is the first mesh, just assign the desired mesh to be the current mesh;
		if (firstMesh) {
			currentMeshArray = desiredMeshArray;
		}
		// Else create an array of lerps between the prior and desired meshes, to give us an mesh to be generated on this frame;
		else {
			currentMeshArray = GetCurrentMeshStructure (priorMeshArray, desiredMeshArray, journeyLengthArray);
		}

		//Build the mesh:
		Mesh mesh = BuildMesh(currentMeshArray);
		//Debug.Log("Mesh Building");

		//Look for a MeshFilter component attached to this GameObject:
		MeshFilter filter = GetComponent<MeshFilter>();
		//Debug.Log("GetComponent<MeshFilter>");

		//If the MeshFilter exists, attach the new mesh to it.
		//Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
		if (filter != null)
		{
			filter.sharedMesh = mesh;
		}

		//Look for a MeshCollider component attached to this GameObject:
		MeshCollider collider = GetComponent<MeshCollider>();

		//If the MeshCollider exists, attach the new mesh to it.
		if (collider != null)
		{
			collider.sharedMesh = mesh;
		}

		//Finally, check if the currentMeshArray matches the desiredMeshArray, and if so stop the change;
		bool checkArrays = CheckMeshArrays(currentMeshArray, desiredMeshArray);
		if (checkArrays) {
			changeInProgress = false;
		}
	}

	public bool CheckMeshArrays(Vector3[,] currentMeshArray, Vector3[,] desiredMeshArray) {
		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++) {
			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++) {
				if (currentMeshArray [i, j] != desiredMeshArray [i, j])
					return false;
			}
		} 
		return true;
	}

	public Vector3[,] GetPriorMeshStructure()
	{

		//Initialise Vector3 array, with the number of rows and columns as the dimensions:
		Vector3[,] priorMeshArray = new Vector3[m_SegmentCount + 1, m_SegmentCount + 1];

		//Convert the seed string to a float to use as an offset for the PerlinNoise
		float seedFloat = (float)priorSeed.GetHashCode() / 10000000.0f;
		//Debug.Log ("Prior seed string " + priorSeed + " hashes to " + seedFloat.ToString ());

		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++) {
			//incremented values for the Z position:
			float z = m_Length * i;

			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++) {
				//incremented values for the X position:
				float x = m_Width * j;

				// Layered Perlin Noise:
				float mul = 1.0f;
				float effectiveHeight = 0.0f;
				float totalPossibleSum = 0.0f;

				for (int k = 0; k < m_LayerCount; k++) {
					float iPerlin = (((float)i + seedFloat) / scale) / mul;
					float jPerlin = (((float)j + seedFloat) / scale) / mul;
					float ijPerlin = Mathf.PerlinNoise (iPerlin, jPerlin);
					//Debug.Log ("iPerlin = " + iPerlin.ToString());
					//Debug.Log ("jPerlin = " + jPerlin.ToString());
					//Debug.Log ("ijPerlin = " + ijPerlin.ToString());
					effectiveHeight += ijPerlin * mul;

					totalPossibleSum += mul;
					mul *= 0.5f;
				}

				// Assign the x, y, z positions to a Vector3 and store in the desiredMeshArray at the correct row and column
				priorMeshArray[i, j] = new Vector3 (x, effectiveHeight * m_Height, z);

			}
		}

		//return the new mesh:
		return priorMeshArray;
	}

	public Vector3[,] GetDesiredMeshStructure()
	{

		//Initialise Vector3 array, with the number of rows and columns as the dimensions:
		Vector3[,] desiredMeshArray = new Vector3[m_SegmentCount + 1, m_SegmentCount + 1];

		//Convert the seed string to a float to use as an offset for the PerlinNoise
		float seedFloat = (float)seed.GetHashCode() / 10000000.0f;
		//Debug.Log ("Desired seed string " + seed + " hashes to " + seedFloat.ToString ());

		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++) {
			//incremented values for the Z position:
			float z = m_Length * i;

			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++) {
				//incremented values for the X position:
				float x = m_Width * j;

				// Layered Perlin Noise:
				float mul = 1.0f;
				float effectiveHeight = 0.0f;
				float totalPossibleSum = 0.0f;

				for (int k = 0; k < m_LayerCount; k++) {
					float iPerlin = (((float)i + seedFloat) / scale) / mul;
					float jPerlin = (((float)j + seedFloat) / scale) / mul;
					float ijPerlin = Mathf.PerlinNoise (iPerlin, jPerlin);
					//Debug.Log ("iPerlin = " + iPerlin.ToString());
					//Debug.Log ("jPerlin = " + jPerlin.ToString());
					//Debug.Log ("ijPerlin = " + ijPerlin.ToString());
					effectiveHeight += ijPerlin * mul;

					totalPossibleSum += mul;
					mul *= 0.5f;
				}

				// Assign the x, y, z positions to a Vector3 and store in the desiredMeshArray at the correct row and column
				desiredMeshArray[i, j] = new Vector3 (x, effectiveHeight * m_Height, z);

			}
		}

		//return the new mesh:
		return desiredMeshArray;
	}

	public float[,] GetJourneyLengths(Vector3[,] priorMeshArray, Vector3[,] desiredMeshArray)
	{

		//Initialise float array, with the number of rows and columns as the dimensions:
		float[,] journeyLengthArray = new float[m_SegmentCount + 1, m_SegmentCount + 1];

		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++) {
			
			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++) {

				//Subtract priorMeshArray value from desiredMeshArray:
				Vector3 diffMesh = desiredMeshArray[i, j] - priorMeshArray[i, j];
				//Debug.Log("desiredMeshArray" + desiredMeshArray[i, j].ToString("F4"));
				//Debug.Log("priorMeshArray" + priorMeshArray[i, j].ToString("F4"));
				//Debug.Log("diffMesh" + diffMesh);
				journeyLengthArray[i, j] = Vector3.Magnitude(diffMesh);
				//Debug.Log("journeyLengthArray for " + i.ToString() + ", " + j.ToString() + ": " + journeyLengthArray[i, j].ToString());

			}
		}

		//return the new mesh:
		return journeyLengthArray;
	}

	public Vector3[,] GetCurrentMeshStructure(Vector3[,] priorMeshArray, Vector3[,] desiredMeshArray, float[,] journeyLengthArray)
	{

		//Initialise Vector3 array, with the number of rows and columns as the dimensions:
		Vector3[,] currentMeshArray = new Vector3[m_SegmentCount + 1, m_SegmentCount + 1];

		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++) {

			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++) {

				//Lerp from the prior to desired Vector3 positions, using startTime and journeyLengthArray
				float distCovered = (Time.time - startTime) * speed;
				//Debug.Log("distCovered for " + i.ToString() + ", " + j.ToString() + ": " + distCovered.ToString());

				float fractionOfJourney = distCovered / journeyLengthArray [i, j];
				//Debug.Log("fractionOfJourney for " + i.ToString() + ", " + j.ToString() + ": " + fractionOfJourney.ToString());

				currentMeshArray[i, j] = Vector3.Lerp(priorMeshArray[i, j], desiredMeshArray[i, j], fractionOfJourney);
				//Debug.Log ("currentMeshArray created for " + i.ToString() + ", " + j.ToString());

			}
		}

		//return the new mesh:
		return currentMeshArray;
	}

	//Build the mesh:
	public Mesh BuildMesh(Vector3[,] currentMeshArray)
	{
		//Create a new mesh builder:
		MeshBuilder meshBuilder = new MeshBuilder();

		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++)
		{
			//incremented values for the  V coordinate:
			float v = (1.0f / m_SegmentCount) * i;

			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++)
			{
				//incremented values for the U coordinate:
				float u = (1.0f / m_SegmentCount) * j;
			
				//Get corresponding Vector3 from the array:
				Vector3 offset = currentMeshArray[i, j];

				//build quads that share vertices:
				Vector2 uv = new Vector2(u, v);
				bool buildTriangles = i > 0 && j > 0;

				BuildQuadForGrid(meshBuilder, offset, uv, buildTriangles, m_SegmentCount + 1);
			}
		}

		//create the Unity mesh:
		Mesh mesh = meshBuilder.CreateMesh();

		//have the mesh calculate its own normals:
		mesh.RecalculateNormals();

		//If this is the first mesh, reset the flag so the next time it runs the full mesh comparison;
		if (firstMesh) {
			firstMesh = false;
		}

		//return the new mesh:
		return mesh;
		
	}

	#region "BuildQuadForGrid() methods"

	/// <summary>
	/// Builds a single quad as part of a mesh grid.
	/// </summary>
	/// <param name="meshBuilder">The mesh builder currently being added to.</param>
	/// <param name="position">A position offset for the quad. Specifically the position of the corner vertex of the quad.</param>
	/// <param name="uv">The UV coordinates of the quad's corner vertex.</param>
	/// <param name="buildTriangles">Should triangles be built for this quad? This value should be false if this is the first quad in any row or collumn.</param>
	/// <param name="vertsPerRow">The number of vertices per row in this grid.</param>
	protected void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow)
	{
		meshBuilder.Vertices.Add(position);
		meshBuilder.UVs.Add(uv);

		if (buildTriangles)
		{
			int baseIndex = meshBuilder.Vertices.Count - 1;

			int index0 = baseIndex;
			int index1 = baseIndex - 1;
			int index2 = baseIndex - vertsPerRow;
			int index3 = baseIndex - vertsPerRow - 1;

			meshBuilder.AddTriangle(index0, index2, index1);
			meshBuilder.AddTriangle(index2, index3, index1);
		}
	}

	/// <summary>
	/// Builds a single quad as part of a mesh grid.
	/// </summary>
	/// <param name="meshBuilder">The mesh builder currently being added to.</param>
	/// <param name="position">A position offset for the quad. Specifically the position of the corner vertex of the quad.</param>
	/// <param name="uv">The UV coordinates of the quad's corner vertex.</param>
	/// <param name="buildTriangles">Should triangles be built for this quad? This value should be false if this is the first quad in any row or collumn.</param>
	/// <param name="vertsPerRow">The number of vertices per row in this grid.</param>
	/// <param name="normal">The normal of the quad's corner vertex.</param>
	protected void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow, Vector3 normal)
	{
		meshBuilder.Vertices.Add(position);
		meshBuilder.UVs.Add(uv);
		meshBuilder.Normals.Add(normal);

		if (buildTriangles)
		{
			int baseIndex = meshBuilder.Vertices.Count - 1;

			int index0 = baseIndex;
			int index1 = baseIndex - 1;
			int index2 = baseIndex - vertsPerRow;
			int index3 = baseIndex - vertsPerRow - 1;

			meshBuilder.AddTriangle(index0, index2, index1);
			meshBuilder.AddTriangle(index2, index3, index1);
		}
	}

	#endregion
}
