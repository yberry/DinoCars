using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testSon : MonoBehaviour {
    public Slider tourMinute;
    public Slider tourAjout;
    private bool embrayage;
    public Toggle startButton;
    private bool start;
    public Button changeVitesse;
    private bool changeV;
    public Text valeurTour;
    public Text valeurAjoutTour;
    public int nbLimiteTour;
    public int nbTourEmbrayage;
    int tourParMinute;
    int valeurMax;
    int additionneur;

	// Use this for initialization
	void Start () {
        tourParMinute = 0;
        additionneur = 50;
        start = false;
        changeV = false;
        nbLimiteTour = 10000;
        nbTourEmbrayage = 3000;
    }
	
	// Update is called once per frame
	void Update () {
        
        if (start)
        {
            if (tourParMinute < nbLimiteTour)
            {

                tourParMinute = tourParMinute + additionneur;
            }

            if (changeV && tourParMinute > nbTourEmbrayage)
            {
                tourParMinute = tourParMinute - nbTourEmbrayage;
                changeV = false;
            }     
        }
        else
        {
            tourParMinute = 0;
        }
        setTourMinute(tourParMinute);
        valeurTour.text = tourParMinute.ToString();
        valeurAjoutTour.text = additionneur.ToString();
    }

    void setTourMinute(int tour)
    {
        tourMinute.value = tour;
    }

    public void onEmbrayage()
    {
        changeV = changeVitesse;
    }

    public void onStart()
    {
        start = startButton.isOn;
    }

    public void onTourAjoutChanged()
    {
        additionneur = (int)tourAjout.value;
    }

}
