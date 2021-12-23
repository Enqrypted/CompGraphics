using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Track : MonoBehaviour
{

    [SerializeField]
    private int submeshCount = 5;

    [SerializeField]
    private float radius = 50f;

    [SerializeField]
    private float roadMarkerWidth = 0.2f;

    [SerializeField]
    private float roadWidth = 5.0f;

    [SerializeField]
    private float barrierWidth = 0.6f;


    [SerializeField]
    private int quadCount = 300;


    //Variance Fields required for the Perlin Noise function
    [SerializeField]
    private float variance = 5.0f;

    [SerializeField]
    private float varianceScale = 0.1f;

    [SerializeField]
    private Vector2 varianceOffset;

    [SerializeField]
    private Vector2 varianceStep = new Vector2(0.01f, 0.01f);
    //

    private MeshGenerator meshGenerator;

    // Start is called before the first frame update
    void Start()
    {    
        
    }

    // Update is called once per frame
    void Update()
    {
        varianceOffset.y += Time.deltaTime;
        RenderTrack();
    }

    private void RenderTrack(){
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        MeshCollider meshCollider = this.GetComponent<MeshCollider>();

        meshFilter.mesh = GenerateTrack();
        meshRenderer.materials = MaterialsList().ToArray();
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    private Mesh GenerateTrack(){
        meshGenerator = new MeshGenerator(submeshCount);

        // generate track code
        float quadDistance = 360f / quadCount;

        List<Vector3> pointRefList = new List<Vector3>();

        for(float degrees = 0; degrees < 360f; degrees+=quadDistance){

            Vector3 point = Quaternion.AngleAxis(degrees, Vector3.up) * Vector3.forward * radius;
            pointRefList.Add(point);
        }

        //Add Noise to our points to create randomness/curves to our track
        Vector2 curve = varianceOffset;

        varianceStep = new Vector2(10f/ pointRefList.Count, 10f/ pointRefList.Count);

        for (int i = 0; i < pointRefList.Count; i++){
            curve += varianceStep;

            Vector3 pointRef = pointRefList[i].normalized;

            float perlinNoise = Mathf.PerlinNoise(curve.x * varianceScale, curve.y * varianceScale);
            perlinNoise *= variance;

            float fix = Mathf.PingPong(i, pointRefList.Count / 2f) / (pointRefList.Count / 2f);

            pointRefList[i] += pointRef * perlinNoise;
        }


        for(int i = 1; i <= pointRefList.Count; i++){
            Vector3 prevQuad = pointRefList[i - 1];
            Vector3 currQuad = pointRefList[i % pointRefList.Count];
            Vector3 nextQuad = pointRefList[(i + 1) % pointRefList.Count];

            CreateTrack(prevQuad, currQuad, nextQuad, i);
        }

        return meshGenerator.CreateMesh();
    }

    private void CreateTrack(Vector3 prevQuad, Vector3 currQuad, Vector3 nextQuad, int quadIndex){

        //create the road marker
        Vector3 offset = Vector3.zero;
        Vector3 targetOffset = Vector3.forward * roadMarkerWidth;

        int markerSubmeshIndex = 0;
        int barrierSubmeshIndex = 2;

        //every two quads, set the road market o have submesh index 1
        //this will give it a dashed line effect instead of a solid straight white line.
        //every two quads, it will also change the color of the barrier so its red and white instead of just red.
        if (quadIndex % 4 == 0 || (quadIndex - 1) % 4 == 0) {
            markerSubmeshIndex = 1;
            barrierSubmeshIndex = 0;
        }

        CreateQuad(prevQuad, currQuad, nextQuad, markerSubmeshIndex, offset, targetOffset);

        //create the road
        offset += targetOffset;
        targetOffset = Vector3.forward * roadWidth;
        CreateQuad(prevQuad, currQuad, nextQuad, 1, offset, targetOffset);

        //create the barrier
        offset += targetOffset;
        targetOffset = Vector3.forward * barrierWidth;
        CreateQuad(prevQuad, currQuad, nextQuad, barrierSubmeshIndex, offset, targetOffset);

        //create the grass
        offset += targetOffset;
        targetOffset = Vector3.forward * (barrierWidth* 3); //grass layer will be 3 times as thick as the barrier
        CreateQuad(prevQuad, currQuad, nextQuad, 3, offset, targetOffset);

        //create the grass layer 2
        offset += targetOffset;
        targetOffset = Vector3.forward * (barrierWidth * 3);
        CreateQuad(prevQuad, currQuad, nextQuad, 4, offset, targetOffset);

        //create the grass layer 3
        offset += targetOffset;
        targetOffset = Vector3.forward * (barrierWidth * 3);
        CreateQuad(prevQuad, currQuad, nextQuad, 3, offset, targetOffset);

        //create the grass layer 4
        offset += targetOffset;
        targetOffset = Vector3.forward * (barrierWidth * 3);
        CreateQuad(prevQuad, currQuad, nextQuad, 4, offset, targetOffset);

    }

    private void CreateQuad(Vector3 prevQuad, Vector3 currQuad, Vector3 nextQuad, 
                            int submesh, Vector3 offset, Vector3 targetOffset){
        //right Side
        Vector3 nextDirection = (nextQuad - currQuad).normalized;
        Vector3 prevDirection = (currQuad - prevQuad).normalized;

        Quaternion nextQuaternion = Quaternion.LookRotation(Vector3.Cross(nextDirection, Vector3.up));
        Quaternion prevQuaternion = Quaternion.LookRotation(Vector3.Cross(prevDirection, Vector3.up));

        Vector3 topLeftPoint = currQuad + (prevQuaternion * offset);
        Vector3 topRightPoint = currQuad + (prevQuaternion * (offset + targetOffset));

        Vector3 bottomLeftPoint = nextQuad + (nextQuaternion * offset);
        Vector3 bottomRightPoint = nextQuad + (nextQuaternion * (offset + targetOffset));

        meshGenerator.CreateTriangle(topLeftPoint, topRightPoint, bottomLeftPoint, submesh);
        meshGenerator.CreateTriangle(topRightPoint, bottomRightPoint, bottomLeftPoint, submesh);

        //left Side
        nextQuaternion = Quaternion.LookRotation(Vector3.Cross(-nextDirection, Vector3.up));
        prevQuaternion = Quaternion.LookRotation(Vector3.Cross(-prevDirection, Vector3.up));

        topLeftPoint = currQuad + (prevQuaternion * offset);
        topRightPoint = currQuad + (prevQuaternion * (offset + targetOffset));

        bottomLeftPoint = nextQuad + (nextQuaternion * offset);
        bottomRightPoint = nextQuad + (nextQuaternion * (offset + targetOffset));

        meshGenerator.CreateTriangle(bottomLeftPoint, bottomRightPoint, topLeftPoint, submesh);
        meshGenerator.CreateTriangle(bottomRightPoint, topRightPoint, topLeftPoint, submesh);

    }

    private List<Material> MaterialsList(){
        List<Material> materialsList = new List<Material>();

        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = Color.white;

        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = Color.black;
        blackMat.SetFloat("_Glossiness", 0f);

        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;

        Material darkGreen = new Material(Shader.Find("Standard"));
        darkGreen.color = new Color(39/255f, 169/255f, 98/255f);
        darkGreen.SetFloat("_Glossiness", 0f);

        Material lightGreen = new Material(Shader.Find("Standard"));
        lightGreen.color = new Color(58/255f, 200/255f, 125/255f);
        lightGreen.SetFloat("_Glossiness", 0f);

        materialsList.Add(whiteMat);
        materialsList.Add(blackMat);
        materialsList.Add(redMat);
        materialsList.Add(darkGreen);
        materialsList.Add(lightGreen);

        return materialsList;

    }
}
