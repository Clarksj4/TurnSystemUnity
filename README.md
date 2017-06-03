# TurnSystemUnity

## Usage

Two components are required to implement a turn system: the _Turn System_ and _Turn Based Entity_ components.

<p align="center">
  <img src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCd1ZzZ05LRHZQTG8">
  <img src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCNjQxa2t2VC1sdFk">
</p>


### Hierarchy

<img align="left" src="https://drive.google.com/uc?export=view&id=0B9MQaq0nXQvCd2NTcjZ4eWdhQnM">

In the scene hierarchy, the _TurnSystem_ component must be an ancestor of each _TurnBasedEntity_ component. The depth of the association in the hierarchy does not matter (parent, grandparent, etc).



### Turn system component



- Add event listeners for the desired events: 
- _TurnStarting_
- _TurnEnding_, 
- _CycleComplete_, or 
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
