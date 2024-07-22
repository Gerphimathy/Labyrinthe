using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class GameHandler : MonoBehaviour
{
    [Header("AI navigation")]
    [SerializeField]
    private NavMeshSurface navMeshSurface;
    
    [Header ("Labyrinth generation")]
    public LabyrinthGenerator generator;
    public BlenderCLIHandler blenderCLIHandler;
    public GameObject playerPrefab;
    public Vector3 playerStart = new Vector3(0, 0.5f, 0);
    public int toGenerate = 2;
    public float creaturesRoamingThreshold = 20.0f;
    public float creaturesRoamingRange = 500.0f;
    public float creatureSpreadRange = 5000.0f;
    public int creatureCirclerMaxPathLength = 10;
    public int creatureStalkerRange = 20;

    private GameObject _player;    
    private GameObject _goal;
    private GameObject _creatures;
    
    private void AddTextureToChildren(Transform t, Texture2D tex)
    {
        Renderer renderer = t.GetComponent<Renderer>();
        if (renderer != null) 
        {
            renderer.material.mainTexture = tex;
        }

        foreach (Transform child in t) 
        {
            AddTextureToChildren(child, tex);
        }
    }

    
    // Start is called before the first frame update&
    void Start()
    {
        generator.Generate();
        int seed = generator.seed;
        navMeshSurface.BuildNavMesh();

        _creatures = blenderCLIHandler.generate(seed, toGenerate);
        for (int i = 0; i < _creatures.transform.childCount; i++)
        {
            GameObject obj = _creatures.transform.GetChild(i).gameObject;
            //Get random point on navmesh
            obj.transform.position = RandomNavmeshLocation(creatureSpreadRange);
            obj.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);
            
            //Add AI behavior script
            AIBehavior aiBehavior = obj.AddComponent<AIBehavior>();
            aiBehavior.Generate(this);
        }
        
        _goal = generator.GetWinArea();
        
        _player = Instantiate(playerPrefab, playerStart, Quaternion.identity);
        
        var LabNodes = generator.GetLabyrinthNodes();

        foreach (var node in LabNodes)
        {
            Texture2D roomTexture = blenderCLIHandler.GetRandomRoomTexture();
            AddTextureToChildren(node.transform, roomTexture);
        }
    }
    
        
    public Vector3 GetPlayerPosition()
    {
        return _player.transform.position;
    }

    
    public Vector3 RandomNavmeshLocation(float radius) 
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) 
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
