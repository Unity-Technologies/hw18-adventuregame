using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform m_Target;

    public Vector2 m_Speed = new Vector2(1.0f, 1.0f);

    public bool m_LockAxisX;

    public bool m_LockAxisY;

    public virtual Vector3 GetStartPosition()
    {
        return new Vector3(
            (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
            (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
            transform.position.z
        );
    }

    public virtual Vector3 GetUpdatedPosition()
    {
        return new Vector3(
            (!m_LockAxisX ? m_Target.transform.position.x : transform.position.x),
            (!m_LockAxisY ? m_Target.transform.position.y : transform.position.y),
            transform.position.z
        );
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

        Vector3 position = GetUpdatedPosition();

        position.x = Mathf.Lerp(transform.position.x, position.x, m_Speed.x * Time.deltaTime);
        position.y = Mathf.Lerp(transform.position.y, position.y, m_Speed.y * Time.deltaTime);

        transform.position = position;
    }
}
