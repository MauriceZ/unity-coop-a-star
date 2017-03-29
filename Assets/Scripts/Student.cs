using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {
  public float speed;
  public int searchDepth;
  public GameManager GameManager { get; private set; }

  private Dictionary<Cell, ReverseResumableAStar> rraDict = new Dictionary<Cell, ReverseResumableAStar>();
  private List<Grid.CellTimePair> path;
  private StudentState state;
  private int currentTimeUnit = 0;

  public static Student Instantiate(GameManager gameManager, Cell startCell, string firstProfName) {
    var student = Instantiate(gameManager.studentPrefab, gameManager.transform) as Student;

    student.transform.position = new Vector3(startCell.WorldCoords.x, gameManager.studentPrefab.transform.localScale.y , startCell.WorldCoords.y);
    student.GameManager = gameManager;
    student.speed = gameManager.studentSpeed;
    student.searchDepth = gameManager.studentSearchDepth;
    student.state = new StudentState(student, firstProfName);

    return student;
  }

  private List<Grid.CellTimePair> GetPath(Cell startCell, Cell destinationCell, int startTimeUnit = 0) {
    var grid = GameManager.Grid;

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

    var path = new List<Grid.CellTimePair>();
    if (queue.IsEmpty()) {// path not found
      Debug.Log("empty path!!");
      return path;
    }

    var destinationCellTimePair = queue.Peek();

    var parent = destinationCellTimePair;
    while (pathParents.ContainsKey(parent)) {
      GameManager.Grid.ReservationTable.Add(parent);
      path.Add(parent);
      parent = pathParents[parent];
    }

    path.Reverse();
    return path;
  }

  public IEnumerator Move() {
    var delay = new WaitForSeconds(1/120f);
    var grid = GameManager.Grid;
    var studentY = transform.position.y;

    state.SeekPlaque();
    currentTimeUnit = 0;

    while (true) {
      var startCell = grid.WorldPointToCell(transform.position.x, transform.position.z);
      var currentCell = startCell;

      path = new List<Grid.CellTimePair>();
      path.Add(new Grid.CellTimePair(startCell, ++currentTimeUnit));
      path.AddRange(GetPath(currentCell, state.DestinationCell, currentTimeUnit));

      for (int i = 0; i < path.Count - 1; i++) {
        var startPosition = new Vector3(path[i].cell.WorldCoords.x, studentY, path[i].cell.WorldCoords.y);
        var nextPosition = new Vector3(path[i + 1].cell.WorldCoords.x, studentY, path[i + 1].cell.WorldCoords.y);
        var currentPosition = startPosition;

        for (float j = 0f; j < 1; j += speed/120f) {
          transform.position = currentPosition;
          currentPosition = Vector3.Lerp(startPosition, nextPosition, j);
          yield return delay;
        }

        if (i == (path.Count - 1) / 2) { // calculate the next partial path
          var partialPath = GetPath(path[path.Count - 1].cell, state.DestinationCell, path[path.Count - 1].timeUnit);
          path.AddRange(partialPath);
        }

        currentTimeUnit = path[i].timeUnit;
        // Debug.Log(path[i].timeUnit);
      }

      state.Behave();
    }
  }

  public int getNearestPlaqueIndex() { // approximate since we are using straight line distance
    var plaques = GameManager.Plaques;
    var currentPlaqueInd = 0;
    var currentPlaque = plaques[currentPlaqueInd];
    var currentPosition = transform.position;
    var currentDistance = Vector3.Distance(currentPlaque.transform.position, currentPosition);

    for (int i = 1; i < plaques.Length; i++) {
      var plaque = plaques[i];
      var plaqueDistance = Vector3.Distance(plaque.transform.position, currentPosition);

      if (plaqueDistance < currentDistance) {
        currentPlaque = plaque;
        currentPlaqueInd = i;
        currentDistance = plaqueDistance;
      }
    }

    return currentPlaqueInd;
  }
}
