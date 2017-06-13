using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityController : MonoBehaviour
{
    public GameObject Prefab;
    public TurnSystem turnSystem;

	public void Add()
    {
        // Create another entity
        GameObject instance = Instantiate(Prefab, turnSystem.transform);

        // Place at a random position on nav mesh
        RandomMovement movement = instance.GetComponent<RandomMovement>();
        if (movement != null)
            instance.transform.position = movement.RandomRoamDestination();
    }

    public void Remove()
    {
        Destroy(turnSystem.Current.gameObject);
    }

    public void UpdatePriority(Slider slider)
    {
        turnSystem.Current.Priority = slider.value;
    }
}
