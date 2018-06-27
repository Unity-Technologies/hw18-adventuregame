using UnityEngine.AI;

namespace UnityEngine.AdventureGame
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class Character : MonoBehaviour
    {
        public string characterName;

        NavMeshAgent m_NavMeshAgent;
        Animator     m_Animator;
        void Awake()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_NavMeshAgent.updateRotation = false;

            m_Animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (m_NavMeshAgent.remainingDistance == 0.0f)
            {
                m_Animator.SetBool("WalkLeft", false);
                m_Animator.SetBool("WalkRight", false);
                m_Animator.SetBool("WalkForward", false);
            }
            else
            {
                Vector2 normalized = m_NavMeshAgent.velocity;
                normalized.Normalize();

                // dot the normalized velocity with the right vector to determine whether it is moving
                // left or right or forward/backwards dot(u,v) = |u||v|cos(theta)
                float cosTheta = Mathf.Cos(Mathf.PI / 3.0f);
                float dot = Vector2.Dot(normalized, Vector2.right);
                m_Animator.SetBool("WalkLeft", dot <= -cosTheta);
                m_Animator.SetBool("WalkRight", dot >= cosTheta);
                m_Animator.SetBool("WalkForward", dot < cosTheta && dot > -cosTheta);
            }
        }

        public void WalkToPosition(Vector2 position)
        {
            m_NavMeshAgent.SetDestination(new Vector3(position.x, position.y, 0.0f));
        }

        public void WarpToPosition(Vector2 position)
        {
            m_NavMeshAgent.Warp(new Vector3(position.x, position.y, 0.0f));
        }
    }
}
