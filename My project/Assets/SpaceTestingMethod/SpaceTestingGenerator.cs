using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEditor.Experimental.GraphView;

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
    private Bounds m_DungeonBoundingBox;

    private GameObject[] m_RoomPrefabsCopy;
    private GameObject[] m_CorridorPrefabsCopy;

    public void Generate()
    {
        Initialize();
        SpawnStartingRoom();
        SpawnOtherModules();
    }

    public void RemovePreviousDungeon()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    public void Regenerate()
    {
        StartCoroutine(RegenerateCo());
    }

    public IEnumerator RegenerateCo()
    {
        RemovePreviousDungeon();
        yield return null;
        Generate();
    }

    private void Initialize()
    {
        m_RoomPrefabsCopy = (GameObject[])m_RoomPrefabs.Clone();
        m_CorridorPrefabsCopy = (GameObject[])m_CorridorPrefabs.Clone();
        m_Random = new System.Random(m_Seed);
        m_Exits.Clear();
        m_FinalRoomCount = 0;
        m_DungeonBoundingBox = new Bounds(transform.position,m_DungeonBounds);
    }

    private void SpawnStartingRoom()
    {
        GameObject startRoom;
        if (m_StartRoom != null)
            startRoom = Instantiate(m_StartRoom, transform);
        else
            startRoom = Instantiate(m_RoomPrefabsCopy[m_Random.Next(m_RoomPrefabsCopy.Length)], transform);

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

        ShuffleArray(m_CorridorPrefabsCopy);
        for (int attemptIdx = 0; attemptIdx < m_CorridorAttempts; ++attemptIdx)
        {
            if (attemptIdx >= m_CorridorPrefabsCopy.Length)
                break;

            prefab = Instantiate(m_CorridorPrefabsCopy[attemptIdx], Vector3.zero, Quaternion.identity);
            newModule = prefab.GetComponent<SpaceTestingModule>();
            connectedExit = newModule.GetExits()[m_Random.Next(newModule.GetExits().Length)];

                                                                                    //180 so exit and connected exit face eachother
            prefab.transform.rotation = exit.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(connectedExit.transform.localRotation);
            Vector3 offset = connectedExit.transform.position - prefab.transform.position;
            prefab.transform.position = exit.transform.position - offset;
            prefab.transform.parent = transform;


            Physics.SyncTransforms();
            if (IsModuleInsideBoundingBox(newModule.GetAproxCollider().bounds))
            {

                Collider[] hits = Physics.OverlapBox(
                                    newModule.GetAproxCollider().bounds.center,
                                    newModule.GetAproxCollider().bounds.extents - m_ColliderLeeway / 2f,
                                    prefab.transform.rotation,
                                    LayerMask.GetMask(LAYER_NAME)
                                    );

                if (!hits.Any(hit => (hit != newModule.GetAproxCollider())))
                {
                    connectedExit.Connect(exit);
                    exit.Connect(connectedExit);
                    break;
                }
            }

            DestroyImmediate(prefab);
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

        ShuffleArray(m_RoomPrefabsCopy);
        for (int attemptIdx = 0; attemptIdx < m_RoomAttempts; ++attemptIdx)
        {
            if (attemptIdx >= m_RoomPrefabsCopy.Length)
                break;

            prefab = Instantiate(m_RoomPrefabsCopy[attemptIdx], Vector3.zero, Quaternion.identity);
            newModule = prefab.GetComponent<SpaceTestingModule>();
            connectedExit = newModule.GetExits()[m_Random.Next(newModule.GetExits().Length)];

                                                                                    //180 so exit and connected exit face eachother
            prefab.transform.rotation = exit.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(connectedExit.transform.localRotation);
            Vector3 offset = connectedExit.transform.position - prefab.transform.position;
            prefab.transform.position = exit.transform.position - offset;
            prefab.transform.parent = transform;

            Physics.SyncTransforms();
            if (IsModuleInsideBoundingBox(newModule.GetAproxCollider().bounds))
            {

                Collider[] hits = Physics.OverlapBox(
                                    newModule.GetAproxCollider().bounds.center,
                                    newModule.GetAproxCollider().bounds.extents - m_ColliderLeeway / 2f,
                                    prefab.transform.rotation,
                                    LayerMask.GetMask(LAYER_NAME)
                                    );

                if (!hits.Any(hit => (hit != newModule.GetAproxCollider())))
                {
                    ++m_FinalRoomCount;
                    connectedExit.Connect(exit);
                    exit.Connect(connectedExit);
                    break;
                }
            }

            DestroyImmediate(prefab);
            prefab = null;
            newModule = null;
            connectedExit = null;
        }
        return newModule;
    }

    private void ShuffleArray<T>(T[] array)
    {
        for(int idx = 0; idx < array.Length; ++idx)
        {
            T temp = array[idx];
            int otherIdx = m_Random.Next(array.Length);
            array[idx] = array[otherIdx];
            array[otherIdx] = temp;
        }
    }

    private bool IsModuleInsideBoundingBox(Bounds moduleBounds)
    {
        return m_DungeonBoundingBox.min.x <= moduleBounds.min.x &&
               m_DungeonBoundingBox.min.y <= moduleBounds.min.y &&
               m_DungeonBoundingBox.min.z <= moduleBounds.min.z &&
               m_DungeonBoundingBox.max.x >= moduleBounds.max.x &&
               m_DungeonBoundingBox.max.y >= moduleBounds.max.y &&
               m_DungeonBoundingBox.max.z >= moduleBounds.max.z;
    }

    private void OnDrawGizmos()
    {
        // bounding box
        Gizmos.color = UnityEngine.Color.white;
        Gizmos.DrawWireCube(transform.position, m_DungeonBounds);
    }
}
