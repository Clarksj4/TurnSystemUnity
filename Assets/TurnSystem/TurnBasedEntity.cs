using UnityEngine;
using TurnBased;
using UnityEngine.Events;

[AddComponentMenu("Turn Based/Turn Based Entity")]
public class TurnBasedEntity : MonoBehaviour, ITurnBased<float>
{
    [Tooltip("Determines the order in which entities activate. Entities with a higher priority will act first")]
    [SerializeField]
    private float priority = 0;

    [Header("Events")]
    [Tooltip("Occurs when this actor's turn has started.")]
    public UnityEvent<TurnBasedEntity> TurnStarted;
    [Tooltip("Occurs when this actor's turn has ended.")]
    public UnityEvent<TurnBasedEntity> TurnEnded;

    private TurnSystem TurnSystem
    {
        get
        {
            if (turnSystem == null)
                turnSystem = GetComponentInParent<TurnSystem>();
            return turnSystem;
        }
    }
    private TurnSystem turnSystem = null;

    /// <summary>
    /// Gets this actor's priority in the turn order.
    /// Higher priorities act first.
    /// </summary>
    public float Priority
    {
        get { return priority; }
        set
        {
            // Check if a change is being made
            bool changed = priority != value;

            priority = value;

            // Update position in order
            if (changed)
                TurnSystem?.UpdatePriority(this);
        }
    }

    private void OnEnable()
    {
        // Get ref to parent system, insert into order
        turnSystem = GetComponentInParent<TurnSystem>();
        if (turnSystem != null)
            turnSystem.Insert(this);
    }

    private void OnDisable()
    {
        // Get ref to parent system, insert into order
        if (turnSystem != null)
            turnSystem.Remove(this);
    }

    /// <summary>
    /// The entities turn has begun.
    /// </summary>
    public void OnTurnStart()
    {
        TurnStarted?.Invoke(this);
    }

    /// <summary>
    /// The entities turn has ended.
    /// </summary>
    public void OnTurnEnd()
    {
        TurnEnded?.Invoke(this);
    }
}
