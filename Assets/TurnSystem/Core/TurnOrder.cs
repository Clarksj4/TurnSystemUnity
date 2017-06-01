using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TurnBased
{
    /// <summary>
    /// Stores and cycles pawns in priority order
    /// </summary>
    public class TurnOrder<T> : IEnumerable<ITurnBased<T>> where T : IComparable<T>
    {
        /// <summary>
        /// The pawn whose turn it currently is. Returns null if the current pawn was removed from the order
        /// </summary>
        public ITurnBased<T> Current
        {
            get
            {
                if (currentToBeRemoved ||   // If Current was removed from list, don't return its ref
                    currentNode == null)    // Current node not yet set
                    return null;

                // Get pawn from linked list node
                return currentNode.Value;
            }
        }

        /// <summary>
        /// The number of pawns in this turn order.
        /// </summary>
        public int Count { get; private set; }

        private LinkedList<ITurnBased<T>> pawns;
        private LinkedListNode<ITurnBased<T>> currentNode;
        private bool currentToBeRemoved;

        /// <summary>
        /// An empty turn order
        /// </summary>
        public TurnOrder()
        {
            pawns = new LinkedList<ITurnBased<T>>();
            currentNode = null;
            currentToBeRemoved = false;
            Count = 0;
        }

        /// <summary>
        /// Inserts the pawn in order based upon its priority
        /// </summary>
        public void Insert(ITurnBased<T> pawn)
        {
            // Can't insert null pawn
            if (pawn == null)
                throw new ArgumentNullException("Pawn cannot be null");

            // Can't have duplicates in order
            if (pawns.Contains(pawn))
                throw new ArgumentException("Order already contains pawn");

            // First pawn to be inserted
            if (pawns.Count == 0)
                pawns.AddFirst(pawn);

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

            Count++;
        }

        /// <summary>
        /// Removes the pawn from the turn order
        /// </summary>
        public bool Remove(ITurnBased<T> pawn)
        {
            // Can't remove null pawn
            if (pawn == null)
                throw new ArgumentNullException("Pawn cannot be null");

            // Find pawn's node incase its the current node
            LinkedListNode<ITurnBased<T>> node = pawns.Find(pawn);

            if (node == null)
                return false;

            // If current pawn is being removed, marked it as removed
            if (node == currentNode)
                currentToBeRemoved = true;
            else
                pawns.Remove(node);

            // Pawn successfully removed
            Count--;
            return true;
        }

        /// <summary>
        /// Updates the pawn's position in the turn order based upon its priority
        /// </summary>
        public void UpdatePriority(ITurnBased<T> pawn)
        {
            // Can't update pawn marked for removal
            if (currentToBeRemoved && currentNode.Value == pawn)
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

            // Remember current node so it can be recycled if necessary
            var node = currentNode;

            // Move to next pawn in order
            bool isMore = Cycle();

            // Remove previous node if it was marked
            DeferredRecycle(node);

            // Notify current of turn start
            StartCurrent();

            // True if there are more pawns in the order who have not had their turn during this cycle
            return isMore;
        }

        public IEnumerator<ITurnBased<T>> GetEnumerator()
        {
            // Don't return the node that is marked for removal
            if (currentToBeRemoved)
                return pawns.Where(p => p != currentNode.Value).GetEnumerator();

            else
                return pawns.GetEnumerator();
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

        void DeferredRecycle(LinkedListNode<ITurnBased<T>> node)
        {
            // Remove current node if its been marked
            if (currentToBeRemoved)
                pawns.Remove(node);

            // Update mark
            currentToBeRemoved = false;
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
            // Don't return the node that is marked for removal
            if (currentToBeRemoved)
                return pawns.Where(p => p != currentNode.Value).GetEnumerator();

            else
                return pawns.GetEnumerator();
        }
    }
}
