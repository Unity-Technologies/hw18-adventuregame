using UnityEngine;

public class RegionWindowFollowingCamera : FollowingCamera
{
    public Vector2 m_RegionDimension;

    Vector3 m_TargetPosition;

    public Rect GetRegionWindow()
    {
        return new Rect(GetComponent<Camera>().pixelRect.center - (m_RegionDimension / 2), m_RegionDimension);
    }

    override public Vector3 GetStartPosition()
    {
        Vector3 position = new Vector3(
            m_Target.transform.position.x,
            m_Target.transform.position.y,
            transform.position.z
        );

        m_TargetPosition = position;
        return m_TargetPosition;
    }

    override public Vector3 GetUpdatedPosition()
    {
        if (m_RegionDimension.x > 0 && m_RegionDimension.y > 0)
        {
            Vector3 targetScreenPosition = GetComponent<Camera>().WorldToScreenPoint(m_Target.transform.position);
            if (!GetRegionWindow().Contains(targetScreenPosition))
            {
                Vector3 position = new Vector3(
                    (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
                    (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
                    transform.position.z
                );

                m_TargetPosition = position;
            }
        }

        return m_TargetPosition;
    }
}
