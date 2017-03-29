using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public Grid Grid { get; private set; }

  public Student studentPrefab;

  [Range(0.5f, 10f)]
  public float cellWidth;
  public LayerMask unwalkableMask;

  public int numStudents;
  public float studentSpeed;
  public int studentSearchDepth;

  public Professor[] Professors { get; private set; }
  public Plaque[] Plaques { get; private set; }

  // Use this for initialization
  void Start() {
    makeGrid();

    Professors = FindObjectsOfType<Professor>();
    Plaques = FindObjectsOfType<Plaque>();
     // sort the plaques by name so that they are ordered in a circle
    System.Array.Sort(Plaques, (a, b) => System.String.Compare(a.transform.name, b.transform.name));

    GenerateStudents();
  }

  private void makeGrid() {
    var floor = GameObject.Find("Floor");
    var floorBounds = floor.GetComponent<Renderer>().bounds;
    var floorDimensions = floorBounds.max - floorBounds.min;
    Grid = new Grid(new Vector2(floorDimensions.x, floorDimensions.z), floor.transform.position, cellWidth, unwalkableMask);
  }

  private Professor getRandomProfessor() {
    return Professors[Random.Range(0, Professors.Length)];
  }

  private void GenerateStudents() {
    var cornerCells = Grid.GetMainFloorCornerCells();

    int minX = (int)cornerCells[0].GridCoords.x;
    int minY = (int)cornerCells[0].GridCoords.y;
    int maxX = (int)cornerCells[1].GridCoords.x;
    int maxY = (int)cornerCells[1].GridCoords.y;

    var numStudentsGenerated = 0;
    var xDisplace = 0;
    for (var displace = 0; displace <= 1; displace++) {
      for (var y = minY; y <= maxY; y++, xDisplace = (xDisplace + 1) % 2) {
        for (var x = minX + displace; x <= maxX - 1 && numStudentsGenerated < numStudents; x += 2) {
          var randProfName = getRandomProfessor().Name;
          var student = Student.Instantiate(this, Grid.GridPointToCell(x + xDisplace, y), randProfName);
          StartCoroutine(student.Move());

          numStudentsGenerated++;
        }
      }
    }
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
