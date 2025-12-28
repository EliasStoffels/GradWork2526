using UnityEngine;

public class SpaceTestingModule : MonoBehaviour
{
    [SerializeField]
    private SpaceTestingExit[] m_Exits;
    [SerializeField]
    private Collider m_AproxCollider;

    public SpaceTestingExit[] GetExits()
    {
        return m_Exits;
    }

    public Collider GetAproxCollider()
    {
        return m_AproxCollider;
    }
}
