using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpaceTestingRunner : MonoBehaviour
{
    [SerializeField]
    private SpaceTestingGenerator m_Generator;
    [SerializeField]
    private int m_NrRuns;
    [SerializeField]
    private int[] m_RoomCounts;
    [SerializeField]
    private Vector3Int[] m_DungeonSizes;

    List<RunnerResults> m_Results = new List<RunnerResults>();
    List<CSVExport> m_SpaceTestingExport = new List<CSVExport>();

    public void GatherData()
    {
        StartCoroutine(GatherDataCo());
    }

    public IEnumerator GatherDataCo()
    {
        foreach (var size in m_DungeonSizes)
        {
            m_Generator.SetDungeonBounds(size);
            UnityEngine.Debug.Log($"Set Dungeon size to {size}");

            m_SpaceTestingExport.Clear();
            foreach (var count in m_RoomCounts)
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

                    var modules = FindObjectsByType<SpaceTestingModule>(FindObjectsSortMode.None);

                    int nrLevelConnections = 0;
                    int nrDiscontinuesLevelConnections = 0;
                    float sumSq = 0f;
                    foreach (var module in modules)
                    {
                        if(module.GetModuleType() == SpaceTestingModule.ModuleType.Room)
                        {
                            float dy = module.transform.position.y;
                            sumSq += dy * dy;
                        }
                        else
                        {
                            if(module.gameObject.name.Contains("CorridorStraightUp"))
                            {
                                ++nrLevelConnections;
                                foreach(var exit in module.GetExits())
                                {
                                    var connectedExit = exit.GetConnectedExit();
                                    if (connectedExit != null)
                                    {
                                        if (connectedExit.GetComponentInParent<SpaceTestingModule>().gameObject.name.Contains("CorridorStraightUp"))
                                        {
                                            ++nrDiscontinuesLevelConnections;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    results.levelConnections = nrLevelConnections;
                    results.discontinuesLevelConnections = nrDiscontinuesLevelConnections;
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

                m_SpaceTestingExport.Add(export);
            }
            DataWriteHelper.WriteCsv(size, m_SpaceTestingExport, "SpaceTesting");
        }
    }
}
