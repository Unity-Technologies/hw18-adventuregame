using UnityEngine;

public class OffsetFollowingCamera : FollowingCamera
{
    public Vector2 m_Offset;

    Vector2 m_StartPosition;

    override public Vector3 GetStartPosition()
    {
        m_StartPosition = transform.position;

        return new Vector3(
            (!m_LockAxisX ? m_Target.transform.position.x : m_StartPosition.x) + m_Offset.x,
            (!m_LockAxisY ? m_Target.transform.position.y : m_StartPosition.y) + m_Offset.y,
            transform.position.z
        );
    }
 
    override public Vector3 GetUpdatedPosition()
    {
        return new Vector3(
            (!m_LockAxisX ? m_Target.transform.position.x : m_StartPosition.x) + m_Offset.x,
            (!m_LockAxisY ? m_Target.transform.position.y : m_StartPosition.y) + m_Offset.y,
            transform.position.z
        );
    }
}
