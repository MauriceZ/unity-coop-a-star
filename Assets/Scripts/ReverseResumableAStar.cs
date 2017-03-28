using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseResumableAStar {
  private Grid grid;
  private Cell destinationCell;
  private HashSet<Cell> visited;
  private PriorityQueue<Cell> queue;
  private Dictionary<Cell, float> gCosts;

  public ReverseResumableAStar(Cell startCell, Cell destinationCell) {
    this.destinationCell = destinationCell;
    visited = new HashSet<Cell>();
    queue = new PriorityQueue<Cell>();
    queue.Enqueue(startCell, 0);
    gCosts = new Dictionary<Cell, float>();
    gCosts[startCell] = 0;
  }

  public float GetTrueDistance(Cell targetCell) {
    if (targetCell == null) {
      throw new System.ArgumentNullException();
    }

    while (!queue.IsEmpty()) {
      if (gCosts.ContainsKey(targetCell))
        return gCosts[targetCell];

      var currentCell = queue.Dequeue();
      visited.Add(currentCell);

      var neighborGCost = gCosts[currentCell] + 1;

      var neighborCells = currentCell.GetNeighbors();
      neighborCells.Reverse();

      foreach (var neighborCell in neighborCells) {
        if (!visited.Contains(neighborCell)) {
          gCosts[neighborCell] = neighborGCost;
          var neighborHCost = Cell.ManhanttanDistance(neighborCell, destinationCell);
          queue.Enqueue(neighborCell, neighborGCost + neighborHCost);
        }
      }
    }

    if (gCosts.ContainsKey(targetCell))
      return gCosts[targetCell];

    throw new System.ArgumentException("Target cell " + targetCell.GridCoords + " was not found");
  }
}
