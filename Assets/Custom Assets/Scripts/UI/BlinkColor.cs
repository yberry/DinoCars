using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlinkColor : MonoBehaviour {

    public float speed = 1f;
    public MovementType type;
    [Range(0f, 1f)]
    public float alphaMin = 0f;
    [Range(0f, 1f)]
    public float alphaMax = 1f;

    Image image;
    Color color;
    float time = 0f;

    void Start()
    {
        image = GetComponent<Image>();
        color = image.color;
    }

    void Update()
    {
        time += Time.unscaledDeltaTime;

        float fact = Mathf.Sin(speed * time);
        if (type == MovementType.Linear)
        {
            fact = Mathf.Asin(fact);
        }
        fact = (fact * (alphaMax - alphaMin) + (alphaMax + alphaMin)) * 0.5f;

        color.a = fact;
        image.color = color;
    }
}
