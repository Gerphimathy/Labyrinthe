using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LabyrinthGenerator : MonoBehaviour
{
    //List of prefabs to use for the labyrinth, must be set in the inspector and have a class that inherits from LabyrinthNode
    public GameObject[] labyrinthNodePrefabs;
    public GameObject startNodePrefab;
    public GameObject winAreaPrefab;
    
    private GameObject _winArea;

    public int gridWidth = 10;
    public int gridHeight = 10;
    public int gridDepth = 10;
    public Vector3 startNodeLocation;
    public int startNodeGridX = 5;
    public int startNodeGridY = 5;
    public int startNodeGridZ = 0;

    public int generationDepth = 3;
    public float gridScale = 5.0f;

    public char emptySymbol = '.';
    public char growthSymbol = 'G';
    private char[][][] _grid;
    private Dictionary<char, GameObject> _nodePrefabs = new Dictionary<char, GameObject>();
    private Dictionary<Vector3Int, GameObject> _nodeInstances = new Dictionary<Vector3Int, GameObject>();

    static bool IsLabyrinthNode(GameObject obj)
    {
        if (obj == null) return false;
        return obj.GetComponent<LabyrinthNode>() != null; //Check if the object inherits from LabyrinthNode
    }

    LabyrinthNode GetLabyrinthNode(GameObject obj)
    {
        return obj.GetComponent<LabyrinthNode>();
    }

    public void Generate()
    {
        if (!IsLabyrinthNode(startNodePrefab))
        {
            Debug.LogError("StartNode is not a LabyrinthNode");
            return;
        }

        foreach (var prefab in labyrinthNodePrefabs)
        {
            if (!IsLabyrinthNode(prefab))
            {
                Debug.LogError(prefab.name + " is not a LabyrinthNode");
                return;
            }

            //If symbol already exists in the dictionary, throw an error
            if (_nodePrefabs.ContainsKey(GetLabyrinthNode(prefab).symbol))
            {
                Debug.LogError("Symbol " + GetLabyrinthNode(prefab).symbol + " is already in use");
                return;
            }

            _nodePrefabs[GetLabyrinthNode(prefab).symbol] = prefab;
        }

        //Instantiate the start node
        GameObject startNode = Instantiate(startNodePrefab, startNodeLocation, Quaternion.identity);
        startNode.GetComponent<LabyrinthNode>().Init(new Vector3Int(startNodeGridX, startNodeGridY, startNodeGridZ));

        //Instantiate the grid
        _grid = new char[gridWidth][][];
        for (int x = 0; x < gridWidth; x++)
        {
            _grid[x] = new char[gridHeight][];
            for (int y = 0; y < gridHeight; y++)
            {
                _grid[x][y] = new char[gridDepth];
                for (int z = 0; z < gridDepth; z++)
                {
                    _grid[x][y][z] = emptySymbol;
                }
            }
        }

        _grid[startNodeGridX][startNodeGridY][startNodeGridZ] = startNode.GetComponent<LabyrinthNode>().symbol;
        _nodeInstances[new Vector3Int(startNodeGridX, startNodeGridY, startNodeGridZ)] = startNode;
        

        AddGrowthNodes(startNodeGridX, startNodeGridY, startNodeGridZ);
        DEBUG_PrintGrid();


        for (int g = 1; g <= generationDepth; g++)
        {
            Generation(g, generationDepth);
        }
    }
    
    public GameObject GetWinArea()
    {
        return _winArea;
    }

    private bool validCoordinate(int x, int y, int z)
    {
        if (x < 0 || x >= gridWidth) return false;
        if (y < 0 || y >= gridHeight) return false;
        if (z < 0 || z >= gridDepth) return false;
        return true;
    }

    private bool IsNodeFree(int x, int y, int z)
    {
        if (!validCoordinate(x, y, z)) return false;
        return _grid[x][y][z] == emptySymbol || _grid[x][y][z] == growthSymbol;
    }

    private void AddGrowthNodes(int x, int y, int z)
    {
        if (!validCoordinate(x, y, z)) return;
        var prefab = _nodePrefabs[_grid[x][y][z]];
        foreach (var connection in GetLabyrinthNode(prefab).connections)
        {
            if (!validCoordinate(x + connection.x, y + connection.y, z + connection.z)) continue;
            if (IsNodeFree(x + connection.x, y + connection.y, z + connection.z))
            {
                _grid[x + connection.x][y + connection.y][z + connection.z] = growthSymbol;
            }
        }
    }

    private Vector3 calcNodePosition(int x, int y, int z)
    {
        //Calc pos using startNodeLocation as the center
        float xPos = startNodeLocation.x + (x - startNodeGridX) * gridScale;
        float zPos = startNodeLocation.y - (y - startNodeGridY) * gridScale;
        float yPos = startNodeLocation.z + (z - startNodeGridZ) * gridScale;
        
        return new Vector3(xPos, yPos, zPos);
    }

    private void DEBUG_PrintGrid()
    {
        string gridString = "";
        for (int z = 0; z < gridDepth; z++)
        {
            gridString += "Layer " + z + "\n";
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    gridString += _grid[x][y][z] + "\t";
                }

                gridString += "\n";
            }

            gridString += "\n";
        }

        Debug.Log(gridString);
    }

    List<LabyrinthNode> GetNeighbors(int x, int y, int z)
    {
        List<LabyrinthNode> neighbors = new List<LabyrinthNode>();
        for (int x2 = -1; x2 <= 1; x2++)
        {
            for (int y2 = -1; y2 <= 1; y2++)
            {
                for (int z2 = -1; z2 <= 1; z2++)
                {
                    if (x2 == x && y2 == y && z2 == z) continue;
                    if(!validCoordinate(x+x2, y+y2, z+z2)) continue;
                    if (IsNodeFree(x + x2, y + y2, z + z2)) continue;
                    
                    neighbors.Add(GetLabyrinthNode(_nodeInstances[new Vector3Int(x + x2, y + y2, z + z2)]));
                }
            }
        }
        return neighbors;
    }

    void Generation(int nGen, int genmax)
    {
        char[][][] newGrid = new char[gridWidth][][];
        for (int x = 0; x < gridWidth; x++)
        {
            newGrid[x] = new char[gridHeight][];
            for (int y = 0; y < gridHeight; y++)
            {
                newGrid[x][y] = new char[gridDepth];
                for (int z = 0; z < gridDepth; z++)
                {
                    newGrid[x][y][z] = _grid[x][y][z];
                }
            }
        }

        List<GameObject> final_gen_nodes = new List<GameObject>();
        //Go over all nodes in the grid and replace growth nodes with actual nodes
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    if (_grid[x][y][z] != growthSymbol) continue;

                    List<LabyrinthNode> neighbours = GetNeighbors(x, y, z);
                    
                    //Choose a random neighbor to connect to
                    int ran = Random.Range(0, neighbours.Count);
                    LabyrinthNode neighbor = neighbours[ran];
                    
                    List<GameObject> possibleNodes = new List<GameObject>();
                    foreach (var prefab in labyrinthNodePrefabs)
                    {
                        
                        Vector3Int diff = neighbor.GetGridPosition() - new Vector3Int(x, y, z);
                        
                        if (GetLabyrinthNode(prefab).CanConnect(neighbor, diff))
                        {
                            possibleNodes.Add(prefab);
                        }
                    }
                    
                    if (possibleNodes.Count == 0) continue;
                    
                    GameObject nodePrefab = possibleNodes[Random.Range(0, possibleNodes.Count)];
                    GameObject node = Instantiate(nodePrefab, calcNodePosition(x,y,z), Quaternion.identity);
                    node.GetComponent<LabyrinthNode>().Init(new Vector3Int(x, y, z));
                    _nodeInstances[new Vector3Int(x, y, z)] = node;
                    newGrid[x][y][z] = node.GetComponent<LabyrinthNode>().symbol;
                    
                    final_gen_nodes.Add(node);
                }
            }
        }
        
        if(nGen == genmax)
        {
            //Instantiate the win area in the middle of one of the final generation nodes
            _winArea = final_gen_nodes[Random.Range(0, final_gen_nodes.Count)];
            Instantiate(winAreaPrefab, _winArea.transform.position, Quaternion.identity);

            return;
        }
        
        _grid = newGrid;
        
        //Add growth nodes for the new nodes
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    if (_grid[x][y][z] == emptySymbol || _grid[x][y][z] == growthSymbol) continue;
                    AddGrowthNodes(x, y, z);
                }
            }
        }
    }
}