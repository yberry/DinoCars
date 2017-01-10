using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFOVTest : MonoBehaviour {


    [Header("Paramètres FOV")]
    public float FOVBoost = 90;
    public float FOVDefault = 50;
    public float FOVEvolution = 5;
    [Header("Paramètres Particules Boost")]
    public float pStartLifeTimeDefault = 0.1f;
    public float pStartSpeedDefault = 0.1f;
    public float pStartSizeDefault = 0.1f;
    public float pStartLifeTimeBoost = 0.2f;
    public float pStartSpeedBoost = 50.0f;
    public float pStartSizeBoost = 0.5f;
    public float pStartLifeTimeEvolution = 0.1f;
    public float pStartSpeedEvolution = 0.1f;
    public float pStartSizeEvolution= 0.1f;
    [Header("Paramètres Lignes de Forces")]
    public int flMaxParticulesDefault = 0;
    public int flMaxParticulesBoost = 10;
    public int flMaxParticulesEvolution = 1;
    [Header("Paramètres Jauge de Boost")]
    public float jaugeDeBoostInit = 50;
    public int delais = 50;
    float jaugeDeBoost;
    [Header("Paramètres Screen Shake")]
    public float shakePower= 2.0f;
    [Header("GameObjects")]
    public ParticleSystem particules2;
    public ParticleSystem particules1;
    public ParticleSystem ForceLines;
    public Camera cam;
    public Slider slider;
    private ScreenShaker shaker;
    bool boostUtilisé = false;
    int temps = 0;
    bool pressed = false;
    bool reload = false;

	// Use this for initialization
	void Start () {
        jaugeDeBoost = jaugeDeBoostInit;
        this.shaker = (ScreenShaker)this.GetComponent<ScreenShaker>();
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void Boost(bool boutonA)
    {
        if (Input.GetKeyDown(KeyCode.Space) || boutonA)
        {
            pressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Space) || boutonA == false)
        {
            pressed = false;
        }
        if (boostUtilisé == false && pressed == true)
        {
            this.shaker.Shake(shakePower, true);
            reload = true;
            if (cam.fieldOfView < FOVBoost)
            {
                if (ForceLines.maxParticles < flMaxParticulesBoost)
                {
                    ForceLines.maxParticles = ForceLines.maxParticles + flMaxParticulesEvolution;
                }
                if (particules1.startSpeed < pStartSpeedBoost)
                {
                    particules1.startSpeed = particules1.startSpeed + pStartSpeedEvolution;
                    particules2.startSpeed = particules2.startSpeed + pStartSpeedEvolution;
                }
                if (particules1.startLifetime < pStartLifeTimeBoost)
                {
                    particules1.startLifetime = particules1.startLifetime + pStartLifeTimeEvolution;
                    particules2.startLifetime = particules2.startLifetime + pStartLifeTimeEvolution;
                }
                if (particules1.startSize < pStartSizeBoost)
                {
                    particules1.startSize = particules1.startSize + pStartSizeEvolution;
                    particules2.startSize = particules2.startSize + pStartSizeEvolution;
                }
                cam.fieldOfView = cam.fieldOfView + FOVEvolution;

            }
            if (cam.fieldOfView >= FOVBoost)
            {
                if (jaugeDeBoost > 0)
                {
                    jaugeDeBoost--;
                }
                else {
                    boostUtilisé = true;
                }
            }
        }
        else
        {
            if (cam.fieldOfView > FOVDefault)
            {
                this.shaker.Shake(shakePower, false);
                if (ForceLines.maxParticles > flMaxParticulesDefault)
                {
                    ForceLines.maxParticles = ForceLines.maxParticles - flMaxParticulesEvolution;
                }
                if (particules1.startSpeed > pStartSpeedDefault)
                {
                    particules1.startSpeed = particules1.startSpeed - pStartSpeedEvolution;
                    particules2.startSpeed = particules2.startSpeed - pStartSpeedEvolution;
                }
                if (particules1.startLifetime > pStartLifeTimeDefault)
                {
                    particules1.startLifetime = particules1.startLifetime - pStartLifeTimeEvolution;
                    particules2.startLifetime = particules2.startLifetime - pStartLifeTimeEvolution;
                }
                if (particules1.startSize > pStartSizeDefault)
                {
                    particules1.startSize = particules1.startSize - pStartSizeEvolution;
                    particules2.startSize = particules2.startSize - pStartSizeEvolution;

                }
                cam.fieldOfView = cam.fieldOfView - FOVEvolution;
            }
            if (cam.fieldOfView <= FOVDefault && reload == true)
            {
                if (temps < delais)
                {
                    temps++;
                }
                else {
                    if (jaugeDeBoost < jaugeDeBoostInit)
                    {
                        jaugeDeBoost++;
                    }
                    else {
                        boostUtilisé = false;
                        reload = false;
                        temps = 0;
                    }
                }
            }
        }
        slider.value = (jaugeDeBoost / jaugeDeBoostInit);
    }
}
