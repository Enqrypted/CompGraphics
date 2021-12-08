using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Maze : MonoBehaviour
{

    public MeshGenerator meshGenerator = new MeshGenerator(1);
    

    public float wallLength = 1f;



    [System.Serializable]
    public class Cell{
        public bool visited;
        public Wall north;
        public Wall east;
        public Wall west;
        public Wall south;
    }

    public class Wall
    {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
        public bool disabled = false;
        public bool horizontal = false;

        public Wall(Vector3 midPoint, bool isHorizontal, float wallLength, float wallThickness) {
            float sideSize = wallLength / 2;

            horizontal = isHorizontal;
            if (isHorizontal)
            {
                topLeft = midPoint + new Vector3(0, sideSize, -(sideSize + (wallThickness/2)));
                topRight = midPoint + new Vector3(0, sideSize, (sideSize + (wallThickness / 2)));
                bottomLeft = midPoint + new Vector3(0, -sideSize, -(sideSize + (wallThickness / 2)));
                bottomRight = midPoint + new Vector3(0, -sideSize, (sideSize + (wallThickness / 2)));
            }
            else {
                topLeft = midPoint + new Vector3(-(sideSize + (wallThickness / 2)), sideSize, 0);
                topRight = midPoint + new Vector3((sideSize + (wallThickness / 2)), sideSize, 0);
                bottomLeft = midPoint + new Vector3(-(sideSize + (wallThickness / 2)), -sideSize, 0);
                bottomRight = midPoint + new Vector3((sideSize + (wallThickness / 2)), -sideSize, 0);
            }
        }


    }


    public int mazeSize;
    private int xSize;
    private int ySize;
    public float wallThickness = .25f;

    private Vector3 initialPos;
    private List<Wall> wallHolder = new List<Wall>();
    public Cell[] cells;
    private int currentCell = 0;
    private int totalCells;
    private int visitedCells;
    private bool startedBuilding = false;
    private int currentNeighbour = 0;
    private List<int> lastCells;
    private int backingUp = 0;
    private int wallToBreak = 0;

    public GameObject startObject;
    public GameObject endObject;
    

    // Start is called before the first frame update
    void Start()
    {
        ySize = mazeSize;
        xSize = mazeSize;

        //create the maze walls in a list and generate the maze using depth-first search algorithm
        CreateWalls();

        //Create the mesh of the walls
        GenerateWallMesh();

        //add collider to maze
        gameObject.AddComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;

        //calculate the origin cell position (the position of the cell at 0,0)
        Vector3 origin = new Vector3(2.5f - (ySize * .5f), 1, 0 - (xSize * .5f));

        //calculate the random start cell position
        Vector3 startPos = origin + new Vector3(wallLength * (Random.Range(1, xSize) - 1), 0, wallLength * (ySize - 1));

        //instantiate the start cube
        GameObject startCube = new GameObject();
        Cube cube = startCube.AddComponent<Cube>();
        cube.color = new Color(.5f, .5f, 1f);
        cube.Generate(); //call the generate method that creates the cube mesh
        startCube.transform.position = startPos;
        startCube.name = "start";

        //calculate the random end cell position
        Vector3 endPos = origin + new Vector3(wallLength * (Random.Range(1, xSize) - 1), 0, 0);

        //instantiate the finish cube
        GameObject endCube = new GameObject();
        cube = endCube.AddComponent<Cube>();
        cube.color = new Color(.5f, 1f, .5f);
        cube.Generate();
        endCube.transform.position = endPos;
        endCube.name = "finish";

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(.8f,1f,1f);
        gameObject.GetComponent<MeshRenderer>().material = mat;

        //create the floor
        GameObject floorPlane = new GameObject();
        cube = floorPlane.AddComponent<Cube>();
        cube.cubeSize = new Vector3(mazeSize*wallLength, .1f, mazeSize*wallLength);
        floorPlane.transform.position = origin + new Vector3((wallLength * (xSize / 2 - 1) + (wallLength/2f)), -1f, (wallLength * (xSize / 2 - 1) + (wallLength / 2f)));
        cube.color = new Color(.5f, .5f, 0.2f);
        cube.Generate();
        floorPlane.AddComponent<BoxCollider>();


        //finally, create the player
        CreatePlayer(startPos);

    }

    void CreatePlayer(Vector3 startPos) {
        GameObject playerCube = new GameObject();
        Cube cube = playerCube.AddComponent<Cube>();
        cube.cubeSize = new Vector3(1, 1.5f, 1);
        cube.color = new Color(1f, 1f, .5f);
        cube.Generate();
        playerCube.transform.position = startPos;
        playerCube.name = "player";

        playerCube.AddComponent<playerController>();
    }

    void GenerateWallMesh() {
        //for each wall that is still enabled, draw the wall by creating the polygons for each side
        foreach (Wall wall in wallHolder)
        {
            if (wall.disabled == false)
            {

                Vector3 thicknessOffset;


                if (wall.horizontal)
                {

                    thicknessOffset = new Vector3(wallThickness / 2f, 0, 0);

                    //wall back
                    meshGenerator.CreateTriangle(wall.topLeft + thicknessOffset, wall.topRight + thicknessOffset, wall.bottomRight + thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.bottomRight + thicknessOffset, wall.bottomLeft + thicknessOffset, wall.topLeft + thicknessOffset, 0);

                    //wall front
                    meshGenerator.CreateTriangle(wall.bottomRight - thicknessOffset, wall.topRight - thicknessOffset, wall.topLeft - thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.topLeft - thicknessOffset, wall.bottomLeft - thicknessOffset, wall.bottomRight - thicknessOffset, 0);

                    //wall top
                    meshGenerator.CreateTriangle(wall.topLeft - thicknessOffset, wall.topRight - thicknessOffset, wall.topRight + thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.topRight + thicknessOffset, wall.topLeft + thicknessOffset, wall.topLeft - thicknessOffset, 0);

                    //wall left
                    meshGenerator.CreateTriangle(wall.topLeft - thicknessOffset, wall.topLeft + thicknessOffset, wall.bottomLeft + thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.bottomLeft + thicknessOffset, wall.bottomLeft - thicknessOffset, wall.topLeft - thicknessOffset, 0);

                    //wall right
                    meshGenerator.CreateTriangle(wall.topRight + thicknessOffset, wall.topRight - thicknessOffset, wall.bottomRight - thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.bottomRight - thicknessOffset, wall.bottomRight + thicknessOffset, wall.topRight + thicknessOffset, 0);
                }
                else
                {

                    thicknessOffset = new Vector3(0, 0, wallThickness / 2f);

                    //wall front
                    meshGenerator.CreateTriangle(wall.topLeft - thicknessOffset, wall.topRight - thicknessOffset, wall.bottomRight - thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.bottomRight - thicknessOffset, wall.bottomLeft - thicknessOffset, wall.topLeft - thicknessOffset, 0);

                    //wall back
                    meshGenerator.CreateTriangle(wall.bottomRight + thicknessOffset, wall.topRight + thicknessOffset, wall.topLeft + thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.topLeft + thicknessOffset, wall.bottomLeft + thicknessOffset, wall.bottomRight + thicknessOffset, 0);

                    //wall top
                    meshGenerator.CreateTriangle(wall.topLeft + thicknessOffset, wall.topRight + thicknessOffset, wall.topRight - thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.topRight - thicknessOffset, wall.topLeft - thicknessOffset, wall.topLeft + thicknessOffset, 0);

                    //wall left
                    meshGenerator.CreateTriangle(wall.topLeft + thicknessOffset, wall.topLeft - thicknessOffset, wall.bottomLeft - thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.bottomLeft - thicknessOffset, wall.bottomLeft + thicknessOffset, wall.topLeft + thicknessOffset, 0);

                    //wall right
                    meshGenerator.CreateTriangle(wall.topRight - thicknessOffset, wall.topRight + thicknessOffset, wall.bottomRight + thicknessOffset, 0);
                    meshGenerator.CreateTriangle(wall.bottomRight + thicknessOffset, wall.bottomRight - thicknessOffset, wall.topRight - thicknessOffset, 0);
                }
            }
        }

        //set the mesh property to the generated wall mesh
        gameObject.GetComponent<MeshFilter>().mesh = meshGenerator.CreateMesh();
    }

    void CreateWalls() {


        initialPos = new Vector3((-xSize / 2) + wallLength / 2, 0, (-ySize / 2) + wallLength / 2);
        Vector3 myPos = initialPos;
        Wall tempWall;

        //horizontal walls
        for (int i = 0; i < ySize; i++) {
            for (int j = 0; j <= xSize; j++) {
                myPos = new Vector3(initialPos.x + (j*wallLength)-wallLength/2, 0, initialPos.z + (i * wallLength) - wallLength/2);
                tempWall = new Wall(myPos, true, wallLength, wallThickness);
                // isHorizontal true
                wallHolder.Add(tempWall);
            }
        }

        //vertical walls
        for (int i = 0; i <= ySize; i++)
        {
            for (int j = 0; j < xSize; j++)
            {
                myPos = new Vector3(initialPos.x + (j * wallLength), 0, initialPos.z + (i * wallLength) - wallLength);
                tempWall = new Wall(myPos, false, wallLength, wallThickness);
                // isHorizontal false
                wallHolder.Add(tempWall);
            }
        }

        CreateCells();
    }

    void CreateCells() {

        lastCells = new List<int>();
        lastCells.Clear();

        totalCells = xSize * ySize;
        List<Wall> allWalls;
        int children = wallHolder.Count;
        allWalls = new List<Wall>();
        cells = new Cell[xSize * ySize];
        int eastWestProcess = 0;
        int childProcess = 0;
        int termCount = 0;


        for (int i = 0; i < children; i++) {
            allWalls.Add(wallHolder[i]);
        }

        for (int cellprocess = 0; cellprocess < cells.Length; cellprocess++) {
            cells[cellprocess] = new Cell();
            cells[cellprocess].east = allWalls[eastWestProcess];
            cells[cellprocess].south = allWalls[childProcess + (xSize+1)*ySize];
            if (termCount == xSize)
            {
                eastWestProcess += 2;
                termCount = 0;
            }
            else {
                eastWestProcess++;
            }

            termCount++;
            childProcess++;
            cells[cellprocess].west = allWalls[eastWestProcess];
            cells[cellprocess].north = allWalls[(childProcess + (xSize + 1) * ySize) + ySize - 1];
        }
        CreateMaze();
    }

    void CreateMaze() {
        if (visitedCells < totalCells)
        {
            if (startedBuilding)
            {
                GetNeighbour();
                if (cells[currentNeighbour].visited == false && cells[currentCell].visited == true)
                {
                    BreakWall();
                    cells[currentNeighbour].visited = true;
                    visitedCells++;
                    lastCells.Add(currentCell);
                    currentCell = currentNeighbour;
                    if (lastCells.Count > 0)
                    {
                        backingUp = lastCells.Count - 1;
                    }
                }
            }
            else
            {
                currentCell = 0;
                cells[currentCell].visited = true;
                visitedCells++;
                startedBuilding = true;
            }

            CreateMaze();
        }
    }

    void BreakWall() {
        switch (wallToBreak) {
            case 1: cells[currentCell].north.disabled = true; break;
            case 2: cells[currentCell].east.disabled = true; break;
            case 3: cells[currentCell].west.disabled = true; break;
            case 4: cells[currentCell].south.disabled = true; break;
        }
    }

    void GetNeighbour() {
        
        int length = 0;
        int[] neighbours = new int[4];
        int[] connectingWall = new int[4];
        int check = 0;

        check = (currentCell + 1) / xSize;
        check -= 1;
        check *= xSize;
        check += xSize;

        //west
        if (currentCell + 1 < totalCells && (currentCell + 1) != check) {
            if (cells[currentCell + 1].visited == false) {
                neighbours[length] = currentCell + 1;
                connectingWall[length] = 3;
                length++;
            }
        }

        //east
        if (currentCell - 1 >= 0 && currentCell != check)
        {
            if (cells[currentCell - 1].visited == false)
            {
                neighbours[length] = currentCell - 1;
                connectingWall[length] = 2;
                length++;
            }
        }

        //north
        if (currentCell + xSize < totalCells)
        {
            if (cells[currentCell + xSize].visited == false)
            {
                neighbours[length] = currentCell + xSize;
                connectingWall[length] = 1;
                length++;
            }
        }

        //south
        if (currentCell - xSize >= 0)
        {
            if (cells[currentCell - xSize].visited == false)
            {
                neighbours[length] = currentCell - xSize;
                connectingWall[length] = 4;
                length++;
            }
        }

        if (length != 0)
        {
            int chosenCell = Random.Range(0, length);
            currentNeighbour = neighbours[chosenCell];
            wallToBreak = connectingWall[chosenCell];
        }
        else {
            if (backingUp > 0) {
                currentCell = lastCells[backingUp];
                backingUp--;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
