using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentFrustation {
  private float prevDistanceRemaining;
  private int timeUnitsFrustated;
  private Student student;
  private Color studentOriginalColor;

  public StudentFrustation(Student student) {
    this.student = student;
    prevDistanceRemaining = 0;
    timeUnitsFrustated = 0;
    studentOriginalColor = student.GetComponent<Renderer>().material.color;
  }

  public void UpdateFrustation(float distanceRemaining) {
    // Debug.Log(prevDistanceRemaining);

    if (prevDistanceRemaining != 0 && distanceRemaining >= prevDistanceRemaining)  {
      timeUnitsFrustated++;
    } else {
      timeUnitsFrustated = 0;
    }

    prevDistanceRemaining = distanceRemaining;
    updateStudentColor();
  }

  private void updateStudentColor() {
    Color color;

    if (timeUnitsFrustated >= 18) {
      color = Color.red;
    } else if (timeUnitsFrustated >= 1) {
      color = new Color(1, 0.54f, 0, 1);
    } else {
      color = studentOriginalColor;
    }

    student.GetComponent<Renderer>().material.color = color;
  }
}
