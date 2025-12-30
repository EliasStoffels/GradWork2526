using UnityEngine;

public enum ExitType
{
    Room, Corridor, All
}

public class SpaceTestingExit : MonoBehaviour
{
    [SerializeField]
    private GameObject m_ClosedExit;
    [SerializeField]
    private ExitType m_ExitType;

    private SpaceTestingExit m_ConnectedExit;

    public void Connect(SpaceTestingExit connectedExit)
    {
        m_ConnectedExit = connectedExit;
    }
    public void Close()
    {
        m_ClosedExit.SetActive(true);
    }

    public bool IsClosedOrConnected()
    {
        if (m_ConnectedExit != null)
            return true;

        return m_ClosedExit.activeSelf;
    }

    public ExitType GetExitType()
    {
        return m_ExitType;
    }
}
