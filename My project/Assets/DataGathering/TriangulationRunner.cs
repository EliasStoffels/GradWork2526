using UnityEngine;

public class TriangulationRunner : MonoBehaviour
{
    [SerializeField]
    private TriangulationGenerator m_Generator;
    [SerializeField]
    private int m_NrRuns;
    [SerializeField]
    private int[] m_RoomCounts;
    [SerializeField]
    private Vector3Int[] m_DungeonSizes;

    struct TriangulationRunnerResults
    {
        float time;
        float maxMem;
        float succesRate;
        float standardDeviationFromY0;
        int levelConnections;
        int discontinuesLevelConnections;
    }

    public void GatherData()
    {
        foreach(var count in m_RoomCounts)
        {
            m_Generator.SetTargetRoomCount(m_NrRuns);

            foreach(var size in m_DungeonSizes)
            {
                m_Generator.SetDungeonBounds(size);

                for (int runIdx = 0; runIdx < m_NrRuns; runIdx++)
                {
                    m_Generator.SetSeed(runIdx);

                    m_Generator.Generate();

                    m_Generator.GenerateMemoryProfile();
                }
            }
        }
    }
}
