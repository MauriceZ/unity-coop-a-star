using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class StudentState {
  public enum Status { SeekingPlaque, SeekingProf, ConsultingProf, RandomIdling };

  public Status CurrentStatus { get; private set; }
  public Plaque CurrentPlaque { get; private set; }
  public Professor CurrentProf { get; private set; }
  public string CurrentProfName { get; private set; }
  public Cell DestinationCell { get; private set; }

  private int currentPlaqueInd;
  private Student student;
  private GameManager gameManager;
  private List<Professor> last4Professors = new List<Professor>();

  public StudentState(Student student, string firstProfName) {
    CurrentProfName = firstProfName;
    this.student = student;
    gameManager = student.GameManager;
  }

  public void SeekPlaque() {
    Debug.Log("Seeking Plaque");

    var prof = GetMemorizedProfessor(CurrentProfName);
    if (prof != null) {
      CurrentProf = prof;
      Debug.Log("Memorized");
      SeekProf();
      return;
    }

    CurrentStatus = Status.SeekingPlaque;
    currentPlaqueInd = student.getNearestPlaqueIndex();
    CurrentPlaque = gameManager.Plaques[currentPlaqueInd];
    DestinationCell = CurrentPlaque.GetTargetCell();
  }

  public void SeekNextPlaque() {
    Debug.Log("Seeking Plaque");
    CurrentStatus = Status.SeekingPlaque;
    currentPlaqueInd = (currentPlaqueInd + 1) % gameManager.Plaques.Length;
    CurrentPlaque = gameManager.Plaques[currentPlaqueInd];
    DestinationCell = CurrentPlaque.GetTargetCell();
  }

  public void SeekProf() {
    Debug.Log("Seeking Prof");
    CurrentStatus = Status.SeekingProf;
    DestinationCell = CurrentProf.GetTargetCell();
    CurrentPlaque = null;
  }

  public void ConsultProf() {
    Debug.Log("Consulting Prof");
    CurrentStatus = Status.ConsultingProf;
    CurrentProfName = CurrentProf.GetNextProfName();
    CurrentProf = null;
    // TODO idle for 0.5 seconds
  }

  public void RandomIdle() {
    Debug.Log("Randomly Idling");
    CurrentStatus = Status.RandomIdling;
    DestinationCell = gameManager.Grid.GetRandomMainFloorCell();
    // idle for 2-3 seconds
  }

  // called after the last status is finished
  public void Behave() {
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
        if (Random.value < 0.5f) // 50/50 chance of idling or seeking the next prof
          RandomIdle();
        else
          SeekPlaque();
        break;

      case Status.RandomIdling:
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
