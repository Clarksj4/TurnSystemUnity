using System;
using System.Collections;
using System.Collections.Generic;

namespace TurnBased
{
    /// <summary>
    /// Stores and cycles pawns in priority order
    /// </summary>
    public class TurnOrder<T> : ITurnOrder<T> 
        where T : IComparable<T>
    {
        /// <summary>
        /// The pawn whose turn it currently is. Returns null if the current pawn was removed from the order
        /// </summary>
        public ITurnBased<T> Current
        {
            get
            {
                if (currentNode == null ||       // Current node not yet set
                    toBeRemoved == currentNode)  // If Current was removed from list, don't return its ref
                    return null;

                // Get pawn from linked list node
                return currentNode.Value;
            }
        }

        /// <summary>
        /// The number of pawns in this turn order.
        /// </summary>
        public int Count { get { return toBeRemoved == null ? pawns.Count : pawns.Count - 1; } }

        private LinkedList<ITurnBased<T>> pawns = new LinkedList<ITurnBased<T>>();
        private LinkedListNode<ITurnBased<T>> currentNode = null;
        private LinkedListNode<ITurnBased<T>> toBeRemoved = null;

        /// <summary>
        /// Inserts the pawn in order based upon its priority
        /// </summary>
        public void Insert(ITurnBased<T> pawn)
        {
            // Can't insert null pawn
            if (pawn == null)
                throw new ArgumentNullException("Pawn cannot be null");

            // Can't have duplicates in order
            if (Contains(pawn))
                throw new ArgumentException("Order already contains pawn");

            // First pawn to be inserted
            if (pawns.Count == 0)
                pawns.AddFirst(pawn);

            // If the pawn was marked for removal, insert it back into the order
            // at the correct position.
            else if (MarkedForRemoval(pawn))
            {
                toBeRemoved = null;
                UpdatePriority(pawn);
            }

            else
            {
                var walker = pawns.First;

                // Walk until finding a smaller node
                while (walker != null && 
                       pawn.Priority.CompareTo(walker.Value.Priority) < 0)
                    walker = walker.Next;

                // Add to end of order
                if (walker == null)
                    pawns.AddLast(pawn);

                // Add in front of smaller node
                else
                    pawns.AddBefore(walker, pawn);
            }
        }

        /// <summary>
        /// Removes the pawn from the turn order
        /// </summary>
        public bool Remove(ITurnBased<T> pawn)
        {
            // Can't remove null pawn
            if (pawn == null)
                throw new ArgumentNullException("Pawn cannot be null");

            // If its their turn - don't remove them until the end of the round
            // OR maybe the local ref handles this already?
            if (currentNode != null &&
                currentNode.Value == pawn)
            {
                toBeRemoved = currentNode;
                return true;
            }

            else
                return pawns.Remove(pawn);
        }

        /// <summary>
        /// Updates the pawn's position in the turn order based upon its priority
        /// </summary>
        public void UpdatePriority(ITurnBased<T> pawn)
        {
            // Can't update pawn marked for removal
            if (MarkedForRemoval(pawn))
                throw new ArgumentException("Order does not contain pawn");

            // Remove from order
            bool removed = Remove(pawn);

            // Can't update pawn if it doesn't exists in order
            if (removed == false)
                throw new ArgumentException("Order does not contain pawn");

            // Re-insert into order in correct position
            Insert(pawn);
        }

        /// <summary>
        /// Move to the next pawn in the turn order. Notifies the next pawn of its turn starting.
        /// </summary>
        /// <returns>True if there is another pawn in the order who has not had its turn yet during this cycle</returns>
        public bool MoveNext()
        {
            // Can't move to next pawn in order if there is none
            if (pawns.Count == 0)
                throw new InvalidOperationException("Order is empty");

            // Notify current of turn end
            EndCurrent();

            // Move to next pawn in order
            bool isMore = Cycle();

            // Remove previous node if it was marked
            DeferredRecycle();

            // Notify current of turn start
            StartCurrent();

            // True if there are more pawns in the order who have not had their turn during this cycle
            return isMore;
        }

        /// <summary>
        /// Reset this round of actors. Subsequent calls to MoveNext() will
        /// begin at the start of the turn order.
        /// </summary>
        public void Reset()
        {
            currentNode = null;
        }

        /// <summary>
        /// Checks if this turn order contains the given pawn.
        /// </summary>
        public bool Contains(ITurnBased<T> pawn)
        {
            return !MarkedForRemoval(pawn) && pawns.Contains(pawn);
        }

        public IEnumerator<ITurnBased<T>> GetEnumerator()
        {
            foreach (ITurnBased<T> pawn in pawns)
            {
                // Don't return the node that is marked for removal
                if (!MarkedForRemoval(pawn))
                    yield return pawn;
            }  
        }

        /// <summary>
        /// Checks if the given pawn is marked to be removed from the list of pawns.
        /// </summary>
        private bool MarkedForRemoval(ITurnBased<T> pawn)
        {
            return toBeRemoved != null &&
                    toBeRemoved.Value != null &&
                    toBeRemoved.Value == pawn;
        }


        /// <summary>
        /// Informs the current pawn that its turn has ended
        /// </summary>
        void EndCurrent()
        {
            // If thing has been removed from order, do not notify it of turn end
            if (Current != null)
                Current.TurnEnd();
        }

        void DeferredRecycle()
        {
            // Remove current node if its been marked
            if (toBeRemoved != null)
                pawns.Remove(toBeRemoved);

            // Update mark
            toBeRemoved = null;
        }

        /// <summary>
        /// Proceed to next pawn in order. 
        /// </summary>
        /// <returns>True if there is another pawn in the order who has not had its turn yet during this cycle</returns>
        bool Cycle()
        {
            // Move to next pawn in order
            if (currentNode == null)
                currentNode = pawns.First;

            else
                currentNode = currentNode.Next;

            return currentNode != null;
        }

        /// <summary>
        /// Inform current pawn that its their turn
        /// </summary>
        void StartCurrent()
        {
            // Activate current pawn...
            if (Current != null)
                Current.TurnStart();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
