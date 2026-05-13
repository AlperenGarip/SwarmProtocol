using UnityEngine;
using UnityEngine.AI;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// NavMeshAgent wrapper that abstracts navigation.
    /// This allows swapping to custom steering later if NavMesh performance degrades.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyNavigation : MonoBehaviour
    {
        private NavMeshAgent _agent;

        public bool HasReachedDestination =>
            !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;

        public float CurrentSpeed => _agent.velocity.magnitude;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Configures the NavMeshAgent with speed from EnemyDataSO.
        /// </summary>
        public void Initialize(float moveSpeed)
        {
            _agent.speed = moveSpeed;
            _agent.acceleration = moveSpeed * 4f;
            _agent.angularSpeed = 360f;
            _agent.stoppingDistance = 0.5f;
            _agent.enabled = true;
        }

        /// <summary>
        /// Sets the NavMeshAgent destination.
        /// </summary>
        public void MoveTo(Vector3 target)
        {
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.SetDestination(target);
                _agent.isStopped = false;
            }
        }

        /// <summary>
        /// Stops the agent movement.
        /// </summary>
        public void Stop()
        {
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }

        /// <summary>
        /// Warps the agent to a position (useful for pool respawn).
        /// </summary>
        public void WarpTo(Vector3 position)
        {
            _agent.enabled = false;
            transform.position = position;
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.Warp(position);
            }
        }

        /// <summary>
        /// Enables or disables the agent.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _agent.enabled = enabled;
        }

        /// <summary>
        /// Pauses pathfinding without disabling the agent.
        /// Safe for use during knockback or freeze — keeps the agent on the NavMesh.
        /// </summary>
        public void SetStopped(bool stopped)
        {
            if (_agent.enabled && _agent.isOnNavMesh)
                _agent.isStopped = stopped;
        }

        /// <summary>
        /// Moves the agent by delta without changing the destination.
        /// Used by knockback to push the enemy while keeping it on the NavMesh.
        /// </summary>
        public void Move(Vector3 delta)
        {
            if (_agent.enabled && _agent.isOnNavMesh)
                _agent.Move(delta);
        }
    }
}
