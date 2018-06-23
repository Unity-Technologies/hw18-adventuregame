using UnityEngine.AI;

namespace UnityEngine.AdventureGame
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Character : MonoBehaviour
    {
        NavMeshAgent m_NavMeshAgent;
        void Start()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void WalkToPosition(Vector2 position)
        {
            m_NavMeshAgent.SetDestination(new Vector3(position.x, position.y, 0.0f));
        }
    }
}
