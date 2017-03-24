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

  public List<Cell> GetNeighbors() {
    var neighbors = new List<Cell>();

    var x = (int)GridCoords.x;
    var y = (int)GridCoords.y;

    for (int i = -1; i <= 1; i++) {
      for (int j = -1; j <= 1; j++) {
        if (i == 0 && j == 0)
          continue;

        var cell = grid.GridPointToCell(x + i, y + j);
        if (cell != null)
          neighbors.Add(cell);
      }
    }

    return neighbors;
  }
}
