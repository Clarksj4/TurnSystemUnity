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

    private void OnEnable()
    {
        // Get ref to parent system, insert into order
        turnSystem = GetComponentInParent<TurnSystem>();
        if (turnSystem != null && !turnSystem.Contains(this))
            turnSystem.Insert(this);
    }

    private void OnDisable()
    {
        // Get ref to parent system, insert into order
        if (turnSystem != null)
            turnSystem.Remove(this);
    }

    /// <summary>
    /// The entities turn has begun
    /// </summary>
    public void TurnStart()
    {
        SendMessage("OnTurnStart", SendMessageOptions.DontRequireReceiver);

        TurnStarting?.Invoke(this);
    }

    /// <summary>
    /// The entities turn has ended
    /// </summary>
    public void TurnEnd()
    {
        SendMessage("OnTurnEnd", SendMessageOptions.DontRequireReceiver);

        TurnEnding?.Invoke(this);
    }
}
