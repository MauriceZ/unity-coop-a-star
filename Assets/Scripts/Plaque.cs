using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plaque : MonoBehaviour {
  public Grid.Direction direction;
  public Professor professor;

  private GameManager gameManager;
  private Cell targetCell;

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

  public bool HasProfessor(string profName) {
    return professor.gameObject.transform.name == profName;
  }

  public Professor GetProfessor() {
    return professor;
  }
}
