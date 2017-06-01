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

    /// <summary>
    /// The entities turn has begun
    /// </summary>
    public void TurnStart()
    {
        // [PLACHOLDER]: TODO: Remove
        print(gameObject.name + "'s turn");

        if (TurnStarting != null)
            TurnStarting.Invoke(this);
    }

    /// <summary>
    /// The entities turn has ended
    /// </summary>
    public void TurnEnd()
    {
        // [PLACHOLDER]: TODO: Remove
        print("End of " + gameObject.name + "'s turn");

        if (TurnEnding != null)
            TurnEnding.Invoke(this);
    }

    void Awake()
    {
        // Get ref to parent system, insert into order
        turnSystem = GetComponentInParent<TurnSystem>();
        turnSystem.Insert(this);
    }
}
