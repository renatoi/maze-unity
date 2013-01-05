using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour {
	
	// defined in the inspector
	public Transform CellPrefab;
	public Vector3 GridSize = new Vector3(5,0,5);
	public Light PointLight;
	public Vector3[] Directions;
	
	// list of carved cells
	private List<Transform> Carved = new List<Transform>();
	
	// multi-dimensional array for each cell in the maze
	private Transform[,] Cells;
	
	// debug line
	LineRenderer DebugLine;
	
	// line count
	int LineCount = 0;
	
	// Use this for initialization
	void Start () {
		SetupCamera();
		CreateGrid();
		DebugLine = gameObject.AddComponent<LineRenderer>();
		DebugLine.material = new Material(Shader.Find("Particles/Additive"));
		DebugLine.SetWidth(0.2F,0.2F);
		DebugLine.SetColors (Color.blue, Color.green);
		Carve(0,0, Vector3.zero);
	}
	
	void SetupCamera () {
		//float w = Screen.width/2;
		//float h = Screen.height/2;
		float x = Mathf.Max(GridSize.x,GridSize.z)/2;
		float y = Mathf.Max(GridSize.x,GridSize.z);
		float z = Mathf.Max(GridSize.x,GridSize.z)/2;
		// Setup camera for debugging purposes
		Camera.mainCamera.transform.position = new Vector3(x,y,z);
		Camera.mainCamera.orthographicSize = x;
		PointLight.transform.position = new Vector3(x,y,z);
		PointLight.range = x * 10;
	}
	
	void CreateGrid () {
		int y = 0;
		Transform newCell;
		Cells = new Transform[(int) GridSize.x, (int) GridSize.z];

		for (int x = 0; x < GridSize.x; x++) {
			for (int z = 0; z < GridSize.z; z++) {
				// instantiate and name it
				newCell = (Transform) Instantiate(CellPrefab, new Vector3(x, y, z), Quaternion.identity);
				newCell.name = string.Format("{0}, {1}, {2}", x,y,z);
				newCell.parent = transform;
				newCell.GetComponent<Cell> ().Position = new Vector3(x, y, z);
				
				// add instance to Cells array
				Cells[x, z] = newCell;
			}
		}
	}
	
	void Carve (int x, int z, Vector3 currentPos = new Vector3(), Vector3 direction = new Vector3()) {
		// add the carved cell to the Carved stack
		Carved.Add(Cells[x, z]);
		
		// set flag that this cell has been executed
		Cell cellScript = Cells[x,z].GetComponent<Cell>();
		cellScript.IsCarved = true;
		Cells[x,z].renderer.material.color = Color.grey;
		
		// destrouy walls by raycasting
		currentPos.y = 0.5F;
		RaycastHit[] hits;
		hits = Physics.RaycastAll(currentPos, direction, 1);
		for (int i = 0; i < hits.Length; i++) {
			Debug.DrawLine (currentPos, hits[i].point, Color.red, 200);
			Destroy(hits[i].collider.gameObject);
		}
	}
	
	void GenerateCorridors() {

		int needle;
		Transform currentCell;
		Vector3 currentPos;;
		int nextX;
		int nextZ;
		int randSeed;
		
		// Carved holds the list of carved cells
		// We will add each carved cell to the list
		// We will try to carve until no cell is left uncarved
		while (Carved.Count > 0) {
			needle = NextIndex(Carved.Count);
			currentCell = Carved[needle];
			currentPos = currentCell.GetComponent<Cell>().Position;
			randSeed = Random.Range(0, 4);
			for (int neighbors=0; neighbors < 4; neighbors++) {			
				// pick a random a neighbor
				Vector3 randDirection = Directions[randSeed];
				nextX = (int) currentPos.x + (int) randDirection.x;
				nextZ = (int) currentPos.z + (int) randDirection.z;
				
				// check if neighbor is valid
				if (nextX >= 0 && nextZ >= 0 && nextX < GridSize.x && nextZ < GridSize.z && !Cells[nextX, nextZ].GetComponent<Cell>().IsCarved) {
					// neighbor is valid then carve
					Carve (nextX, nextZ, currentPos, randDirection);
					//this.DrawDebugLine((int) currentPos.x, (int) currentPos.z, nextX, nextZ);
					break;
				}
				// if we are at the last iteration then none of the neighbors are valid
				else if (neighbors == 3) {
					// backtracing
					Carved.RemoveAt(needle);
				}
				// neighbor is not valid then try next neighbor
				else {
					randSeed = (randSeed + 1) % 4; // 0, 1, 2, 3
				}
			}
		}
	}
	
	int NextIndex(int length) {
		return length - 1;
	}
	
	void DrawDebugLine(int cx, int cz, int nx, int nz) {
		DebugLine = GetComponent<LineRenderer>();
		DebugLine.SetVertexCount(LineCount + 2);
		if (LineCount == 0) {
			DebugLine.SetPosition(LineCount, new Vector3(cx, 1, cz));
		}
		DebugLine.SetPosition(LineCount, new Vector3(nx, 1, nz));
		LineCount++;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			GenerateCorridors();
		}
	}
}
