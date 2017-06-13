using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystemUI : MonoBehaviour
{
    [Tooltip("The turn system this UI is tracking")]
    public TurnSystem turnSystem;
    [Tooltip("Prefab of UI item to be displayed for each turn based entity")]
    public RectTransform UIPrefab;

    [Header("Content")]
    [Tooltip("The panel to instantiate UI items into and position according to current entity")]
    public RectTransform ContentPanel;
    [Tooltip("UI panel containing mask")]
    public RectTransform Viewport;
    [Tooltip("The time it takes the content to scroll to a selected item")]
    public float ScrollDuration;
    [Tooltip("Number of content items to be visible (not masked)")]
    public int VisibleItems;

    [Header("Selection")]
    [Tooltip("Selection overlay")]
    public RectTransform Selection;
    [Tooltip("Number of units by which the selection is larger than the content items")]
    public Vector2 SelectionSizePadding;

    private Dictionary<TurnBasedEntity, RectTransform> entityCatalogue = new Dictionary<TurnBasedEntity, RectTransform>();

    /// <summary>
    /// Recreate the list of entitys on the UI
    /// </summary>
    public void Refresh()
    {
        // Remove all current entities
        Clear();

        // Add each entity
        foreach (var entity in turnSystem.Order)
            Add(entity);

        // Order them, and scroll to the current entity
        PositionItems();
        ScrollToCurrent();
    }

    /// <summary>
    /// Scrolls to the current entity over time
    /// </summary>
    public void TurnStarted()
    {
        ScrollToCurrent();
    }

	void Start ()
    {
        // Size elements based on prefab size
        Viewport.sizeDelta = new Vector2(Viewport.sizeDelta.x, VisibleItems * UIPrefab.sizeDelta.y);
        Selection.sizeDelta = new Vector2(Selection.sizeDelta.x, UIPrefab.sizeDelta.y) + SelectionSizePadding;

        // Fill with entities from order
        Refresh();
	}
	
    /// <summary>
    /// Removes all entitys from the UI and destroys their game objects
    /// </summary>
    void Clear()
    {
        foreach (var item in entityCatalogue.Values)
            DestroyImmediate(item.gameObject);

        // Increase content panel size to accomodate new item
        ContentPanel.sizeDelta = Vector2.zero;

        entityCatalogue.Clear();
    }

    /// <summary>
    /// Add an entity to the UI
    /// </summary>
    void Add(TurnBasedEntity entity)
    {
        // Increase content panel size to accomodate new item
        ContentPanel.sizeDelta += (Vector2.up * UIPrefab.sizeDelta.y);

        // Create associated UI element, remember association
        RectTransform entityUI = Instantiate(UIPrefab, ContentPanel.transform);
        entityCatalogue.Add(entity, entityUI);

        // [PLACHOLDER] TODO: Get portrait
        entityUI.GetComponentInChildren<Text>().text = entity.name;
    }

    /// <summary>
    /// Orders the items in the UI according to their priority
    /// </summary>
    void PositionItems()
    {
        int i = 0;
        foreach (var entity in turnSystem.Order)
        {
            // Get associated RectTransform, place in order
            RectTransform ui = entityCatalogue[entity];
            ui.anchoredPosition = new Vector2(0, (-i * ui.sizeDelta.y));
            ui.SetAsLastSibling();

            i++;
        }
    }

    /// <summary>
    /// Scrolls to the current entity over time
    /// </summary>
    void ScrollToCurrent()
    {
        int index = -1;

        var current = turnSystem.Current;
        if (current != null)
        {
            var associatedRect = entityCatalogue[current];
            index = associatedRect.GetSiblingIndex();
        }

        ScrollTo(index);
    }

    /// <summary>
    /// Scrolls nth entity in the order over times
    /// </summary>
    void ScrollTo(int index)
    {
        StopAllCoroutines();
        StartCoroutine(DoScroll(index, ScrollDuration));
    }

    /// <summary>
    /// Gets the position to anchor the content panel at to display the entity at index
    /// </summary>
    Vector2 IndexPosition(int index)
    {
        Vector2 top = new Vector2(0, -(ContentPanel.sizeDelta.y / 2));                      // Top of order
        Vector2 zeroIndex = top + (Vector2.up * (UIPrefab.sizeDelta.y / 2));                // Centered on first item
        Vector2 indexPosition = zeroIndex + (Vector2.up * (UIPrefab.sizeDelta.y * index));  // Centered on index item

        return indexPosition;
    }

    /// <summary>
    /// Coroutine scrolls the content panel over time
    /// </summary>
    IEnumerator DoScroll(int index, float duration)
    {
        // Lerp between current position and position of index over duration
        Vector2 start = ContentPanel.anchoredPosition;
        Vector2 destination = IndexPosition(index);
        float time = 0;

        while (time < duration)
        {
            // Percent complete
            float t = time / duration;

            // Incremental scroll
            ContentPanel.anchoredPosition = Vector2.Lerp(start, destination, t);

            // Track time
            time += Time.deltaTime;
            yield return null;
        }

        // Clamp to destination
        ContentPanel.anchoredPosition = destination;
    }
}
