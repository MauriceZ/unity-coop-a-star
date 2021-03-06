﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class StudentState {
  public enum Status { SeekingPlaque, SeekingProf, ConsultingProf, ConsultingProfIdling, SeekingIdleCell, RandomIdling };

  public Status CurrentStatus { get; private set; }
  public Plaque CurrentPlaque { get; private set; }
  public Professor CurrentProf { get; private set; }
  public string CurrentProfName { get; private set; }
  public Cell DestinationCell { get; private set; }
  public bool IsIdling { get; private set; }
  public int IdleTime { get; private set; }

  private int currentPlaqueInd;
  private Student student;
  private GameManager gameManager;
  private List<Professor> last4Professors = new List<Professor>();

  public StudentState(Student student, string firstProfName) {
    CurrentProfName = firstProfName;
    this.student = student;
    gameManager = student.GameManager;
    IsIdling = false;
  }

  public void SeekPlaque() {
    var prof = GetMemorizedProfessor(CurrentProfName);
    if (prof != null) {
      CurrentProf = prof;
      SeekProf();
      return;
    }

    CurrentStatus = Status.SeekingPlaque;

    if (student.path.Count == 0) {
      var currentCell = gameManager.Grid.WorldPointToCell(student.transform.position.x, student.transform.position.z);
      currentPlaqueInd = student.getNearestPlaqueIndex(currentCell);
    } else {
      currentPlaqueInd = student.getNearestPlaqueIndex(student.path[student.path.Count - 1].cell);
    }

    CurrentPlaque = gameManager.Plaques[currentPlaqueInd];
    DestinationCell = CurrentPlaque.GetTargetCell();
  }

  public void SeekNextPlaque() {
    CurrentStatus = Status.SeekingPlaque;
    currentPlaqueInd = (currentPlaqueInd + 1) % gameManager.Plaques.Length;
    CurrentPlaque = gameManager.Plaques[currentPlaqueInd];
    DestinationCell = CurrentPlaque.GetTargetCell();
  }

  public void SeekProf() {
    CurrentStatus = Status.SeekingProf;
    DestinationCell = CurrentProf.GetTargetCell();
    CurrentPlaque = null;
  }

  public void ConsultProf() {
    CurrentStatus = Status.ConsultingProf;
    CurrentProfName = CurrentProf.GetNextProfName();
    CurrentProf = null;
  }

  public void ConsultProfIdle() {
    CurrentStatus = Status.ConsultingProfIdling;
    setIdling(8); // 8 timeunits ~ 1 s
  }

  public void SeekIdlingCell() {
    CurrentStatus = Status.SeekingIdleCell;
    DestinationCell = gameManager.Grid.GetRandomMainFloorCell();
  }

  public void RandomIdle() {
    CurrentStatus = Status.RandomIdling;
    setIdling(16); // 16 timeunits ~ 3 s
  }

  private void setIdling(int idleTime) {
    IsIdling = true;
    IdleTime = idleTime;
  }

  // called after the last state is finished
  // implementation of the behavior tree
  public void NextState() {
    switch (CurrentStatus) { // checking the previous status
      case Status.SeekingPlaque:
        if (last4Professors.Count >= 4)
          last4Professors.RemoveAt(0);
        last4Professors.Add(CurrentPlaque.GetProfessor());

        if (CurrentPlaque.HasProfessor(CurrentProfName)) { // plaque found
          CurrentProf = CurrentPlaque.GetProfessor();
          SeekProf();
        } else { // find next plaque
          SeekNextPlaque();
        }
        break;

      case Status.SeekingProf:
        ConsultProf();
        break;

      case Status.ConsultingProf:
        ConsultProfIdle();
        break;

      case Status.ConsultingProfIdling:
        IsIdling = false;
        if (Random.value < 0.5f) // 50/50 chance of idling or seeking the next prof
          SeekIdlingCell();
        else
          SeekPlaque();
        break;

      case Status.SeekingIdleCell:
        RandomIdle();
        break;

      case Status.RandomIdling:
        IsIdling = false;
        SeekPlaque();
        break;
    }
  }

  private Professor GetMemorizedProfessor(string profName) {
    foreach (var prof in last4Professors) {
      if (prof.Name == profName)
        return prof;
    }

    return null;
  }
}
