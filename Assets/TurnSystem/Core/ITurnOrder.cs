using System;
using System.Collections.Generic;

namespace TurnBased
{
    public interface ITurnOrder<T> : IReadOnlyCollection<ITurnBased<T>> 
        where T : IComparable<T>
    {
        /// <summary>
        /// The current actor whose turn it is.
        /// </summary>
        ITurnBased<T> Current { get; }
        /// <summary>
        /// Adds the given actor to the current round order.
        /// </summary>
        void Insert(ITurnBased<T> actor);
        /// <summary>
        /// Removes the given actor from the current round order.
        /// </summary>
        bool Remove(ITurnBased<T> actor);
        /// <summary>
        /// Updates the actors position in the turn order.
        /// </summary>
        void UpdatePriority(ITurnBased<T> actor);
        /// <summary>
        /// Gets the next actor in the round.
        /// </summary>
        bool MoveNext();
        /// <summary>
        /// Reset this round.
        /// </summary>
        void Reset();
        /// <summary>
        /// Checks if this turn order contains the given actor.
        /// </summary>
        bool Contains(ITurnBased<T> actor);
    }
}