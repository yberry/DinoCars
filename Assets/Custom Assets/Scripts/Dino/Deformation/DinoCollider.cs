using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer), typeof(MeshCollider))]
public class DinoCollider : MonoBehaviour {

    public int freqRefreshCol = 2;

    SkinnedMeshRenderer skin;
    Mesh mesh;
    MeshCollider col;

    int fr = 0;

    void Start()
    {
        skin = GetComponent<SkinnedMeshRenderer>();
        mesh = new Mesh();
        col = GetComponent<MeshCollider>();

        Vector3 scaleParent = transform.parent.localScale;
        transform.localScale = new Vector3(1f / scaleParent.x, 1f / scaleParent.y, 1f / scaleParent.z);
    }

    void LateUpdate()
    {
        if (freqRefreshCol > 0 && fr % freqRefreshCol == 0)
        {
            skin.BakeMesh(mesh);
            col.sharedMesh = mesh;
        }
        fr++;
    }
    
}