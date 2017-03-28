using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {
  public float speed;
  public int searchDepth;

  private GameManager gameManager;
  private Dictionary<string, Cell> teacherCells = new Dictionary<string, Cell>();
  private Dictionary<Cell, ReverseResumableAStar> rraDict = new Dictionary<Cell, ReverseResumableAStar>();

  public static Student Instantiate(GameManager gameManager, Cell startCell) {
    var student = Instantiate(gameManager.studentPrefab, gameManager.transform) as Student;

    student.transform.position = new Vector3(startCell.WorldCoords.x, gameManager.studentPrefab.transform.localScale.y , startCell.WorldCoords.y);
    student.gameManager = gameManager;
    student.speed = gameManager.studentSpeed;
    student.searchDepth = gameManager.studentSearchDepth;

    return student;
  }

  private List<Cell> GetPath(Cell startCell, Cell destinationCell, int startTimeUnit = 0) {
    var grid = gameManager.Grid;

    if (startCell == null)
      throw new System.NullReferenceException("Student is not in grid");

    ReverseResumableAStar rra = null;
    if (rraDict.ContainsKey(destinationCell)) {
      rra = rraDict[destinationCell];
    } else {
      rra = new ReverseResumableAStar(destinationCell, startCell);
      rraDict[destinationCell] = rra;
    }

    var queue = new PriorityQueue<Grid.CellTimePair>();
    var startCellTime = new Grid.CellTimePair(startCell, startTimeUnit);
    queue.Enqueue(startCellTime, 0);

    var pathParents = new Dictionary<Grid.CellTimePair, Grid.CellTimePair>();

    while (!queue.IsEmpty()) {
      var currentCellTimePair = queue.Peek();
      var currentCell = currentCellTimePair.cell;

      if (currentCellTimePair.timeUnit - startTimeUnit == searchDepth || currentCell == destinationCell)
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
    if (queue.IsEmpty()) {// path not found
      Debug.Log("empty path!!");
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
    var delay = new WaitForSeconds(1/120f);
    var grid = gameManager.Grid;
    var studentY = transform.position.y;

    var startCell = grid.WorldPointToCell(transform.position.x, transform.position.z);
    var currentCell = startCell;

    var path = GetPath(currentCell, destinationCell, 0);

    for (int i = 0; i < path.Count - 1; i++) {
      var startPosition = new Vector3(path[i].WorldCoords.x, studentY, path[i].WorldCoords.y);
      var nextPosition = new Vector3(path[i + 1].WorldCoords.x, studentY, path[i + 1].WorldCoords.y);
      var currentPosition = startPosition;

      for (float j = 0f; j < 1; j += speed/120f) {
        transform.position = currentPosition;
        currentPosition = Vector3.Lerp(startPosition, nextPosition, j);
        yield return delay;
      }

      if (i == (path.Count - 1) / 2) { // calculate the next partial path
        var partialPath = GetPath(path[path.Count - 1], destinationCell, path.Count - 1);
        partialPath.RemoveAt(0);
        path.AddRange(partialPath);
      }

      currentCell = path[i + 1];
    }
  }
}
