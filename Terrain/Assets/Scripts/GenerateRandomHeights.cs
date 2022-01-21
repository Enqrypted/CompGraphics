using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainTextureData {
    public Texture2D terrainTexture;
    public Vector3 tileSize;

    public float minHeight;

    public float maxHeight;
}

[System.Serializable]
public class TreeData {
    public GameObject treeMesh;

    public float minHeight;
    public float maxHeight;
}

public class GenerateRandomHeights : MonoBehaviour
{

    private Terrain terrain;
    private TerrainData terrainData;

    [SerializeField]
    [Range(0f, 1f)]
    private float minRandomHeightRange = 0f; // min is 0

    [SerializeField]
    [Range(0f, 1f)]
    private float maxRandomHeightRange = .1f; //max is 1

    [SerializeField]
    private bool flattenTerrain = false;

    [SerializeField]
    private bool perlinNoise = true;

    [SerializeField]
    private float perlinNoiseWidthScale = .01f;

    [SerializeField]
    private float perlinNoiseHeightScale = .01f;

    [Header("Texture Data")]
    [SerializeField]
    private List<TerrainTextureData> terrainTextureData;

    [SerializeField]
    private bool addTerrainTexture = false;

    [SerializeField]
    private float terrainTextureBlendOffset = .01f;

    [Header("Tree data")]
    [SerializeField]
    private List<TreeData> treeData;

    [SerializeField]
    private int maxTrees = 2000;

    [SerializeField]
    private int treeSpacing = 10;

    [SerializeField]
    private bool addTrees = false;

    [SerializeField]
    private int terrainLayerIndex;

    [Header("Water")]
    [SerializeField]
    private GameObject water;

    [SerializeField]
    private float waterHeight = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        if (terrain == null) {
            terrain = this.GetComponent<Terrain>();
        }

        if (terrainData == null)
        {
            terrainData = Terrain.activeTerrain.terrainData;
        }

        GenerateHeights();
        AddTerrainTextures();
        AddTrees();
        AddWater();

        terrain.treeDistance = 10000f;

    }

    void GenerateHeights() {

        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        Vector2 midVector = new Vector2(.5f, .5f);

        for (int width = 0; width < terrainData.heightmapResolution; width++) {
            for (int height = 0; height < terrainData.heightmapResolution; height++)
            {
                if (perlinNoise)
                {

                    float distanceFromMid = Vector2.Distance(midVector, new Vector2((float)width/terrainData.heightmapResolution, (float)height / terrainData.heightmapResolution));

                    float totalNoise = Mathf.PerlinNoise(width * perlinNoiseWidthScale, height * perlinNoiseHeightScale)*2.5f;

                    totalNoise += Mathf.PerlinNoise(width * perlinNoiseWidthScale * 3f, height * perlinNoiseHeightScale * 3f)/2f;

                    totalNoise += Mathf.PerlinNoise(width * perlinNoiseWidthScale * 20f, height * perlinNoiseHeightScale * 20f) / 25f;

                    heightMap[width, height] = totalNoise * .6f * (.75f-(distanceFromMid*1.5f));
                }
                else {
                    heightMap[width, height] = Random.Range(minRandomHeightRange, maxRandomHeightRange);
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    void FlattenTerrain()
    {

        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        for (int width = 0; width < terrainData.heightmapResolution; width++)
        {
            for (int height = 0; height < terrainData.heightmapResolution; height++)
            {
                heightMap[width, height] = 0;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    private void AddTerrainTextures() {

        TerrainLayer[] terrainLayers = new TerrainLayer[terrainTextureData.Count];

        for (int i = 0; i < terrainTextureData.Count; i++) {
            if (addTerrainTexture)
            {
                terrainLayers[i] = new TerrainLayer();
                terrainLayers[i].diffuseTexture = terrainTextureData[i].terrainTexture;
                terrainLayers[i].tileSize = terrainTextureData[i].tileSize;
            }
            else {

                terrainLayers[i] = new TerrainLayer();
                terrainLayers[i].diffuseTexture = null;

            }
        }

        terrainData.terrainLayers = terrainLayers;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        float[,,] alphaMapList = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int height = 0; height < terrainData.alphamapHeight; height++) {
            for (int width = 0; width < terrainData.alphamapWidth; width++)
            {
                float[] alphaMap = new float[terrainData.alphamapLayers];

                for (int i = 0; i < terrainTextureData.Count; i++) {

                    float heightBegin = terrainTextureData[i].minHeight - terrainTextureBlendOffset;
                    float heightEnd = terrainTextureData[i].maxHeight - terrainTextureBlendOffset;

                    if (heightMap[width, height] >= heightBegin && heightMap[width, height] <= heightEnd) {
                        alphaMap[i] = 1;
                    }

                }

                Blend(alphaMap);

                for (int j = 0; j < terrainTextureData.Count; j++) {
                    alphaMapList[width, height, j] = alphaMap[j];
                }



            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMapList);

    }

    private void Blend(float[] alphaMap) {
        float total = 0;

        for (int i = 0; i < alphaMap.Length; i++) {
            total += alphaMap[i];
        }

        for (int i = 0; i < alphaMap.Length; i++) {
            alphaMap[i] = alphaMap[i] / total;
        }

    }

    private void AddTrees() {
        TreePrototype[] trees = new TreePrototype[treeData.Count];

        for (int i = 0; i < treeData.Count; i++) {
            trees[i] = new TreePrototype();
            trees[i].prefab = treeData[i].treeMesh;
        }

        terrainData.treePrototypes = trees;

        List<TreeInstance> treeInstanceList = new List<TreeInstance>();

        if (addTrees) {
            for (int z = 0; z < terrainData.heightmapResolution*2; z += treeSpacing) {
                for (int x = 0; x < terrainData.heightmapResolution * 2; x += treeSpacing) {

                    float height = terrainData.GetHeight(x, z) / (terrainData.size.y / 2);

                    if (height > .15f && height < .25f)
                    {
                        TreeInstance treeInstance = new TreeInstance();


                        RaycastHit raycastHit;
                        int layerMask = 1 << terrainLayerIndex;

                        Vector3 treePosition = new Vector3(x / (terrainData.size.x / 1.95f), height / 2, z / (terrainData.size.z / 1.95f));

                        if (Physics.Raycast(treePosition, -Vector3.up, out raycastHit, 1000, layerMask) ||
                            Physics.Raycast(treePosition, Vector3.up, out raycastHit, 1000, layerMask))
                        {

                            float treeDistance = (raycastHit.point.y - transform.position.y);

                            print(raycastHit.transform.name);

                            treeInstance.position = new Vector3(treePosition.x, treePosition.y, treePosition.z);
                            treeInstance.rotation = Random.Range(0, 360);
                            treeInstance.prototypeIndex = 0;
                            treeInstance.color = Color.white;
                            treeInstance.lightmapColor = Color.white;
                            treeInstance.heightScale = .95f;
                            treeInstance.widthScale = .95f;

                            treeInstanceList.Add(treeInstance);
                        }


                        

                    }

                }
            }
        }

        terrainData.treeInstances = treeInstanceList.ToArray();
    }

    private void AddWater() {

        GameObject waterGaneObject = Instantiate(water, transform.position, transform.rotation);
        waterGaneObject.name = "water";
        waterGaneObject.transform.position = transform.position + new Vector3(terrainData.size.x / 2, waterHeight * terrainData.size.y, terrainData.size.z / 2);
        waterGaneObject.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);

    }

    private void OnDestroy()
    {
        FlattenTerrain();
    }

}
