using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FuckingMovement : MonoBehaviour
{
    public MeshFilter filter;
    public MeshCollider col;

    Mesh mesh;

    void Res()
    {
        mesh = filter.sharedMesh;
        Vector3[] vert = new Vector3[24];
        vert[0] = new Vector3(0.5f, -0.5f, 0.5f);
        vert[1] = new Vector3(-0.5f, -0.5f, 0.5f);
        vert[2] = new Vector3(0.5f, 0.5f, 0.5f);
        vert[3] = new Vector3(-0.5f, 0.5f, 0.5f);
        vert[4] = new Vector3(0.5f, 0.5f, -0.5f);
        vert[5] = new Vector3(-0.5f, 0.5f, -0.5f);
        vert[6] = new Vector3(0.5f, -0.5f, -0.5f);
        vert[7] = new Vector3(-0.5f, -0.5f, -0.5f);
        vert[8] = new Vector3(0.5f, 0.5f, 0.5f);
        vert[9] = new Vector3(-0.5f, 0.5f, 0.5f);
        vert[10] = new Vector3(0.5f, 0.5f, -0.5f);
        vert[11] = new Vector3(-0.5f, 0.5f, -0.5f);
        vert[12] = new Vector3(0.5f, -0.5f, -0.5f);
        vert[13] = new Vector3(0.5f, -0.5f, 0.5f);
        vert[14] = new Vector3(-0.5f, -0.5f, 0.5f);
        vert[15] = new Vector3(-0.5f, -0.5f, -0.5f);
        vert[16] = new Vector3(-0.5f, -0.5f, 0.5f);
        vert[17] = new Vector3(-0.5f, 0.5f, 0.5f);
        vert[18] = new Vector3(-0.5f, 0.5f, -0.5f);
        vert[19] = new Vector3(-0.5f, -0.5f, -0.5f);
        vert[20] = new Vector3(0.5f, -0.5f, -0.5f);
        vert[21] = new Vector3(0.5f, 0.5f, -0.5f);
        vert[22] = new Vector3(0.5f, 0.5f, 0.5f);
        vert[23] = new Vector3(0.5f, -0.5f, 0.5f);
        mesh.vertices = vert;
    }

    void Start()
    {
        Res();
    }

    void Update()
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += Vector3.right * (Time.deltaTime * 0.1f);
        }
        mesh.vertices = vertices;

        col.sharedMesh = mesh;
    }
}