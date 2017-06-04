# TurnSystemUnity

The following is a Unity implementation of [this turn order](https://github.com/Clarksj4/TurnSystem). It is a priority ordered collection of items that allows enumeration. Two components are required to implement a turn system: the _Turn System_ and _Turn Based Entity_ components.

<img align="right" src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCd2NTcjZ4eWdhQnM">

## Hierarchy
In the scene hierarchy, the _TurnSystem_ component must be an ancestor of each _TurnBasedEntity_ component; the depth of the association does not matter (parent, grandparent, etc). The order of the _TurnBasedEntity_'s in the hierarchy does not impact the order in which they take their turns; it is determined solely by the entity's _Priority_ property.

---

<img align="left" src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCd1ZzZ05LRHZQTG8">

## Turn system component
The _TurnSystem_ component holds a reference to each child _TurnBasedEntity_, it notifies each entity in priority order whenever _EndTurn_ is called via script.

### Properties
_BeginOnLoad_: Causes the turn order to progress to the first entity when the game loads (called during Start)

_Paused_: If checked, interrupts calls to the _EndTurn_ method so that the turn is not ended while the system is paused.

### Events
_TurnStarting_: Called each time an entity begins its turn. __Example usage:__ update a turn order UI to highlight the entity whose turn it is.

_TurnEnding_: Called each time an entity ends its turn. __Example usage:__ resolve effects on entities, or a UI.

_CycleComplete_: Called each time the order has been completely cycled (i.e. every entity has had a turn). __Example usage:__ Draw new cards, update weather system or day/night cycle.

_OrderChanged_: Called whenever an entity is added, removed, or has its priority changed in the order. __Example usage:__ update a turn order UI; adding / removing an entity portrait, etc 

---

<img align="right" src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCNjQxa2t2VC1sdFk">

## Turn Based Entity component

### Properties

_Priority_: Determines the order in which entities are activated. Higher priority entities get their turn first.

### Events

_TurnStarting_: Called when __this__ entity's turn has begun. __Example Usage:__ Allow the attached object to act during its turn, display a notification that the objects turn has begun, etc

_TurnEnding_: Called when __this__ entity's turn has ended. __Example Usage:__ Check quest objectives (e.g. is the object standing in a particular spot), hide the turn notification, etc

---

## Removing Entities

Calling _Destroy_ on an entity will remove it from its associated _TurnSystem_. By default, removing the _Current_ entity from the _TurnSystem_ will cause the system to progress to the next entity. It is possible to prevent this by calling the _Remove_ method via script.

    public class TurnSystemRemoveExample : MonoBehaviour
    {
        TurnSystem turnSytem;
        TurnBasedEntity entity;

        public void RemoveExample()
        {
            // Proceed to next entity if this entity is the current entity
            bool nextIfCurrent = false;
            turnSystem.Remove(entity, nextIfCurrent);

            Destory(entity.gameObject);
            
            // ... Do something ...
            // ... Wait for animation to complete, end game, etc ...
            
            turnSystem.EndTurn();
        }
    }

---

<img align="left" src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCQ3lMQnN4eHFUNms">

## Multiple Turn Systems

It is possible to nest Turn Systems. This is useful in a Team-Unit situation where each team acts in a turn based manner; but also, the units on each team activate in a set order. In the example image each nested _TurnSystem_ has a _TurnBasedEntity_ component attached to it. The _TurnBasedEntity_ component's turn started event is connected to the associated _TurnSystem_'s _EndTurn_ method. Only the top-most turn system should have the _BeginOnLoad_ property checked.

---

## Equal Priorities

In the event that entities are added to a _TurnSystem_ that have the same priority, the system will iterate over them in insertion order.

---

## Demo

Clone or download the repo as a zip to check out the demo scene that shows the functionality of thes components.
