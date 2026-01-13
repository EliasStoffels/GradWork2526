using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
public struct RunnerResults
{
    public float timeMiliseconds;
    public float maxMem;
    public float succesRate;
    public float standardDeviationFromY0;
    public int levelConnections;
    public int discontinuesLevelConnections;
    public int loops;
}

public struct CSVExport
{
    public int roomCount;
    public RunnerResults minimum;
    public RunnerResults maximum;
    public RunnerResults trimmedAverage;
}

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

    List<RunnerResults> m_Results = new List<RunnerResults>();
    List<CSVExport> m_TriangulationExport = new List<CSVExport>();
    
    public void GatherData()
    {
        StartCoroutine(GatherDataCo());
    }

    public IEnumerator GatherDataCo()
    {
        foreach(var size in m_DungeonSizes)
        {
            m_Generator.SetDungeonBounds(size);
            UnityEngine.Debug.Log($"Set Dungeon size to {size}");

            m_TriangulationExport.Clear();
            foreach(var count in m_RoomCounts)
            {
                m_Generator.SetTargetRoomCount(count);
                UnityEngine.Debug.Log($"Set target room count to {count}");

                m_Results.Clear();

                for (int runIdx = 0; runIdx < m_NrRuns; ++runIdx)
                {
                    RunnerResults results = new RunnerResults();

                    m_Generator.SetSeed(runIdx);
                    m_Generator.RemovePreviousDungeon();
                    yield return null;

                    Stopwatch sw = Stopwatch.StartNew();
                    m_Generator.Generate();
                    sw.Stop();
                    results.timeMiliseconds = (float)sw.ElapsedMilliseconds;

                    results.succesRate = m_Generator.GetSuccesRate();

                    List<Edge> connections = m_Generator.GetRoomConnections();

                    results.loops = connections.Count - m_Generator.GetFinalRoomCount() + 1;

                    foreach(Edge edge in connections)
                    {
                        float diff = Mathf.Abs(edge.v1.y - edge.v2.y);
                        if ( diff >= 1)
                        {
                            ++results.levelConnections;
                        }
                        if (diff >= 2)
                        {
                            ++results.discontinuesLevelConnections;
                        }
                    }

                    var rooms = m_Generator.GetRooms();

                    float sumSq = 0f;
                    for(int idx = 0; idx < m_Generator.GetFinalRoomCount();++idx)
                    {
                        Room room = rooms[idx];
                        float dy = room.gridPosition.y;
                        sumSq += dy * dy;
                    }

                    results.standardDeviationFromY0 = Mathf.Sqrt(sumSq / m_Generator.GetFinalRoomCount());

                    m_Generator.RemovePreviousDungeon();
                    yield return null;

                    results.maxMem = m_Generator.GenerateMemoryProfile();

                    m_Results.Add(results);
                }

                CSVExport export = new CSVExport();
                export.roomCount = count;
                float trim = 0.1f;

                export.trimmedAverage.timeMiliseconds =
                    DataWriteHelper.TrimmedAverage(m_Results.Select(r => r.timeMiliseconds), trim);
                export.trimmedAverage.maxMem =
                    DataWriteHelper.TrimmedAverage(m_Results.Select(r => r.maxMem), trim);
                export.trimmedAverage.succesRate =
                    DataWriteHelper.TrimmedAverage(m_Results.Select(r => r.succesRate), trim);
                export.trimmedAverage.standardDeviationFromY0 =
                    DataWriteHelper.TrimmedAverage(m_Results.Select(r => r.standardDeviationFromY0), trim);
                export.trimmedAverage.levelConnections =
                    Mathf.RoundToInt(DataWriteHelper.TrimmedAverage(m_Results.Select(r => (float)r.levelConnections), trim));
                export.trimmedAverage.discontinuesLevelConnections =
                    Mathf.RoundToInt(DataWriteHelper.TrimmedAverage(m_Results.Select(r => (float)r.discontinuesLevelConnections), trim));
                export.trimmedAverage.loops =
                    Mathf.RoundToInt(DataWriteHelper.TrimmedAverage(m_Results.Select(r => (float)r.loops), trim));

                export.minimum.timeMiliseconds = m_Results.Min(r => r.timeMiliseconds);
                export.minimum.maxMem = m_Results.Min(r => r.maxMem);
                export.minimum.succesRate = m_Results.Min(r => r.succesRate);
                export.minimum.standardDeviationFromY0 = m_Results.Min(r => r.standardDeviationFromY0);
                export.minimum.levelConnections = m_Results.Min(r => r.levelConnections);
                export.minimum.discontinuesLevelConnections = m_Results.Min(r => r.discontinuesLevelConnections);
                export.minimum.loops = m_Results.Min(r => r.loops);

                export.maximum.timeMiliseconds = m_Results.Max(r => r.timeMiliseconds);
                export.maximum.maxMem = m_Results.Max(r => r.maxMem);
                export.maximum.succesRate = m_Results.Max(r => r.succesRate);
                export.maximum.standardDeviationFromY0 = m_Results.Max(r => r.standardDeviationFromY0);
                export.maximum.levelConnections = m_Results.Max(r => r.levelConnections);
                export.maximum.discontinuesLevelConnections = m_Results.Max(r => r.discontinuesLevelConnections);
                export.maximum.loops = m_Results.Max(r => r.loops);

                m_TriangulationExport.Add(export);
                yield return null;
            }
            DataWriteHelper.WriteCsv(size, m_TriangulationExport, "Triangulation");
            yield return null;
        }
    }
}

