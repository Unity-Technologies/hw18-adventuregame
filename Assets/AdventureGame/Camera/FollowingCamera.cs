using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public class FollowingCamera : MonoBehaviour
    {
        public Transform m_Target;

        public float m_Speed;

        public bool m_LockAxisX;

        public bool m_LockAxisY;

        protected virtual Vector3 GetStartPosition()
        {
            return new Vector3(
                (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
                (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
                transform.position.z
            );
        }

        protected virtual Vector3 GetTargetPosition()
        {
            return new Vector3(
                (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
                (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
                transform.position.z
            );
        }

        public void WarpToTargetPosition() {
            if (m_Target)
                transform.position = GetTargetPosition();
            return;
        }

        void Start()
        {
            if (m_Target)
            {
                transform.position = GetStartPosition();
            }
        }

        void LateUpdate()
        {
            if (!m_Target || (m_LockAxisX && m_LockAxisY))
            {
                return;
            }

            Vector3 position = GetTargetPosition();

            float elapsed = Time.deltaTime;
            float delta = m_Speed * elapsed;
            float dx = position.x - transform.position.x;
            float dy = position.y - transform.position.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > delta) 
            {
                dx /= dist;
                dy /= dist;
                position.x = transform.position.x + (dx * delta);
                position.y = transform.position.y + (dy * delta);
            }

            //position.x = Mathf.Lerp(transform.position.x, position.x, m_Speed * elapsed);
            //position.y = Mathf.Lerp(transform.position.y, position.y, m_Speed * elapsed);

            transform.position = position;
        }
    }
}