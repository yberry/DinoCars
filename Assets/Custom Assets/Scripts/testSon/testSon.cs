using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testSon : MonoBehaviour {
    public Slider rotationPerMinuteSlider;
    public Slider rotationAdditionSlider;
    private bool gearUpDown;
    public Toggle startButton;
    private bool start;
    private bool gearDown;
    private bool gearUp;
    public Text rotationValueText;
    public Text addRotationTourText;
    public int nbRotationLimit = 12000;
    public int nbRotationClutch = 3000;
    public Slider speedValueSlider;
    public Text speedValueText;
    public Text currentGearText;

    public int rotationPerMinute;
    public int maxValueRotation;
    int addition;
    int currentGear;
    float speedToAdd;

	// Use this for initialization
	void Start () {
        rotationPerMinute = 0;
        addition = 50;

        currentGear = 0;
        speedToAdd = 0;
        

    }

    //Vitesse maximale de 300km/h on a 6 vitesses 
    //On divise par 6 ça fait 50km/h pour chaque vitesse lorsque les tours sont au maximum

    // Update is called once per frame
    void Update () {
        
        if (start)
        {
            if (rotationPerMinute < nbRotationLimit && currentGear > 0)
            {

                rotationPerMinute = rotationPerMinute + addition;
            }
            else if (rotationPerMinute < nbRotationLimit && rotationPerMinute - addition >= 0)
            {
                rotationPerMinute = rotationPerMinute - addition;
            }
            else rotationPerMinute = 0;


            if (gearDown && rotationPerMinute>= nbRotationClutch && currentGear >= 0)
            {
                rotationPerMinute = rotationPerMinute - nbRotationClutch;
                currentGear = currentGear-1;
            }
            else if(gearUp && rotationPerMinute >= nbRotationClutch && currentGear < 6)
            {
                rotationPerMinute = rotationPerMinute - nbRotationClutch;
                currentGear = currentGear + 1;
            }
            else if(gearUp && currentGear==0)
            {
                currentGear = currentGear + 1;
            }
            gearDown = false;
            gearUp = false;
        }
        else
        {
            rotationPerMinute = 0;
        }

        float pourcentage = 1f*rotationPerMinute / nbRotationLimit;
        speedToAdd = 50f *pourcentage * currentGear * 1f;

        setSpeedToSlider(speedToAdd);
        setTourMinute(rotationPerMinute);
        rotationValueText.text = rotationPerMinute.ToString();
        addRotationTourText.text = addition.ToString();
        speedValueText.text = speedToAdd.ToString();
        currentGearText.text = currentGear.ToString();

        //Wwise
        AkSoundEngine.SetRTPCValue("Gear", currentGear);
        AkSoundEngine.SetRTPCValue("RPM", rotationPerMinute);
        AkSoundEngine.SetRTPCValue("Velocity", speedToAdd);
    
    }

    void setTourMinute(int tour)
    {
        rotationPerMinuteSlider.value = tour;
    }

    void setSpeedToSlider(float speed)
    {
       
        speedValueSlider.value = speed;
    }

    public void onGearDown()
    {
        gearDown = true;
    }

    public void onGearUp()
    {
        gearUp = true;
    }

    public void onStart()
    {
        start = startButton.isOn;
        currentGear = 1;
    }

    public void onTourAjoutChanged()
    {
        addition = (int)rotationAdditionSlider.value;
    }


}
