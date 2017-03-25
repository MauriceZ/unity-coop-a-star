using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {
  public float speed;

  private GameManager gameManager;
  private Dictionary<string, Cell> teacherCells = new Dictionary<string, Cell>();

  private List<Cell> GetPath(Cell destinationCell) {
    var grid = gameManager.Grid;
    var startCell = grid.WorldPointToCell(transform.position.x, transform.position.z);

    if (startCell == null)
      throw new System.NullReferenceException("Student is not in grid");

    return grid.GetPath(startCell, destinationCell);
  }

  public IEnumerator MoveToCell(Cell destinationCell) {
    var path = GetPath(destinationCell);
    yield return StartCoroutine(MoveAlongPath(path));
  }

  public IEnumerator MoveAlongPath(List<Cell> path) {
    var delay = new WaitForSeconds(1/120f);

    var studentY = transform.position.y;

    for (int i = 0; i < path.Count - 1; i++) {
      var startPosition = new Vector3(path[i].WorldCoords.x, studentY, path[i].WorldCoords.y);
      var nextPosition = new Vector3(path[i + 1].WorldCoords.x, studentY, path[i + 1].WorldCoords.y);
      var currentPosition = startPosition;

      for (float j = 0f; j < 1; j += speed/120f) {
        transform.position = currentPosition;
        currentPosition = Vector3.Lerp(startPosition, nextPosition, j);
        yield return delay;
      }
    }
  }

  // Use this for initialization
  void Start () {
    gameManager = transform.root.GetComponent<GameManager>();
    Debug.Log(transform.position);
  }
  
  // Update is called once per frame
  void Update () {
    
  }
}
