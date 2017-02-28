using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class BlinkColor : MonoBehaviour {

    public float speed = 1f;
    public MovementType type;
    public Color colorMin = Color.white;
    public Color colorMax = Color.white;

    Graphic graphic;
    float time = 0f;

    void Start()
    {
        graphic = GetComponent<Graphic>();
    }

    void Update()
    {
        time += Time.unscaledDeltaTime;

        float fact = Mathf.Sin(speed * time);
        if (type == MovementType.Linear)
        {
            fact = Mathf.Asin(fact);
        }

        fact = (fact + 1f) * 0.5f;

        graphic.color = Color.Lerp(colorMin, colorMax, fact);
    }
}
