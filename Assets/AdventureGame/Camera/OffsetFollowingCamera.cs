using UnityEngine;

public class OffsetFollowingCamera : FollowingCamera
{
    public Vector2 m_Offset;

    override public Vector3 GetStartPosition()
    {
        return new Vector3(
            m_Target.transform.position.x + m_Offset.x,
            m_Target.transform.position.y + m_Offset.y,
            transform.position.z
        );
    }
 
    override public Vector3 GetUpdatedPosition()
    {
        return new Vector3(
            (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x) + m_Offset.x,
            (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y) + m_Offset.y,
            transform.position.z
        );
    }
}
