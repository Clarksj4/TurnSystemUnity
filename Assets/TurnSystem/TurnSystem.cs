using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
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
    /// Gets the actors in the order they will act this round - null if the round hasn't started.
    /// </summary>
    public IEnumerable<TurnBasedEntity> CurrentRoundOrder => currentOrder?.Where(a => actors.Contains(a));
    /// <summary>
    /// Gets the actors in the order they will act next round.
    /// </summary>
    public IEnumerable<TurnBasedEntity> NextRoundOrder => actors?.OrderByDescending(a => a.Priority);
    /// <summary>
    /// Gets the actor whose turn it is.
    /// </summary>
    public TurnBasedEntity Current
    {
        get
        {
            // If the actor exists and has not been removed from the order.
            if (currentNode != null && actors.Contains(currentNode.Value))
                return currentNode.Value;
            else
                return null;
        }
    }
    /// <summary>
    /// Gets the number of actors in the turn system.
    /// </summary>
    public int ActorCount { get { return actors.Count; } }
    /// <summary>
    /// The index of the current round.
    /// </summary>
    public int RoundCount { get; private set; }

    [SerializeField]
    private List<TurnBasedEntity> actors = new();
    private LinkedList<TurnBasedEntity> currentOrder = null;
    private LinkedListNode<TurnBasedEntity> currentNode = null;

    private bool turnEndInProgress = false;
    private bool nextTurnRequested = false;

    private void Start()
    {
        EditorApplication.playModeStateChanged -= ModeChanged;
        EditorApplication.playModeStateChanged += ModeChanged;

        if (AutoStart)
            QueueNextTurn();
    }

    private void ModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            currentNode = null;
            currentOrder = null;
        }
    }

    /// <summary>
    /// Gets the current round of actors, or the next round of actors if the current one is null.
    /// </summary>
    public IEnumerable<TurnBasedEntity> GetCurrentOrNextRoundOrder()
    {
        return CurrentRoundOrder ?? NextRoundOrder;
    }

    public int GetCurrentOrNextRoundOrderCount()
    {
        return GetCurrentOrNextRoundOrder()?.Count() ?? 0;
    }

    /// <summary>
    /// Insert an entity into the order
    /// </summary>
    public void Insert(TurnBasedEntity entity)
    {
        // Get your null shit out of here.
        if (entity == null)
            throw new ArgumentException("Can't insert a null entity.");

        // Insert into current turn order so long as the entity doesn't
        // already exist.
        if (!actors.Contains(entity))
        {
            actors.Add(entity);
            if (Application.IsPlaying(gameObject))
                InsertIntoCurrentOrder(entity);
            OrderChanged?.Invoke();
        }
    }

    /// <summary>
    /// Remove an entity from the order. If the current entity is removed, the turn order progresses to the next entity
    /// </summary>
    public bool Remove(TurnBasedEntity entity)
    {
        if (entity == null)
            throw new ArgumentException("Can't remove a null entity.");

        bool isCurrent = entity == Current;

        // Check if the current actor is being removed
        bool removed = actors.Remove(entity);
        if (removed)
        {
            OrderChanged?.Invoke();

            // If the current actor was removed, proceed to the next turn.
            if (isCurrent)
                QueueNextTurn();
            else
            {
                if (Application.IsPlaying(gameObject))
                    currentOrder.Remove(entity);
            }
        }

        return removed;
    }

    /// <summary>
    /// Update the priority of the given entity, reordering entities
    /// </summary>
    public void UpdatePriority(TurnBasedEntity actor)
    {
        if (currentNode != null)
            throw new ArgumentException("Can't update priority during a round.");

        // Notify that an object has had its priority changed
        OrderChanged?.Invoke();
    }

    /// <summary>
    /// Checks if the turn system contains the given actor.
    /// </summary>
    public bool Contains(TurnBasedEntity actor)
    {
        return actors.Contains(actor);
    }

    /// <summary>
    /// Queues the next turn (in case there is a turn end already being processed)
    /// </summary>
    public void QueueNextTurn()
    {
        // Only do a thing if the order is not empty
        if (actors.Count > 0)
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

    private void Next()
    {
        nextTurnRequested = false;
        turnEndInProgress = true;

        // If there is currently an actor, end their turn.
        if (currentNode != null)
            EndCurrentActorsTurn();
        else
            StartRound();

        // Move to the next actor in order.
        MoveToNextNode();

        // If there is a next actor, then start their turn.
        if (currentNode != null)
            StartCurrentActorsTurn();
        else
            EndRound();

        // Go to next turn if there is one queued.
        turnEndInProgress = false;
        if (nextTurnRequested)
            Next();
    }

    private void EndCurrentActorsTurn()
    {
        if (currentNode != null)
        {
            // Actor gets notified FIRST.
            Current?.OnTurnEnd();
            TurnEnded?.Invoke(Current);
        }
    }

    private void StartRound()
    {
        if (currentNode == null)
        {
            // Lock in the order of actors for the upcoming round.
            currentOrder = NextRoundOrder.ToLinkedList();
            RoundCount++;
            OnRoundStarting?.Invoke();
        }
    }

    private void StartCurrentActorsTurn()
    {
        if (currentNode != null)
        {
            // Actor gets notified FIRST
            Current?.OnTurnStart();
            TurnStarted?.Invoke(Current);
        }
    }

    private void EndRound()
    {
        if (currentNode == null)
        {
            OnRoundEnded?.Invoke();

            // Clear the current order of actors - a new order will be determined
            // at the start of the round.
            currentOrder = null;

            // Queue another turn if set to auto loop.
            if (AutoLoop)
                QueueNextTurn();
        }
    }

    private void InsertIntoCurrentOrder(TurnBasedEntity entity)
    {
        if (currentOrder != null)
        {
            LinkedListNode<TurnBasedEntity> walker = currentOrder.First;

            // Walk until there's no more, or we find one with lower priority
            while (walker != null && entity.Priority > walker.Value.Priority)
                walker = walker.Next;

            // Add in front of the last found entity, or last if there was none.
            if (walker == null)
                currentOrder.AddLast(entity);
            else
                currentOrder.AddBefore(walker, entity);
        }
    }

    private void MoveToNextNode()
    {
        // Get the next node, or null if there isn't one.
        if (currentNode == null)
            currentNode = currentOrder.First;
        else
            currentNode = currentNode.Next;
    }
}
