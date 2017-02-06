﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CustomMegaBarrier : MonoBehaviour {

    [System.Serializable]
    public struct SurfaceLayer
    {
        public MegaShapeLoft surfaceLoft;
        public int surfaceLayer;
    }

    public MegaWalkLoft prefabWalk;

    public MegaShapeLoft surfaceLoft;
    public int surfaceLayer = -1;
    public SurfaceLayer surfaceLayers;
    public int numbers = 0;
    public float min = 0f;
    public float max = 1f;
    public float crossalpha = 0f;
    public float delay = 0f;
    public float offset = 0f;
    public float tangent = 0.01f;
    public Vector3 rotate = Vector3.zero;
    public bool lateupdate = true;
    public float upright = 0f;
    public Vector3 uprot = Vector3.zero;
    public bool initrot = true;

    void Reset()
    {
        foreach (Transform tr in transform)
        {
            DestroyImmediate(tr.gameObject);
        }
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            return;
        }

        while (numbers > transform.childCount)
        {
            MegaWalkLoft walk = Instantiate(prefabWalk);
            walk.transform.SetParent(transform);
        }

        while (numbers < transform.childCount)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        for (int i = 0; i < numbers; i++)
        {
            float alpha = numbers == 1 ? 0.5f : Mathf.Lerp(min, max, i / (numbers - 1f));
            SetProperties(transform.GetChild(i).GetComponent<MegaWalkLoft>(), alpha);
        }
    }

    void SetProperties(MegaWalkLoft walk, float alpha)
    {
        walk.surfaceLoft = surfaceLoft;
        walk.surfaceLayer = surfaceLayer;
        walk.alpha = alpha;
        walk.crossalpha = crossalpha;
        walk.delay = delay;
        walk.offset = offset;
        walk.tangent = tangent;
        walk.rotate = rotate;
        walk.mode = MegaWalkMode.Alpha;
        walk.lateupdate = lateupdate;
        walk.upright = upright;
        walk.uprot = uprot;
    }
}

