using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {
  public Cell[,] Cells { get; private set; }
  private Vector2 dimensions;
  private Vector2 center;
  private float cellWidth;

  public Grid(Vector2 dimensions, Vector2 center, float cellWidth, LayerMask unwalkableMask) {
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
        cell.IsWalkable = !(Physics.CheckSphere(new Vector3(worldCellX, 0, worldCellY), cellWidth/2 + 0.5f, unwalkableMask));
        Cells[y, x] = cell;
      }
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
