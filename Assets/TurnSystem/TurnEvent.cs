using System;
using UnityEngine;
using UnityEngine.Events;

namespace TurnBased
{
    [Serializable]
    public class TurnEvent : UnityEvent<TurnBasedEntity>
    {

    }
}
