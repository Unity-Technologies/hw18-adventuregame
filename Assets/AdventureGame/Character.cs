using UnityEngine.AI;

namespace UnityEngine.AdventureGame
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class Character : MonoBehaviour
    {
        public string characterName;

        NavMeshAgent m_NavMeshAgent;
        Animator     m_Animator;

        bool m_Controllable = true;
        public bool Controllable
        {
            get { return m_Controllable;}
            set { m_Controllable = value; }
        }

        public Animator Animator
        {
            get { return m_Animator; }
        }

        void Awake()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_NavMeshAgent.updateRotation = false;

            m_Animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (IsAtDestination())
            {
                m_Animator.SetBool("Walk", false);
            }
            else
            {
                m_Animator.SetBool("Walk", true);

                Vector2 normalized = m_NavMeshAgent.velocity;
                normalized.Normalize();

                // dot the normalized velocity with the right vector to determine whether it is moving
                // left or right or forward/backwards dot(u,v) = |u||v|cos(theta)
                float cosTheta = Mathf.Cos(Mathf.PI / 3.0f);
                float dot = Vector2.Dot(normalized, Vector2.right);
                bool movingLeft = dot <= -cosTheta;
                Vector3 scale = transform.localScale;

                if (movingLeft && scale.x > 0.0)
                {
                    scale.x *= -1;
                    transform.localScale = scale;
                }
                else if (!movingLeft && scale.x < 0.0)
                {
                    scale.x *= -1;
                    transform.localScale = scale;
                }
            }
        }

        public void WalkToPosition(Vector2 position)
        {
            m_NavMeshAgent.SetDestination(new Vector3(position.x, position.y, 0.0f));
        }

        public bool IsAtDestination()
        {
            return m_NavMeshAgent.remainingDistance == 0.0f;
        }

        public void WarpToPosition(Vector2 position)
        {
            m_NavMeshAgent.Warp(new Vector3(position.x, position.y, 0.0f));
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var triggerableArea = collision.gameObject.GetComponent<TriggerArea>();
            if (triggerableArea != null)
            {
                triggerableArea.OnTriggered();
            }    
        }
    }
}
