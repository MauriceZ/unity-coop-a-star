using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {
  [Range(1f, 40f)]
  public float speed;

  private GameManager gameManager;

  private Dictionary<string, Cell> teacherCells = new Dictionary<string, Cell>();

  private List<Cell> FindPath(Cell destinationCell) {
    var grid = gameManager.Grid;
    var startCell = grid.WorldPointToCell(transform.position.x, transform.position.z);

    if (startCell == null) {
      throw new System.NullReferenceException("Student is not in grid");
    }

    if (startCell == destinationCell || destinationCell == null) {
      return new List<Cell>();
    }

    var queue = new PriorityQueue<Cell>();
    // var queue = new Queue<Cell>(); // for BFS
    queue.Enqueue(startCell, 0);

    var parents = new Dictionary<Cell, Cell>();
    var visited = new HashSet<Cell>();
    var gCost = new Dictionary<Cell, float>();

    visited.Add(startCell);
    gCost[startCell] = 0;

    while (!queue.IsEmpty()) {
      var currentCell = queue.Dequeue();

      if (currentCell == destinationCell) {
        break;
      }

      foreach (var neighborCell in currentCell.GetNeighbors()) {
        if (!visited.Contains(neighborCell)) {
          visited.Add(neighborCell);

          // gCost[neighborCell] = gCost[currentCell] + distanceBetweenCells(currentCell, neighborCell);
          gCost[neighborCell] = gCost[currentCell] + 1;
          var hCost = distanceBetweenCells(neighborCell, destinationCell); // using the straight line distance as a heuristic
          parents[neighborCell] = currentCell;

          queue.Enqueue(neighborCell, gCost[neighborCell] + hCost);
        }
      }
    }

    var path = new List<Cell>();
    path.Add(destinationCell);
    var parent = destinationCell;

    while (parents.ContainsKey(parent)) {
      path.Add(parent);
      parent = parents[parent];
    }

    path.Reverse();

    return path;
  }

  private float distanceBetweenCells(Cell cell1, Cell cell2) {
    return Vector2.Distance(cell1.WorldCoords, cell2.WorldCoords);
  }

  public IEnumerator MoveToCell(Cell destinationCell) {
    var delay = new WaitForSeconds (1.0f/82f);

    var path = FindPath(destinationCell);
    var studentY = transform.position.y;

    for (int i = 0; i < path.Count - 1; i++) {
      var startPosition = new Vector3(path[i].WorldCoords.x, studentY, path[i].WorldCoords.y);
      var nextPosition = new Vector3(path[i + 1].WorldCoords.x, studentY, path[i + 1].WorldCoords.y);
      var currentPosition = startPosition;

      for (float j = 0f; j < 1; j += speed/82f) {
        transform.position = currentPosition;
        currentPosition = Vector3.Lerp(startPosition, nextPosition, j);
        yield return delay;
      }
    }
  }

	// Use this for initialization
	void Start () {
		gameManager = transform.root.GetComponent<GameManager>();
    Debug.Log(transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
