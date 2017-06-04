# TurnSystemUnity

Blurb about what the turn system is....
Two components are required to implement a turn system: the _Turn System_ and _Turn Based Entity_ components.






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

<img align="left" src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCQ3lMQnN4eHFUNms">

## Multiple turn orders

It is possible to nest Turn Systems. This is useful in a Team-Unit situation where each team acts in a turn based manner; but also, the units on each team activate in a set order. In the exmaple image each nested _TurnSystem_ has a _TurnBasedEntity_ component attached to it. The _TurnBasedEntity_ component's turn started event is connected to the associated _TurnSystem_'s _EndTurn_ method. Only the top-most turn system should have the _BeginOnLoad_ property checked.


- Add event listeners for the desired events: 


 or 
- _OrderChanged_
- Begin on Load: if you want the first turn to occur as soon as the game begins (called during Start)

2. Add turn based entity component to each item that is part of the turn order
- In the scene hierarchy, each entity must be a child the turn system
3. Set the priority of each entity; higher priority items get their turn first
4. Add event listeners for the desired events: _TurnStarting_ or _TurnEnding_


Hierarchy - entities are children

Have to call end turn on turn system - show example with button

Delete item, removes automatically from order

Nested turn systems

Hook up entity event to script on attached object
Add another listener for turn notification

Add UI listener for when order is changed

End of turn effects when Cycle Complete is called

Update priotity

Move next automatically when current is removed



Hierarchy
https://drive.google.com/open?id=0B9MQaq0nXQvCd2NTcjZ4eWdhQnM
