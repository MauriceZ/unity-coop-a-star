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

  public List<Cell> GetNeighbors(int timeunit) {
    var neighbors = new List<Cell>();

    var x = (int)GridCoords.x;
    var y = (int)GridCoords.y;

    for (int i = -1; i <= 1; i++) {
      for (int j = -1; j <= 1; j++) {

        if (i != 0 && j != 0) { // is diagonal neighbor
          continue;
          
          var horiCell = grid.GridPointToCell(x + i, y);
          var vertCell = grid.GridPointToCell(x, y + j);

          if (
            horiCell == null || vertCell == null || 
            grid.ReservationTable.Contains(new Grid.ReservationKey(horiCell, timeunit)) || 
            grid.ReservationTable.Contains(new Grid.ReservationKey(vertCell, timeunit))
          ) continue; // prevent corner cutting when there's an obstacle
        }

        var cell = grid.GridPointToCell(x + i, y + j);
        if (cell != null && !grid.ReservationTable.Contains(new Grid.ReservationKey(cell, timeunit)))
          neighbors.Add(cell);
      }
    }

    return neighbors;
  }

  public static float Distance(Cell c1, Cell c2) {
    return Vector2.Distance(c1.WorldCoords, c2.WorldCoords);
  }
}
