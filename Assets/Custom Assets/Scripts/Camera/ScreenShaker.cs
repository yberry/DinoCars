using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScreenShaker : MonoBehaviour
{

    public float ShakeAmount = 0.1f;
    public float DecreaseFactor = 1.0f;
    public GameObject camRef;

    private new Camera camera;
    private Vector3 cameraPos;
    private Quaternion cameraRot;
    private float shake = 0.0f;

    // Use this for initialization
    void Start()
    {
        this.camera = (Camera)this.GetComponent<Camera>();
        if (this.camera == null)
        {
            // Print an error.
            Debug.Log("CameraShake: Unable to find 'Camera' component attached to GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.shake > 0.0f)
        {
            // Shake the camera.
            this.cameraPos = camRef.transform.localPosition;
            this.cameraRot = camRef.transform.localRotation;
            this.camera.transform.localPosition = Random.insideUnitSphere * this.ShakeAmount * this.shake + this.camRef.transform.localPosition;
            this.shake -= Time.deltaTime * this.DecreaseFactor;
            if (this.shake <= 0.0f)
            {
                // Clamp the shake amount back to zero, and reset the camera position to our cached value.
                this.shake = 0.0f;
                this.camera.transform.localPosition = this.cameraPos;
                this.camera.transform.localRotation = this.cameraRot;
            }
        }
    }
    public void Shake(float amount, bool acceleration)
    {
        // Check if we're already shaking.
        if (this.shake <= 0.0f)
        {
            // If we aren't, cache the camera position.
            // Guess camera evolution 
            this.cameraPos = camRef.transform.localPosition;
            this.cameraRot = camRef.transform.localRotation;
        }

        // Set the 'shake' value.
        this.shake = amount;

        if (acceleration)
        {
            this.DecreaseFactor = -this.DecreaseFactor;
        }
        else
        {
            this.DecreaseFactor = System.Math.Abs(DecreaseFactor);
        }
    }
}