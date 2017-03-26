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

    var student1 = GameObject.Find("Student1");
    var student2 = GameObject.Find("Student2");

    StartCoroutine(student1.GetComponent<Student>().MoveToCell(Grid.WorldPointToCell(-15f, 0f)));
    StartCoroutine(student2.GetComponent<Student>().MoveToCell(Grid.WorldPointToCell(0, 15f)));
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
