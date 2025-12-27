using UnityEngine;

public class SpaceTestingGenerator : MonoBehaviour
{
    [SerializeField]
    private int m_Seed = 0;
    [SerializeField]
    private Vector3Int m_DungeonBounds = Vector3Int.zero;
    [SerializeField]
    private int m_TargetRoomCount = 20;
    [SerializeField]
    private int m_RoomAttempts = 10;
    [SerializeField]
    private GameObject[] m_RoomPrefabs;
    [SerializeField]
    private GameObject[] m_CorridorPrefabs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate();
    }

    public void Generate()
    {

    }
}
