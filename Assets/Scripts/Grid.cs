using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {
  public Cell[,] Cells { get; private set; }
  private Vector2 dimensions;
  private Vector2 center;
  private float cellWidth;

  public enum Heuristic { ManhanttanDistance, StraightLineDistance, TrueDistance };
  private Heuristic heuristic = Heuristic.ManhanttanDistance;

  public struct CellTimePair {
    public Cell cell;
    public int timeUnit;

    public CellTimePair(Cell c, int t) {
      cell = c;
      timeUnit = t;
    }
  }

  public HashSet<CellTimePair> ReservationTable { get; private set; }

  public Grid(Vector2 dimensions, Vector2 center, float cellWidth, LayerMask unwalkableMask) {
    ReservationTable = new HashSet<CellTimePair>();

    this.dimensions = dimensions;
    this.center = center;
    this.cellWidth = cellWidth;

    var gridSize = new Vector2(dimensions.x / cellWidth, dimensions.y / cellWidth);
    var gridX = Mathf.CeilToInt(gridSize.x);
    var gridY = Mathf.CeilToInt(gridSize.y);

    Cells = new Cell[gridY, gridX];

    var lowerLeftCorner = center - dimensions/2;

    for (int y = 0; y < gridSize.y; y++) {
      for (int x = 0; x < gridSize.x; x++) {
        var worldCellX = lowerLeftCorner.x + x * cellWidth + cellWidth / 2;
        var worldCellY = lowerLeftCorner.y + y * cellWidth + cellWidth / 2;

        var cell = new Cell(this, new Vector2(worldCellX, worldCellY), new Vector2(x, y));
        cell.IsWalkable = !(Physics.CheckSphere(new Vector3(worldCellX, 0, worldCellY), cellWidth/2, unwalkableMask));
        Cells[y, x] = cell;
      }
    }
  }

  public List<Cell> GetPath(Cell startCell, Cell destinationCell) {
    if (startCell == destinationCell || destinationCell == null) {
      return new List<Cell>();
    }

    var queue = new PriorityQueue<CellTimePair>();
    var startCellTime = new CellTimePair(startCell, 0);
    queue.Enqueue(startCellTime, 0);

    var pathParents = new Dictionary<CellTimePair, CellTimePair>();

    while (!queue.IsEmpty()) {
      var currentCellTimePair = queue.Peek();
      var currentCell = currentCellTimePair.cell;

      if (currentCell == destinationCell)
        break;

      queue.Dequeue();

      var neighborGCost = currentCellTimePair.timeUnit + 1;
      foreach (var neighborCell in currentCell.GetAvailableNeighbors(neighborGCost)) {
        var neighborCellTimePair = new CellTimePair(neighborCell, neighborGCost);

        var neighborHCost = GetHeuristicValue(neighborCell, destinationCell); // using the straight line distance as the heuristic
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
      ReservationTable.Add(parent);
      path.Add(parent.cell);
      parent = pathParents[parent];
    }

    ReservationTable.Add(parent);
    path.Add(parent.cell);
    path.Reverse();

    return path;
  }

  private float GetHeuristicValue(Cell c1, Cell c2) {
    switch (heuristic) {
      case Heuristic.StraightLineDistance:
        return Cell.StraightLineDistance(c1, c2);
      case Heuristic.ManhanttanDistance:
      default:
        return Cell.ManhanttanDistance(c1, c2);
    }
  }

  public Cell WorldPointToCell(Vector2 position) {
    var worldLength = position - (center - dimensions / 2);
    var gridCoordsX = Mathf.FloorToInt(worldLength.x / cellWidth);
    var gridCoordsY = Mathf.FloorToInt(worldLength.y / cellWidth);

    return GridPointToCell(gridCoordsX, gridCoordsY);
  }

  public Cell WorldPointToCell(float x, float y) {
    return WorldPointToCell(new Vector2(x, y));
  }

  public Cell GridPointToCell(float x, float y) {
    return ContainsGridCoords(x, y) && Cells[(int)y, (int)x].IsWalkable ? Cells[(int)y, (int)x] : null;
  }

  public bool ContainsGridCoords(float x, float y) {
    return y < Cells.GetLength(0) && x < Cells.GetLength(1);
  }
}
