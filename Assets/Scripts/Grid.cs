using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {
  public Cell[,] Cells { get; private set; }
  private Vector2 dimensions;
  private Vector2 center;
  private float cellWidth;

  public struct ReservationKey {
    public Cell cell;
    public int timeUnit;

    public ReservationKey(Cell c, int t) {
      cell = c;
      timeUnit = t;
    }
  }

  public HashSet<ReservationKey> ReservationTable { get; private set; }

  public Grid(Vector2 dimensions, Vector2 center, float cellWidth, LayerMask unwalkableMask) {
    ReservationTable = new HashSet<ReservationKey>();

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

    var queue = new PriorityQueue<Cell>();
    // var queue = new Queue<Cell>(); // for BFS
    queue.Enqueue(startCell, 0);

    var pathParents = new Dictionary<Cell, Cell>();
    var visited = new HashSet<Cell>();
    var gCost = new Dictionary<Cell, int>();

    gCost[startCell] = 0;

    while (!queue.IsEmpty()) {
      var currentCell = queue.Dequeue();
      visited.Add(currentCell);

      if (currentCell == destinationCell) {
        break;
      }

      var neighborGCost = gCost[currentCell] + 1;
      foreach (var neighborCell in currentCell.GetNeighbors(neighborGCost)) {
        if (!visited.Contains(neighborCell)) {
          // gCost[neighborCell] = gCost[currentCell] + distanceBetweenCells(currentCell, neighborCell);
          gCost[neighborCell] = neighborGCost;
          var hCost = Cell.Distance(neighborCell, destinationCell); // using the straight line distance as a heuristic
          pathParents[neighborCell] = currentCell;

          queue.Enqueue(neighborCell, gCost[neighborCell] + hCost);
        }
      }
    }

    var path = new List<Cell>();
    if (!pathParents.ContainsKey(destinationCell)) { // path not found
      return path;
    }

    path.Add(destinationCell);
    var parent = destinationCell;

    while (pathParents.ContainsKey(parent)) {
      path.Add(parent);
      parent = pathParents[parent];
    }

    path.Add(parent);
    path.Reverse();

    foreach (var pathCell in path)
      ReservationTable.Add(new ReservationKey(pathCell, gCost[pathCell]));

    return path;
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
