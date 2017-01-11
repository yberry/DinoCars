using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarTurn", menuName ="CND")]
public class CarTurnPreset
     : ScriptableObject {

    //Contrôle Voiture Classique
    [Header("Controle Classique")]
    public float maxTurnAngleC=60f;
    [Range(0, 360), Tooltip("Max degrees per second")]
    public float turnSpeedC = 1f;
    [Range(0,1)]
    public float tractionControlC;
    [Range(0, 1)]
    public float driftControlC;
    //Contrôle Voiture En Drift
    [Header("Controle Drift")]
    public float maxTurnAngleD = 60f;
    [Range(0, 360), Tooltip("Max degrees per second")]
    public float turnSpeedD = 1f;
    [Range(0, 1)]
    public float tractionControlD;
    [Range(0, 1)]
    public float driftControlD;
}
