using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{

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
        public bool disabled;

        public Wall(Vector3 midPoint, bool isHorizontal, float wallLength) {
            float sideSize = wallLength / 2;
            if (isHorizontal)
            {
                topLeft = midPoint + new Vector3(0, sideSize, -sideSize);
                topRight = midPoint + new Vector3(0, sideSize, sideSize);
                bottomLeft = midPoint + new Vector3(0, -sideSize, -sideSize);
                bottomRight = midPoint + new Vector3(0, -sideSize, sideSize);
            }
            else {
                topLeft = midPoint + new Vector3(-sideSize, sideSize, 0);
                topRight = midPoint + new Vector3(sideSize, sideSize, 0);
                bottomLeft = midPoint + new Vector3(-sideSize, -sideSize, 0);
                bottomRight = midPoint + new Vector3(sideSize, -sideSize, 0);
            }
        }


    }

    

    public int xSize = 5;
    public int ySize = 5;

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

    // Start is called before the first frame update
    void Start()
    {
        CreateWalls();

        foreach (Wall wall in wallHolder) {
            print(wall.disabled + " " + wall.topLeft);
        }

    }

    void CreateWalls() {


        initialPos = new Vector3((-xSize / 2) + wallLength / 2, 0, (-ySize / 2) + wallLength / 2);
        Vector3 myPos = initialPos;
        Wall tempWall;

        //vertical walls
        for (int i = 0; i < ySize; i++) {
            for (int j = 0; j <= xSize; j++) {
                myPos = new Vector3(initialPos.x + (j*wallLength)-wallLength/2, 0, initialPos.z + (i * wallLength) - wallLength/2);
                tempWall = new Wall(myPos, true, wallLength);
                // isHorizontal true
                wallHolder.Add(tempWall);
            }
        }

        //horizontal walls
        for (int i = 0; i <= ySize; i++)
        {
            for (int j = 0; j < xSize; j++)
            {
                myPos = new Vector3(initialPos.x + (j * wallLength), 0, initialPos.z + (i * wallLength) - wallLength);
                tempWall = new Wall(myPos, false, wallLength);
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
        print(children);
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
        if (visitedCells < totalCells) {
            if (startedBuilding) {
                GetNeighbour();
                if (cells[currentNeighbour].visited == false && cells[currentCell].visited == true) {
                    BreakWall();
                    cells[currentNeighbour].visited = true;
                    visitedCells++;
                    lastCells.Add(currentCell);
                    currentCell = currentNeighbour;
                    if (lastCells.Count > 0) {
                        backingUp = lastCells.Count - 1;
                    }
                }
            } else{
                currentCell = Random.Range(0, totalCells);
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
