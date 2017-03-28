using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public Grid Grid { get; private set; }

  public Student studentPrefab;

  [Range(0.5f, 10f)]
  public float cellWidth;
  public LayerMask unwalkableMask;

  [Range(1, 60)]
  public int numStudents;
  public float studentSpeed;
  public int studentSearchDepth;

  private Professor[] professors;

  // Use this for initialization
  void Start() {
    makeGrid();

    professors = FindObjectsOfType<Professor>();

    GenerateStudents();

    /*var student1 = GameObject.Find("Student1");
    var student2 = GameObject.Find("Student2");

    StartCoroutine(student1.GetComponent<Student>().MoveToCell(Grid.WorldPointToCell(-15f, 0f)));
    StartCoroutine(student2.GetComponent<Student>().MoveToCell(Grid.WorldPointToCell(0, 15f)));*/
  }

  private void makeGrid() {
    var floor = GameObject.Find("Floor");
    var floorBounds = floor.GetComponent<Renderer>().bounds;
    var floorDimensions = floorBounds.max - floorBounds.min;
    Grid = new Grid(new Vector2(floorDimensions.x, floorDimensions.z), floor.transform.position, cellWidth, unwalkableMask);
  }

  private Professor getRandomProfessor() {
    return professors[Random.Range(0, professors.Length)];
  }

  private void GenerateStudents() {
    var cornerCells = getStartingFloorCornerCells();

    int minX = (int)cornerCells[0].GridCoords.x;
    int minY = (int)cornerCells[0].GridCoords.y;
    int maxX = (int)cornerCells[1].GridCoords.x;
    int maxY = (int)cornerCells[1].GridCoords.y;

    var numStudentsGenerated = 0;

    var randProf = professors[0];

    var xDisplace = 0;
    for (var displace = 0; displace <= 1; displace++) {
      for (var y = minY; y <= maxY; y++, xDisplace = (xDisplace + 1) % 2) {
        for (var x = minX + displace; x <= maxX - 1 && numStudentsGenerated < numStudents; x += 2) {
          var student = Student.Instantiate(this, Grid.GridPointToCell(x + xDisplace, y));

          StartCoroutine(student.MoveToCell(randProf.GetTargetCell()));

          numStudentsGenerated++;
        }
      }
    }
  }

  private Cell[] getStartingFloorCornerCells() {
    var startFloor = GameObject.Find("StudentStartFloor");
    var floorBounds = startFloor.GetComponent<Renderer>().bounds;

    return new [] {
      Grid.WorldPointToCell(floorBounds.min.x, floorBounds.min.z),
      Grid.WorldPointToCell(floorBounds.max.x, floorBounds.max.z)
    };
  }

  /*void OnDrawGizmos() {
    if (Grid != null) {
      foreach (Cell cell in Grid.Cells) {
        Gizmos.color = cell.IsWalkable ? Color.red : Color.white;
        Gizmos.DrawCube(new Vector3(cell.WorldCoords.x, 0, cell.WorldCoords.y), Vector3.one * (cellWidth - 0.1f));
      }
    }
  }*/
}
