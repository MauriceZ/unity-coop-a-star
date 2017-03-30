using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {
  public GameManager GameManager { get; private set; }
  public List<Grid.CellTimePair> path = new List<Grid.CellTimePair>();

  private float speed;
  private int searchDepth;
  private Dictionary<Cell, ReverseResumableAStar> rraDict = new Dictionary<Cell, ReverseResumableAStar>();
  private int currentTimeUnit = 0;

  private StudentState state;
  private StudentFrustation frustation;

  // Student factory
  public static Student Instantiate(GameManager gameManager, Cell startCell, string firstProfName) {
    var student = Instantiate(gameManager.studentPrefab, gameManager.transform) as Student;

    student.transform.position = new Vector3(startCell.WorldCoords.x, gameManager.studentPrefab.transform.localScale.y , startCell.WorldCoords.y);
    student.GameManager = gameManager;
    student.speed = gameManager.studentSpeed;
    student.searchDepth = gameManager.studentSearchDepth;
    student.state = new StudentState(student, firstProfName);
    student.frustation = new StudentFrustation(student);

    return student;
  }

  private List<Grid.CellTimePair> GetPath(Cell startCell, Cell destinationCell, int startTimeUnit = 0) {
    // implementation of A star
    // a timeunit represents one movement

    var grid = GameManager.Grid;

    if (startCell == null)
      throw new System.NullReferenceException("Student is not in grid");

    var rra = getRRA(startCell, destinationCell);
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
    if (queue.IsEmpty()) { // path not found
      path.Add(new Grid.CellTimePair(startCell, startTimeUnit + 1));
      path.Add(new Grid.CellTimePair(startCell, startTimeUnit + 2));
      path.Add(new Grid.CellTimePair(startCell, startTimeUnit + 3));
      return path;
    }

    var destinationCellTimePair = queue.Peek();

    var parent = destinationCellTimePair;
    while (pathParents.ContainsKey(parent)) {
      GameManager.Grid.ReservationTable.Add(parent, this);
      path.Add(parent);
      parent = pathParents[parent];
    }

    path.Reverse();
    return path;
  }

  private List<Grid.CellTimePair> GetIdlePath(Cell idleCell, int idleTime, int startTimeUnit = 0) {
    // a star that favors staying at the current cell

    var grid = GameManager.Grid;

    if (idleCell == null)
      throw new System.NullReferenceException("Student is not in grid");

    var queue = new PriorityQueue<Grid.CellTimePair>();
    var startCellTime = new Grid.CellTimePair(idleCell, startTimeUnit);
    queue.Enqueue(startCellTime, 0);

    var pathParents = new Dictionary<Grid.CellTimePair, Grid.CellTimePair>();
    var currentIdleTime = 0;

    while (!queue.IsEmpty()) {
      var currentCellTimePair = queue.Peek();
      var currentCell = currentCellTimePair.cell;

      if (currentCell == idleCell && ++currentIdleTime >= idleTime)
        break;

      queue.Dequeue();

      var time = currentCellTimePair.timeUnit + 1;
      foreach (var neighborCell in currentCell.GetAvailableNeighbors(time)) {
        var neighborCellTimePair = new Grid.CellTimePair(neighborCell, time);
        var neighborHCost = Cell.ManhanttanDistance(neighborCell, idleCell);
        pathParents[neighborCellTimePair] = currentCellTimePair;

        queue.Enqueue(neighborCellTimePair, neighborHCost);
      }
    }

    var path = new List<Grid.CellTimePair>();
    if (queue.IsEmpty()) { // path not found
      path.Add(new Grid.CellTimePair(idleCell, startTimeUnit + 1));
      path.Add(new Grid.CellTimePair(idleCell, startTimeUnit + 2));
      path.Add(new Grid.CellTimePair(idleCell, startTimeUnit + 3));
      return path;
    }

    var destinationCellTimePair = queue.Peek();

    var parent = destinationCellTimePair;
    while (pathParents.ContainsKey(parent)) {
      GameManager.Grid.ReservationTable.Add(parent, this);
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

    var startCell = grid.WorldPointToCell(transform.position.x, transform.position.z);

    path = new List<Grid.CellTimePair>();
    path.Add(new Grid.CellTimePair(startCell, ++currentTimeUnit));
    path.AddRange(GetPath(startCell, state.DestinationCell, currentTimeUnit));    

    var idlePaths = new Queue<List<Grid.CellTimePair>>(); // used to determine when student is idling
    var isIdling = false;
    var destinationCells = new Queue<Cell>();
    destinationCells.Enqueue(state.DestinationCell);

    for (int i = 0; i < path.Count - 1; i++) {
      var startPosition = new Vector3(path[i].cell.WorldCoords.x, studentY, path[i].cell.WorldCoords.y);
      var nextPosition = new Vector3(path[i + 1].cell.WorldCoords.x, studentY, path[i + 1].cell.WorldCoords.y);
      var currentPosition = startPosition;

      if (idlePaths.Count > 0) {
        var nextIdlePath = idlePaths.Peek();

        if (path[i + 1].timeUnit == nextIdlePath[0].timeUnit) {
          isIdling = true;
        } else if (isIdling && path[i].timeUnit == nextIdlePath[nextIdlePath.Count - 1].timeUnit) {
          isIdling = false;
          idlePaths.Dequeue();
        }
      }

      if (!isIdling) {
        frustation.UpdateFrustation(getRRA(path[i + 1].cell, destinationCells.Peek()).GetTrueDistance(path[i + 1].cell));
      }
      
      for (float j = 0f; j < 1; j += speed/120f) {
        transform.position = currentPosition;
        currentPosition = Vector3.Lerp(startPosition, nextPosition, j);
        yield return delay;
      }

      // calculate the next partial path when student has reached half its current path
      if (i == (path.Count - 1) / 2) {
        var origPathCount = path.Count;

        while (path.Count - origPathCount <= 1) { // path must increase by at least 2 due to integer division offset
          if (getLastPathItem().cell != state.DestinationCell) {
            path.AddRange(GetPath(getLastPathItem().cell, state.DestinationCell, getLastPathItem().timeUnit));
          } else {
            state.NextState();

            if (state.IsIdling) {
              var idlePath = GetIdlePath(getLastPathItem().cell, state.IdleTime, getLastPathItem().timeUnit);
              path.AddRange(idlePath);
              idlePaths.Enqueue(idlePath);
            } else {
              path.AddRange(GetPath(getLastPathItem().cell, state.DestinationCell, getLastPathItem().timeUnit));
              destinationCells.Enqueue(state.DestinationCell);
            }
          }
        }
      }

      currentTimeUnit = path[i].timeUnit;

      if (path[i + 1].cell == destinationCells.Peek()) {
        destinationCells.Dequeue();
      }
    }
  }

  private Grid.CellTimePair getLastPathItem() {
    return path[path.Count - 1];
  }

  public int getNearestPlaqueIndex(Cell currentCell) { // approximate since we are using straight line distance
    // gets the plaque nearest to where the student currently is

    var plaques = GameManager.Plaques;
    var currentPlaqueInd = 0;
    var currentPlaque = plaques[currentPlaqueInd];
    var currentPosition = currentCell.WorldCoords;
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

  private ReverseResumableAStar getRRA(Cell startCell, Cell destinationCell) {
    // if the RRA destination cell is too far from the current cell (8 cells), then it is restarted

    if (rraDict.ContainsKey(destinationCell)) {
      var rra = rraDict[destinationCell];
      if (rra.HasCell(startCell) || Cell.ManhanttanDistance(rra.DestinationCell, startCell) <= 8) {
        return rra;
      }
    }

    var newRRA = new ReverseResumableAStar(destinationCell, startCell);
    rraDict[destinationCell] = newRRA;
    return newRRA;
  }
}
