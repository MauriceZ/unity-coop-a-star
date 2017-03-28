﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Professor : MonoBehaviour {
  public Grid.Direction direction;
  public Plaque plaque;

  private GameManager gameManager;
  private Cell targetCell;

  public string Name { 
    get { return gameObject.transform.name; }
  }

  // Use this for initialization
  void Start () {
    gameManager = transform.root.GetComponent<GameManager>();
  }

  public Cell GetTargetCell() {
    if (targetCell == null) {
      var cell = gameManager.Grid.WorldPointToCell(transform.position.x, transform.position.z, false);
      var targetCellCoords = cell.GridCoords + Grid.DirectionToVector(direction);
      targetCell = gameManager.Grid.GridPointToCell(targetCellCoords.x, targetCellCoords.y);
    }

    return targetCell;
  }
}