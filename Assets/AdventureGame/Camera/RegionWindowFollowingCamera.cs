using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public class RegionWindowFollowingCamera : FollowingCamera
    {
        public Vector2 m_RegionDimension;

        Vector3 m_TargetPosition;

        public Rect GetRegionWindow()
        {
            Camera camera = GetComponent<Camera>();
            Vector3 origin = camera.ViewportToScreenPoint(camera.rect.center - (m_RegionDimension * 0.5f));
            Vector3 extent = camera.ViewportToScreenPoint(m_RegionDimension);
            return new Rect(origin, extent);
        }

        override protected Vector3 GetStartPosition()
        {
            m_TargetPosition = new Vector3(
                (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
                (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
                transform.position.z
            );

            return m_TargetPosition;
        }

        override protected Vector3 GetTargetPosition()
        {
            Camera camera = GetComponent<Camera>();
            Vector3 targetScreenPosition = camera.WorldToScreenPoint(m_Target.transform.position);
            if (!GetRegionWindow().Contains(targetScreenPosition))
            {
                m_TargetPosition = new Vector3(
                    (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
                    (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
                    transform.position.z
                );
            }

            return m_TargetPosition;
        }
    }
}
