using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.Serialization;

public class GameHandler : MonoBehaviour
{
    [Header("AI navigation")]
    [SerializeField]
    private NavMeshSurface navMeshSurface;
    
    [Header ("Labyrinth generation")]
    public LabyrinthGenerator generator;
    public GameObject playerPrefab;
    public Vector3 playerStart = new Vector3(0, 0.5f, 0);

    private GameObject _player;    
    private GameObject _goal;
    
    // Start is called before the first frame update
    void Start()
    {
        generator.Generate();
        
        navMeshSurface.BuildNavMesh();
        
        _goal = generator.GetWinArea();
        
        _player = Instantiate(playerPrefab, playerStart, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
