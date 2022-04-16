using System;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Turn Based/Turn System")]
public class TurnSystem : MonoBehaviour
{
    [Tooltip("Should the turn system begin automatically?")]
    public bool AutoStart = false;
    [Tooltip("Should the turn system start a new round automatically when one ends?")]
    public bool AutoLoop = false;

    [Tooltip("Occurs when an actors turn is starting.")]
    public UnityEvent<TurnBasedEntity> TurnStarted;
    [Tooltip("Occurs when an actors turn is ending.")]
    public UnityEvent<TurnBasedEntity> TurnEnded;
    [Tooltip("Occurs when the order of actors changes.")]
    public UnityEvent OrderChanged;
    [Tooltip("Occurs when a new round is starting. " +
        "A round is a full cycle of all actors in the turn order.")]
    public UnityEvent OnRoundStarting;
    [Tooltip("Occurs when a round is ending. " +
        "A round is a full cycle of all actors in the turn order.")]
    public UnityEvent OnRoundEnded;

    /// <summary>
    /// Gets the current actor.
    /// </summary>
    public TurnBasedEntity Current { get { return order.Current as TurnBasedEntity; } }
    /// <summary>
    /// Gets the actors in the order they will act.
    /// </summary>
    public IEnumerable<TurnBasedEntity> Order 
    { 
        get 
        {
            // Cast each pawn to TurnBasedEntity
            foreach (ITurnBased<float> pawn in order)
                yield return pawn as TurnBasedEntity;
        } 
    }
    /// <summary>
    /// Gets the number of actors in the turn system.
    /// </summary>
    public int ActorCount { get { return order.Count; } }
    /// <summary>
    /// The index of the current round.
    /// </summary>
    public int RoundCount { get; private set; }

    private bool turnEndInProgress = false;
    private bool nextTurnRequested = false;
    private bool roundStarted = false;

    private TurnOrder<float> order = new TurnOrder<float>();

    private void Start()
    {
        if (AutoStart)
            RequestNextTurn();
    }

    /// <summary>
    /// Insert an entity into the order
    /// </summary>
    public void Insert(TurnBasedEntity entity)
    {
        // Insert wrapper object into order
        order.Insert(entity);

        // Notify that an object has been inserted
        OrderChanged?.Invoke();
    }

    /// <summary>
    /// Remove an entity from the order. If the current entity is removed, the turn order progresses to the next entity
    /// </summary>
    public void Remove(TurnBasedEntity entity)
    {
        if (entity == null)
            throw new ArgumentException("Can't remove a null entity.");

        // Check if the current actor is being removed
        bool currentBeingRemoved = entity == Current;

        // Remove from order
        bool removed = order.Remove(entity);
        if (removed)
        {
            // Notify that an object has been removed
            OrderChanged?.Invoke();

            // If the current item was removed, progress to next actor's turn
            if (currentBeingRemoved && order.Count > 0)
                RequestNextTurn();
        }
    }

    /// <summary>
    /// Checks if the turn system contains the given actor.
    /// </summary>
    public bool Contains(TurnBasedEntity actor)
    {
        return order.Contains(actor);
    }

    /// <summary>
    /// Update the priority of the given entity; reordering entities
    /// </summary>
    public void UpdatePriority(TurnBasedEntity entity)
    {
        // Update wrapper priority in order
        order.UpdatePriority(entity);

        // Notify that an object has had its priority changed
        OrderChanged?.Invoke();
    }

    /// <summary>
    /// Requests that the current turn ends - will finish
    /// any in progress sequences before starting a new turn.
    /// </summary>
    [ContextMenu("Next Turn")]
    public void RequestNextTurn()
    {
        // Only do a thing if the order is not empty
        if (order.Count > 0)
        {
            // If a turn is already ending - wait for it to finish
            // before ending the next one (so that ALL listeners
            // get notified in order)
            if (turnEndInProgress)
                nextTurnRequested = true;

            else
                Next();
        }
    }

    /// <summary>
    /// Proceeds to the next thing in the turn order if
    /// there is one.
    /// </summary>
    private void Next()
    {
        nextTurnRequested = false;
        turnEndInProgress = true;

        // Notify that the current actor's turn is ending
        if (Current != null)
        {
            Current.OnTurnEnd();
            TurnEnded?.Invoke(Current);
        }

        // If the round hasn't started notify that a new one
        // is starting
        if (!roundStarted)
        {
            RoundCount++;
            roundStarted = true;
            OnRoundStarting?.Invoke();
        }

        // Go to next thing in the turn order
        bool anyMore = order.MoveNext();

        // If there's nothing left in the order then the round has ended
        if (!anyMore)
        {
            roundStarted = false;
            order.Reset();
            OnRoundEnded?.Invoke();

            if (AutoLoop)
                RequestNextTurn();
        }

        // Otherwise, let the new thing know its turn is
        // starting
        else
        {
            // Notify the turn based thing FIRST - then other listeners.
            Current.OnTurnStart();
            TurnStarted?.Invoke(Current);
        }

        turnEndInProgress = false;

        if (nextTurnRequested)
            Next();
    }
}
