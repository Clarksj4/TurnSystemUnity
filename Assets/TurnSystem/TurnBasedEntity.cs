using UnityEngine;
using TurnBased;

[AddComponentMenu("Turn Based/Turn Based Entity")]
public class TurnBasedEntity : MonoBehaviour, ITurnBased<float>
{
    [Tooltip("Determines the order in which entities activate. Entities with a higher priority will act first")]
    [SerializeField]
    private float priority = 0;

    [Header("Events")]
    public TurnEvent TurnStarting;
    public TurnEvent TurnEnding;

    private TurnSystem turnSystem;

    /// <summary>
    /// This entity's priority in the turn system
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
                turnSystem.UpdatePriority(this);
        }
    }

    /// <summary>
    /// The entities turn has begun
    /// </summary>
    public void TurnStart()
    {
        if (TurnStarting != null)
            TurnStarting.Invoke(this);
    }

    /// <summary>
    /// The entities turn has ended
    /// </summary>
    public void TurnEnd()
    {
        if (TurnEnding != null)
            TurnEnding.Invoke(this);
    }

    void Awake()
    {
        // Get ref to parent system, insert into order
        turnSystem = GetComponentInParent<TurnSystem>();
        turnSystem.Insert(this);
    }

    void OnDestroy()
    {
        // Remove this entity from the turn system
        if (turnSystem.Contains(this))
            turnSystem.Remove(this);
    }
}