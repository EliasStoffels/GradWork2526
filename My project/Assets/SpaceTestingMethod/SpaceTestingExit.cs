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

    private bool m_IsConnected = false;

    public void Connect()
    {
        m_IsConnected = true;
    }
    public void Close()
    {
        m_ClosedExit.SetActive(true);
    }

    public bool IsClosedOrConnected()
    {
        if (m_IsConnected)
            return true;

        return m_ClosedExit.activeSelf;
    }

    public ExitType GetExitType()
    {
        return m_ExitType;
    }
}
