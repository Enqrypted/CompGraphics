using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Cube : MonoBehaviour
{
    // Start is called before the first frame update

    MeshGenerator meshGenerator = new MeshGenerator(1);
    public Vector3 cubeSize = new Vector3(1, 1, 1);
    public Color color;

    public void Generate() {
        gameObject.GetComponent<MeshFilter>().mesh = CreateCube();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    Mesh CreateCube() {
        Vector3 A, B, C, D, E, F, G, H;

        float xExtent = cubeSize.x / 2;
        float yExtent = cubeSize.y / 2;
        float zExtent = cubeSize.z / 2;

        A = new Vector3(-xExtent, yExtent, -zExtent);
        B = new Vector3(xExtent, yExtent, -zExtent);
        C = new Vector3(xExtent, yExtent, zExtent);
        D = new Vector3(-xExtent, yExtent, zExtent);
        E = new Vector3(-xExtent, -yExtent, -zExtent);
        F = new Vector3(xExtent, -yExtent, -zExtent);
        G = new Vector3(xExtent, -yExtent, zExtent);
        H = new Vector3(-xExtent, -yExtent, zExtent);

        meshGenerator.CreateTriangle(A, B, F, 0);
        meshGenerator.CreateTriangle(F, E, A, 0);

        meshGenerator.CreateTriangle(B, C, G, 0);
        meshGenerator.CreateTriangle(G, F, B, 0);

        meshGenerator.CreateTriangle(C, D, H, 0);
        meshGenerator.CreateTriangle(H, G, C, 0);

        meshGenerator.CreateTriangle(D, A, E, 0);
        meshGenerator.CreateTriangle(E, H, D, 0);

        meshGenerator.CreateTriangle(D, C, B, 0);
        meshGenerator.CreateTriangle(B, A, D, 0);

        meshGenerator.CreateTriangle(E, F, G, 0);
        meshGenerator.CreateTriangle(G, H, E, 0);

        return meshGenerator.CreateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
