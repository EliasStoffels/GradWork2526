using UnityEngine;

public class SpaceTestingModule : MonoBehaviour
{
    public enum ModuleType
    {
        Room,Corridor
    }

    [SerializeField]
    private SpaceTestingExit[] m_Exits;
    [SerializeField]
    private Collider m_AproxCollider;
    [SerializeField]
    private ModuleType m_ModuleType = ModuleType.Room;

    public SpaceTestingExit[] GetExits()
    {
        return m_Exits;
    }

    public Collider GetAproxCollider()
    {
        return m_AproxCollider;
    }

    public ModuleType GetModuleType()
    {
        return m_ModuleType;
    }
}
