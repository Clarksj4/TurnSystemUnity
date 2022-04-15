using System.Collections.Generic;
using TurnBased;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Turn Based/Turn System")]
public class TurnSystem : MonoBehaviour
{
    [SerializeField]
    public TurnEvent TurnEnding;
    [SerializeField]
    public TurnEvent TurnStarting;
    [SerializeField]
    public UnityEvent CycleComplete;
    [SerializeField]
    public TurnEvent OrderChanged;

    public TurnBasedEntity Current { get { return order.Current as TurnBasedEntity; } }
    public IEnumerable<TurnBasedEntity> Order 
    { 
        get 
        {
            // Cast each pawn to TurnBasedEntity
            foreach (ITurnBased<float> pawn in order)
                yield return pawn as TurnBasedEntity;
        } 
    }

    private TurnOrder<float> order = new TurnOrder<float>();

    /// <summary>
    /// End the current entities turn, progress to the next entity, restarting the cycle if it is complete
    /// </summary>
    public void EndTurn()
    {
        // Only do a thing if the order is not empty
        if (order.Count > 0)
        {
            // Notify current object's turn has ended
            if (Current != null)
                TurnEnding.Invoke(Current);

            // Move to next object if there is any
            bool isMore = order.MoveNext();

            // If there are no more, then a complete cycle is complete 
            if (!isMore)
            {
                // Notify a complete turn cycle is finished
                CycleComplete.Invoke();
                order.MoveNext(); // Start the cycle again
            }

            // Notify turn has started
            TurnStarting.Invoke(Current);
        }
    }

    /// <summary>
    /// Insert an entity into the order
    /// </summary>
    public void Insert(TurnBasedEntity entity)
    {
        // Insert wrapper object into order
        order.Insert(entity);

        // Notify that an object has been inserted
        OrderChanged.Invoke(entity);
    }

    /// <summary>
    /// Remove an entity from the order. If the current entity is removed, the turn order progresses to the next entity
    /// </summary>
    public void Remove(TurnBasedEntity entity)
    {
        // Check if the current item is being removed
        bool currentRemoved = entity == Current;

        // Remove wrapper object from order
        order.Remove(entity);

        // Notify that an object has been removed
        OrderChanged.Invoke(entity);

        // If the current item was removed, progress to next ite's turn
        if (currentRemoved && order.Count > 0)
            EndTurn();
    }

    /// <summary>
    /// Update the priority of the given entity; reordering entities
    /// </summary>
    public void UpdatePriority(TurnBasedEntity entity)
    {
        // Update wrapper priority in order
        order.UpdatePriority(entity);

        // Notify that an object has had its priority changed
        OrderChanged.Invoke(entity);
    }

    public bool Contains(TurnBasedEntity entity)
    {
        return order.Contains(entity);
    }
}
