using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {
  public Vector2 WorldCoords { get; private set; }
  public Vector2 GridCoords { get; private set; }
  public bool IsWalkable { get; set; }

  private Grid grid;

  public Cell(Grid grid, Vector2 worldCoords, Vector2 gridCoords) {
    this.grid = grid;
    WorldCoords = worldCoords;
    GridCoords = gridCoords;
  }

  public List<Cell> GetAvailableNeighbors(int timeunit) {
    var allNeighbors = GetNeighbors();
    var availableNeighbors = new List<Cell>();

    foreach (var neighbor in allNeighbors) {
      if (!grid.ReservationTable.ContainsKey(new Grid.CellTimePair(neighbor, timeunit))) {
        // head to head collision prevention

        var neighborReservation = new Grid.CellTimePair(neighbor, timeunit - 1);
        if (grid.ReservationTable.ContainsKey(neighborReservation)) {
          var neighborStudent = grid.ReservationTable[neighborReservation];
          var cellNextReservation = new Grid.CellTimePair(this, timeunit);
          
          // head-to-head collision detected
          if (grid.ReservationTable.ContainsKey(cellNextReservation) && grid.ReservationTable[cellNextReservation] == neighborStudent) {
            continue;
          }
        }

        availableNeighbors.Add(neighbor);
      }
    }

    return availableNeighbors;
  }

  // Gets all neighbors, whether or not it's reserved
  public List<Cell> GetNeighbors() {
    var neighbors = new List<Cell>();

    var x = (int)GridCoords.x;
    var y = (int)GridCoords.y;

    for (int i = -1; i <= 1; i++) {
      for (int j = -1; j <= 1; j++) {
        if (i != 0 && j != 0) // no diagonal movements
          continue;

        var cell = grid.GridPointToCell(x + i, y + j);
        if (cell != null)
          neighbors.Add(cell);
      }
    }

    return neighbors;
  }

  public static float StraightLineDistance(Cell c1, Cell c2) {
    return Vector2.Distance(c1.GridCoords, c2.GridCoords);
  }

  public static float ManhanttanDistance(Cell c1, Cell c2) {
    return Mathf.Abs(c1.GridCoords.x - c2.GridCoords.x) + Mathf.Abs(c1.GridCoords.y - c2.GridCoords.y);
  }
}