public class DataWriteHelper
{
    public static float TrimmedAverage(IEnumerable<float> values, float trimFraction)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int n = sorted.Count;

        if (n == 0)
            return 0f;

        int trimCount = Mathf.FloorToInt(n * trimFraction);

        if (trimCount * 2 >= n)
            trimCount = 0;

        float sum = 0f;
        int count = 0;

        for (int i = trimCount; i < n - trimCount; i++)
        {
            sum += sorted[i];
            count++;
        }

        return count > 0 ? sum / count : 0f;
    }

    public static void WriteCsv(Vector3Int dungeonSize, List<CSVExport> exportData, string name)
    {
        string fileName = $"{dungeonSize.x}x{dungeonSize.y}x{dungeonSize.z}_{name}.csv";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "RoomCount;" +
            "MinTimeMs;AvgTimeMs;MaxTimeMs;" +
            "MinMemKb;AvgMemKb;MaxMemKb;" +
            "MinSuccess;AvgSuccess;MaxSuccess;" +
            "MinStdYM;AvgStdYM;MaxStdYM;" +
            "MinLevelConn;AvgLevelConn;MaxLevelConn;" +
            "MinDiscontConn;AvgDiscontConn;MaxDiscontConn;" +
            "MinLoops;AvgLoops;MaxLoops"
        );

        foreach (var entry in exportData)
        {
            sb.AppendLine(
                $"{entry.roomCount};" +

                $"{entry.minimum.timeMiliseconds};" +
                $"{entry.trimmedAverage.timeMiliseconds};" +
                $"{entry.maximum.timeMiliseconds};" +

                $"{entry.minimum.maxMem};" +
                $"{entry.trimmedAverage.maxMem};" +
                $"{entry.maximum.maxMem};" +

                $"{entry.minimum.succesRate};" +
                $"{entry.trimmedAverage.succesRate};" +
                $"{entry.maximum.succesRate};" +

                $"{entry.minimum.standardDeviationFromY0};" +
                $"{entry.trimmedAverage.standardDeviationFromY0};" +
                $"{entry.maximum.standardDeviationFromY0};" +

                $"{entry.minimum.levelConnections};" +
                $"{entry.trimmedAverage.levelConnections};" +
                $"{entry.maximum.levelConnections};" +

                $"{entry.minimum.discontinuesLevelConnections};" +
                $"{entry.trimmedAverage.discontinuesLevelConnections};" +
                $"{entry.maximum.discontinuesLevelConnections};" +

                $"{entry.minimum.loops};" +
                $"{entry.trimmedAverage.loops};" +
                $"{entry.maximum.loops}"
            );

        }

        File.WriteAllText(path, sb.ToString());
        UnityEngine.Debug.Log($"CSV written to:\n{path}");
    }
}