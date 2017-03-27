using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {
  public float speed;

  private GameManager gameManager;
  private Dictionary<string, Cell> teacherCells = new Dictionary<string, Cell>();

  private List<Cell> GetPath(Cell destinationCell) {
    var grid = gameManager.Grid;
    var startCell = grid.WorldPointToCell(transform.position.x, transform.position.z);

    if (startCell == null)
      throw new System.NullReferenceException("Student is not in grid");

    if (startCell == destinationCell || destinationCell == null) {
      return new List<Cell>();
    }

    var rra = new ReverseResumableAStar(destinationCell, startCell);

    var queue = new PriorityQueue<Grid.CellTimePair>();
    var startCellTime = new Grid.CellTimePair(startCell, 0);
    queue.Enqueue(startCellTime, 0);

    var pathParents = new Dictionary<Grid.CellTimePair, Grid.CellTimePair>();

    while (!queue.IsEmpty()) {
      var currentCellTimePair = queue.Peek();
      var currentCell = currentCellTimePair.cell;

      if (currentCell == destinationCell)
        break;

      queue.Dequeue();

      var neighborGCost = currentCellTimePair.timeUnit + 1;
      foreach (var neighborCell in currentCell.GetAvailableNeighbors(neighborGCost)) {
        var neighborCellTimePair = new Grid.CellTimePair(neighborCell, neighborGCost);
        var neighborHCost = rra.GetTrueDistance(neighborCell);
        pathParents[neighborCellTimePair] = currentCellTimePair;
        queue.Enqueue(neighborCellTimePair, neighborGCost + neighborHCost);
      }
    }

    var path = new List<Cell>();
    if (queue.IsEmpty()) { // path not found
      return path;
    }

    var destinationCellTimePair = queue.Peek();

    var parent = destinationCellTimePair;
    while (pathParents.ContainsKey(parent)) {
      gameManager.Grid.ReservationTable.Add(parent);
      path.Add(parent.cell);
      parent = pathParents[parent];
    }

    gameManager.Grid.ReservationTable.Add(parent);
    path.Add(parent.cell);
    path.Reverse();

    return path;
  }

  public IEnumerator MoveToCell(Cell destinationCell) {
    var path = GetPath(destinationCell);
    yield return StartCoroutine(MoveAlongPath(path));
  }

  public IEnumerator MoveAlongPath(List<Cell> path) {
    var delay = new WaitForSeconds(1/120f);

    var studentY = transform.position.y;

    for (int i = 0; i < path.Count - 1; i++) {
      var startPosition = new Vector3(path[i].WorldCoords.x, studentY, path[i].WorldCoords.y);
      var nextPosition = new Vector3(path[i + 1].WorldCoords.x, studentY, path[i + 1].WorldCoords.y);
      var currentPosition = startPosition;

      for (float j = 0f; j < 1; j += speed/120f) {
        transform.position = currentPosition;
        currentPosition = Vector3.Lerp(startPosition, nextPosition, j);
        yield return delay;
      }
    }
  }

  // Use this for initialization
  void Start () {
    gameManager = transform.root.GetComponent<GameManager>();
    // Debug.Log(transform.position);
  }
  
  // Update is called once per frame
  void Update () {
    
  }
}
