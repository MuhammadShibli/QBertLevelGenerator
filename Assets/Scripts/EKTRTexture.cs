using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EKTRTexture : MonoBehaviour
{
    private MeshFilter cubeMesh;
    private Mesh mesh;

    private void Start()
    {
        cubeMesh = GetComponent<MeshFilter>();
        mesh = cubeMesh.mesh;
        Vector2[] uvMap = mesh.uv;
        
        //front
        uvMap[0] = new Vector2(0, 0.5f);
        uvMap[1] = new Vector2(0.333f, 0.5f);
        uvMap[2] = new Vector2(0, 1);
        uvMap[3] = new Vector2(0.333f, 1);
        
        //top
        uvMap[4] = new Vector2(0.334f, 0.5f);
        uvMap[5] = new Vector2(0.666f, 0.5f);
        uvMap[8] = new Vector2(0.333f, 1f);
        uvMap[9] = new Vector2(0.666f, 1f);
        //back
        uvMap[6] = new Vector2(0, 0);
        uvMap[7] = new Vector2(0.333f, 0f);
        uvMap[10] = new Vector2(0, 0.5f);
        uvMap[11] = new Vector2(0.333f, 0.5f);

        //bottom
        uvMap[12] = new Vector2(0.667f, 0.5f);
        uvMap[13] = new Vector2(1f, 0.5f);
        uvMap[14] = new Vector2(0.667f, 1f);
        uvMap[15] = new Vector2(1f, 1f);
        //left
        uvMap[16] = new Vector2(0.334f, 0);
        uvMap[17] = new Vector2(0.666f, 0f);
        uvMap[18] = new Vector2(0.334f, 0.5f);
        uvMap[19] = new Vector2(0.666f, 0.5f);

        //right
        uvMap[20] = new Vector2(0.667f, 0);
        uvMap[21] = new Vector2(1f, 0f);
        uvMap[22] = new Vector2(0.667f, 0.5f);
        uvMap[23] = new Vector2(1f, 0.5f);

        mesh.uv = uvMap;
    }
}
