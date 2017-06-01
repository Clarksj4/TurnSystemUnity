using System;

namespace TurnBased
{
    /// <summary>
    /// Interface for object that operates in a turn based manner
    /// </summary>
    public interface ITurnBased<T> where T : IComparable<T>
    {
        /// <summary>
        /// The pawn's priority in the turn order
        /// </summary>
        T Priority { get; }

        /// <summary>
        /// The pawn's turn is starting
        /// </summary>
        void TurnStart();

        /// <summary>
        /// The pawn's turn is ending
        /// </summary>
        void TurnEnd();


    }
}
