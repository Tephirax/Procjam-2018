﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcPerlinGroundBase : MonoBehaviour {

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

	//The string to be used as the seed for the Perlin Noise sampling
	public string seed = "I wandered lonely as a cloud";

	private void Start()
	{
		RefreshMesh();
	}

	void Update()
	{
		if (Input.GetKeyDown("r")) {
			Debug.Log("r pressed");
			seed = "Testing testing testing stuff";
			RefreshMesh();
		}
	}

	/// <summary>
	/// Initialisation. Build the mesh and assigns it to the object's MeshFilter
	/// </summary>
	private void RefreshMesh()
	{
		//Build the mesh:
		Mesh mesh = BuildMesh();
		Debug.Log("Mesh Building");

		//Look for a MeshFilter component attached to this GameObject:
		MeshFilter filter = GetComponent<MeshFilter>();
		Debug.Log("GetComponent<MeshFilter>");

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
	}

	//Build the mesh:
	public Mesh BuildMesh()
	{
		//Create a new mesh builder:
		MeshBuilder meshBuilder = new MeshBuilder();

		//Convert the seed string to a float to use as an offset for the PerlinNoise
		float seedFloat = (float)seed.GetHashCode() / 10000000.0f;
		Debug.Log ("Seed string " + seed + " hashes to " + seedFloat.ToString ());

		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++)
		{
			//incremented values for the Z position and V coordinate:
			float z = m_Length * i;
			float v = (1.0f / m_SegmentCount) * i;

			//Loop through the columns:
			for (int j = 0; j <= m_SegmentCount; j++)
			{
				//incremented values for the X position and U coordinate:
				float x = m_Width * j;
				float u = (1.0f / m_SegmentCount) * j;

				//The position offset for this quad, with a random height between zero and m_MaxHeight:
				//Vector3 offset = new Vector3(x, Random.Range(0.0f, m_Height), z);
				//Perlin Noise Approach:
				//float iPerlin = ((float)i/* + seedFloat*/) / scale;
				//float jPerlin = ((float)j/* + seedFloat*/) / scale;
				//float ijPerlin = Mathf.PerlinNoise(iPerlin, jPerlin);
				//Debug.Log ("iPerlin = " + iPerlin.ToString());
				//Debug.Log ("jPerlin = " + jPerlin.ToString());
				//Debug.Log ("ijPerlin = " + ijPerlin.ToString());
				//Vector3 offset = new Vector3(x, ijPerlin * m_Height, z);

				// Layered Perlin Noise Approach:
				float mul = 1.0f;
				float effectiveHeight = 0.0f;
				float totalPossibleSum = 0.0f;

				for (int k = 0; k < m_LayerCount; k++)
				{
					float iPerlin = (((float)i + seedFloat) / scale) / mul;
					float jPerlin = (((float)j + seedFloat) / scale) / mul;
					float ijPerlin = Mathf.PerlinNoise(iPerlin, jPerlin);
					//Debug.Log ("iPerlin = " + iPerlin.ToString());
					//Debug.Log ("jPerlin = " + jPerlin.ToString());
					//Debug.Log ("ijPerlin = " + ijPerlin.ToString());
					effectiveHeight += ijPerlin * mul;

					totalPossibleSum += mul;
					mul *= 0.5f;
				}

				Vector3 offset = new Vector3(x, effectiveHeight * m_Height, z);

				////Build individual quads:
				//BuildQuad(meshBuilder, offset);

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
