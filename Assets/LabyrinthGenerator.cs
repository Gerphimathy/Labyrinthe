using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LabyrinthGenerator : MonoBehaviour
{
    //List of prefabs to use for the labyrinth, must be set in the inspector and have a class that inherits from LabyrinthNode
    public GameObject[] labyrinthNodePrefabs;
    public GameObject startNodePrefab;
    
    public int gridWidth = 10;
    public int gridHeight = 10;
    public int gridDepth = 10;
    public Vector3 startNodeLocation;
    public int startNodeGridX = 5;
    public int startNodeGridY = 5;
    public int startNodeGridZ = 0;
    
    public int generationDepth = 3;
    public float gridScale = 5.0f;
    
    private char[][][] _grid;
    private Dictionary<char, GameObject> _nodePrefabs = new Dictionary<char, GameObject>();

    static bool IsLabyrinthNode(GameObject obj)
    {
        if(obj == null) return false;
        return obj.GetComponent<LabyrinthNode>() != null; //Check if the object inherits from LabyrinthNode
    }
    
    LabyrinthNode GetLabyrinthNode(GameObject obj)
    {
        return obj.GetComponent<LabyrinthNode>();
    }
    
    void Start()
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
        startNode.GetComponent<LabyrinthNode>().Init();
        
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
                    _grid[x][y][z] = (char)0;
                }
            }
        }
        
        _grid[startNodeGridX][startNodeGridY][startNodeGridZ] = startNode.GetComponent<LabyrinthNode>().symbol;

        for (int g = 0; g < generationDepth; g++)
        {
            Generation();
        }
    }

    void Generation()
    {
        
    }
    
}
