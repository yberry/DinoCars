using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer), typeof(MeshCollider))]
public class DinoCollider : MonoBehaviour {

    public uint freqRefreshCol = 2;

    SkinnedMeshRenderer skin;
    Mesh mesh;
    MeshCollider col;

    uint fr = 0;

    void Start()
    {
        skin = GetComponent<SkinnedMeshRenderer>();
        mesh = new Mesh();
        col = GetComponent<MeshCollider>();
    }

    void LateUpdate()
    {
        if (fr % freqRefreshCol == 0)
        {
            skin.BakeMesh(mesh);
            col.sharedMesh = null;
            col.sharedMesh = mesh;
        }
        fr++;
    }
    
}