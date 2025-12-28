using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SpaceTestingGenerator : MonoBehaviour
{
    private const string LAYER_NAME = "SpaceTestingModule";
    [SerializeField]
    private int m_Seed = 0;
    [SerializeField]
    private Vector3Int m_DungeonBounds = Vector3Int.zero;
    [SerializeField]
    private int m_TargetRoomCount = 20;
    [SerializeField]
    private int m_RoomAttempts = 10;
    [SerializeField]
    private int m_CorridorAttempts = 10;
    [SerializeField]
    private GameObject m_StartRoom;
    [SerializeField]
    private Vector3 m_ColliderLeeway = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private float m_RoomChance = 0.5f;
    [SerializeField]
    private GameObject[] m_RoomPrefabs;
    [SerializeField]
    private GameObject[] m_CorridorPrefabs;

    private System.Random m_Random;
    private List<SpaceTestingExit> m_Exits = new List<SpaceTestingExit>();
    private int m_FinalRoomCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        StartCoroutine(GenerateCo());
    }

    IEnumerator GenerateCo()
    {
        Initialize();
        yield return null;
        SpawnStartingRoom();
        SpawnOtherModules();
    }

    private void Initialize()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        m_Random = new System.Random(m_Seed);
        m_Exits.Clear();
        m_FinalRoomCount = 0;
        Physics.SyncTransforms();
    }

    private void SpawnStartingRoom()
    {
        GameObject startRoom;
        if (m_StartRoom != null)
            startRoom = Instantiate(m_StartRoom, transform);
        else
            startRoom = Instantiate(m_RoomPrefabs[m_Random.Next(m_RoomPrefabs.Length)], transform);

        ++m_FinalRoomCount;
        m_Exits.AddRange(startRoom.GetComponent<SpaceTestingModule>().GetExits());
    }

    private void SpawnOtherModules()
    {
        while(m_FinalRoomCount < m_TargetRoomCount)
        {
            var openExit = m_Exits.Find((exit) => !exit.IsClosedOrConnected());
            if (openExit != null)
            {
                var newModule = TrySpawnNewModule(openExit);

                if (newModule != null)
                {
                    m_Exits.AddRange(newModule.GetExits());
                    openExit.Connect();
                }
                else
                    openExit.Close();
            }
            else
                return;
        }

        var openExits = m_Exits.FindAll((exit) => !exit.IsClosedOrConnected());
        foreach(var exit in openExits)
        {
            exit.Close();
        }
    }

    private SpaceTestingModule TrySpawnNewModule(SpaceTestingExit exit)
    {
        switch(exit.GetExitType())
        {
            case ExitType.All:
                {
                    if(m_Random.NextDouble() < m_RoomChance)
                        return TrySpawnRoom(exit);
                    else
                        return TrySpawnCorridor(exit);
                }
            case ExitType.Room:
                {
                    return TrySpawnRoom(exit);
                }
            case ExitType.Corridor:
                {
                    return TrySpawnCorridor(exit);
                }
        }

        return null;
    }

    private SpaceTestingModule TrySpawnCorridor(SpaceTestingExit exit)
    {
        GameObject prefab = null;
        SpaceTestingModule newModule = null;
        SpaceTestingExit connectedExit = null;
        for (int attemptIdx = 0; attemptIdx < m_CorridorAttempts; ++attemptIdx)
        {
            prefab = Instantiate(m_CorridorPrefabs[m_Random.Next(m_CorridorPrefabs.Length)], Vector3.zero, Quaternion.identity);
            newModule = prefab.GetComponent<SpaceTestingModule>();
            connectedExit = newModule.GetExits()[m_Random.Next(newModule.GetExits().Length)];

                                                                                    //180 so exit and connected exit face eachother
            prefab.transform.rotation = exit.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(connectedExit.transform.localRotation);
            Vector3 offset = connectedExit.transform.position - prefab.transform.position;
            prefab.transform.position = exit.transform.position - offset;
            prefab.transform.parent = transform;

            Physics.SyncTransforms();

            Collider[] hits = Physics.OverlapBox(
                                newModule.GetAproxCollider().bounds.center,
                                newModule.GetAproxCollider().bounds.extents - m_ColliderLeeway / 2f,
                                prefab.transform.rotation,
                                LayerMask.GetMask(LAYER_NAME)
                                );

            if (!hits.Any(hit => (hit != newModule.GetAproxCollider())))
            {
                connectedExit.Connect();
                break;
            }

            Destroy(prefab);
            prefab = null;
            newModule = null;
            connectedExit = null;
        }
        return newModule;
    }

    private SpaceTestingModule TrySpawnRoom(SpaceTestingExit exit)
    {
        GameObject prefab = null;
        SpaceTestingModule newModule = null;
        SpaceTestingExit connectedExit = null;
        for (int attemptIdx = 0; attemptIdx < m_RoomAttempts; ++attemptIdx)
        {
            prefab = Instantiate(m_RoomPrefabs[m_Random.Next(m_RoomPrefabs.Length)], Vector3.zero, Quaternion.identity);
            newModule = prefab.GetComponent<SpaceTestingModule>();
            connectedExit = newModule.GetExits()[m_Random.Next(newModule.GetExits().Length)];

                                                                                    //180 so exit and connected exit face eachother
            prefab.transform.rotation = exit.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(connectedExit.transform.localRotation);
            Vector3 offset = connectedExit.transform.position - prefab.transform.position;
            prefab.transform.position = exit.transform.position - offset;
            prefab.transform.parent = transform;

            Physics.SyncTransforms();

            Collider[] hits = Physics.OverlapBox(
                                newModule.GetAproxCollider().bounds.center,
                                newModule.GetAproxCollider().bounds.extents - m_ColliderLeeway / 2f,
                                prefab.transform.rotation,
                                LayerMask.GetMask(LAYER_NAME)
                                );

            if (!hits.Any(hit => (hit != newModule.GetAproxCollider())))
            {
                ++m_FinalRoomCount;
                connectedExit.Connect();
                break;
            }

            Destroy(prefab);
            prefab = null;
            newModule = null;
            connectedExit = null;
        }
        return newModule;
    }

    private void OnDrawGizmos()
    {
        // bounding box
        Gizmos.color = UnityEngine.Color.white;
        Gizmos.DrawWireCube(transform.position, m_DungeonBounds);
    }
}
