using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CustomMegaBarrier : MonoBehaviour {

    [System.Serializable]
    public struct SurfaceLayer
    {
        public MegaShapeLoft loft;
        public int layer;
    }

    public MegaWalkLoft prefabWalk;

    public SurfaceLayer[] surfaceLayers = new SurfaceLayer[0];
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

        while (numbers * surfaceLayers.Length > transform.childCount)
        {
            Instantiate(prefabWalk, transform);
        }

        while (numbers * surfaceLayers.Length < transform.childCount)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        for (int i = 0, k = 0; i < surfaceLayers.Length; i++)
        {
            for (int j = 0; j < numbers; j++, k++)
            {
                float alpha = numbers == 1 ? 0.5f : Mathf.Lerp(min, max, j / (numbers - 1f));
                SetProperties(transform.GetChild(k).GetComponent<MegaWalkLoft>(), surfaceLayers[i], alpha);
            }
        }
    }

    void SetProperties(MegaWalkLoft walk, SurfaceLayer surfaceLayer, float alpha)
    {
        walk.surfaceLoft = surfaceLayer.loft;
        walk.surfaceLayer = surfaceLayer.layer;
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

