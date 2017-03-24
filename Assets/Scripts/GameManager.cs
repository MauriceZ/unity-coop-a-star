using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public Grid Grid { get; private set; }
  [Range(0.5f, 10f)]
  public float cellWidth;

  public LayerMask unwalkableMask;

  // Use this for initialization
  void Start () {
    var floor = GameObject.Find("Floor");
    var floorBounds = floor.GetComponent<Renderer>().bounds;
    var floorDimensions = floorBounds.max - floorBounds.min;
    Grid = new Grid(new Vector2(floorDimensions.x, floorDimensions.z), floor.transform.position, cellWidth, unwalkableMask);

    var student = GameObject.Find("Student");

    StartCoroutine(student.GetComponent<Student>().MoveToCell(Grid.WorldPointToCell(28, -10.4f)));
  }
  
  // Update is called once per frame
  void Update () {
    
  }

  // void OnDrawGizmos() {
  //   if (Grid != null) {
  //     foreach (Cell cell in Grid.Cells) {
  //       Gizmos.color = cell.IsWalkable ? Color.red : Color.white;
  //       Gizmos.DrawCube(new Vector3(cell.WorldCoords.x, 0, cell.WorldCoords.y), Vector3.one * (cellWidth - 0.1f));
  //     }
  //   }
  // }
}
