using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TurnBased;

[CustomEditor(typeof(TurnSystem))]
public class TurnSystemEditor : Editor
{
    private const float ACTOR_ITEM_HEIGHT = 18f;
    private const float ACTOR_ITEM_PADDING = 5.5f;
    private const float MAX_ACTOR_COUNT = 6;
    private const float MAX_SCROLL_VIEW_HEIGHT = (MAX_ACTOR_COUNT * ACTOR_ITEM_HEIGHT) + ((MAX_ACTOR_COUNT - 1) * ACTOR_ITEM_PADDING) + (2 * ACTOR_ITEM_PADDING);
    private const double DOUBLE_CLICK_THRESHOLD = 0.3d;
    private const double FOCUS_HOLD_DURATION = 0.1d;

    /// <summary>
    /// Get a lazily cached game object icon.
    /// </summary>
    private GUIContent GameObjectIcon
    {
        get
        {
            if (gameObjectIcon == null)
                gameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon");
            return gameObjectIcon;
        }
    }

    /// <summary>
    /// Gets or sets the serialized state of the foldout.
    /// </summary>
    private bool OrderExpanded
    {
        get { return EditorPrefs.GetBool($"{target.GetInstanceID()}_OrderExpanded", true); }
        set { EditorPrefs.SetBool($"{target.GetInstanceID()}_OrderExpanded", value); }
    }

    /// <summary>
    /// Gets or sets the serialized state of the foldout.
    /// </summary>
    private bool SettingsExpanded
    {
        get { return EditorPrefs.GetBool($"{target.GetInstanceID()}_SettingsExpanded", true); }
        set { EditorPrefs.SetBool($"{target.GetInstanceID()}_SettingsExpanded", value); }
    }

    /// <summary>
    /// Gets or sets the serialized state of the foldout.
    /// </summary>
    private bool EventsExpanded
    {
        get { return EditorPrefs.GetBool($"{target.GetInstanceID()}_EventsExpanded", true); }
        set { EditorPrefs.SetBool($"{target.GetInstanceID()}_EventsExpanded", value); }
    }

    private double lastClickTimestamp = 0d;
    private double holdFocusTimestamp = 0d;
    private GUIContent gameObjectIcon = null;
    private Vector2 scrollPosition = Vector2.zero;
    private TurnBasedEntity deferredActor = null;
    private float deferredPriority = 0f;
    private string wantedFocus = null;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ListenForRepaint();
        DrawOrder();
        DrawFields();
        DrawEvents();
        serializedObject.ApplyModifiedProperties();

        // If an actor's priority was marked for change - make the
        // changes now.
        UpdateDeferredActorPriority();
        HighlightActivePriorityField();
    }

    private void ListenForRepaint()
    {
        TurnSystem turnSystem = target as TurnSystem;

        // Listen for turns starting and ending so we can highlight the correct actor.
        turnSystem.TurnStarted.RemoveListener(Repaint);
        turnSystem.TurnStarted.AddListener(Repaint);

        turnSystem.TurnEnded.RemoveListener(Repaint);
        turnSystem.TurnEnded.AddListener(Repaint);

        // Listen for the order changing so we can highlight the correct actor.
        turnSystem.OrderChanged.RemoveListener(Repaint);
        turnSystem.OrderChanged.AddListener(Repaint);
    }

    private void DrawOrder()
    {
        // Header
        OrderExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(OrderExpanded, "Order", EditorStyles.foldoutHeader);
        if (OrderExpanded)
        {
            DrawActorListHeader();
            DrawActorList();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        //HandleDragAndDrop();
    }

    private void DrawActorListHeader()
    {
        TurnSystem turnSystem = target as TurnSystem;

        // Show header if there's any actors.
        if (turnSystem.ActorCount > 0)
        {
            Rect headerRect = EditorGUILayout.BeginHorizontal("Toolbar");
            {
                // Wrap the actor label so that its area doesn't overlap priority label.
                EditorGUILayout.LabelField("Actor", new GUIStyle("WordWrappedLabel"));
                EditorGUILayout.LabelField("Priority", GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawActorList()
    {
        TurnSystem turnSystem = target as TurnSystem;

        // Scroll view
        using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope
        (
            scrollPosition,
            false,
            false,
            GUIStyle.none,
            "VerticalScrollbar",
            "ObjectPickerBackground",
            GUILayout.Height(CalculateScrollViewHeight())
        ))
        {
            scrollPosition = scrollView.scrollPosition;

            // If there's actors draw them
            if (turnSystem.ActorCount > 0)
            {
                IEnumerable<TurnBasedEntity> actors = turnSystem.CurrentRoundOrder ?? turnSystem.NextRoundOrder;
                foreach (var actor in actors)
                {
                    // If an actor was removed from this list, stop iteration.
                    if (DrawActorListItem(turnSystem, actor))
                        break;
                }
            }

            // There's no actors - just draw an informative message.
            else
            {
                // Information label for when there are no actors.
                GUILayout.Label("Actors will appear here when they are added to the turn order.", EditorStyles.centeredGreyMiniLabel);
            }
        }
    }

    private bool DrawActorListItem(TurnSystem turnSystem, TurnBasedEntity actor)
    {
        // Make button green if it's this actor's turn
        Color previous = GUI.color;
        if (turnSystem.Current == actor)
            GUI.color = Color.green;

        // Two horizontal layouts are used: an inner one that contains
        // the button, and an outer one that contains a flexible space
        // so the button is reduced in size to fit its content.
        Rect outerRect = EditorGUILayout.BeginHorizontal("Button");
        {
            GUI.color = previous;

            Rect leftRect = EditorGUILayout.BeginHorizontal();
            {
                // Ping or select the actor's game object when the button is clicked.
                if (GUI.Button(leftRect, GUIContent.none, GUIStyle.none))
                    HandleActorClicked(actor);

                // Show game object icon - limit height in order to scale icon
                // down to a reasonable size.
                GUILayout.Label(GameObjectIcon, GUILayout.MaxHeight(ACTOR_ITEM_HEIGHT));

                // Show actor information - wrap text with fixed height so that
                // the text gets truncated if it tries to overlap float field
                // space.
                GUILayout.Label(actor.name, "WordWrapLabel", GUILayout.Height(ACTOR_ITEM_HEIGHT));

                // Absorb the remaining space so the button is sized to fit its
                // content.
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            // Each float field is given an ID corresponding to the actor it
            // represents.
            string controlName = $"{actor.name}_{actor.GetInstanceID()}";
            GUI.SetNextControlName(controlName);
            float priority = EditorGUILayout.DelayedFloatField(actor.Priority, EditorStyles.textField, GUILayout.MinWidth(80), GUILayout.Width(80));
            if (priority != actor.Priority)
                DeferActorPriorityUpdate(actor, priority, controlName);

            // Show game object icon - limit height in order to scale icon
            // down to a reasonable size.
            if (GUILayout.Button(GUIContent.none, "SearchCancelButton"))
                return turnSystem.Remove(actor);
        }
        EditorGUILayout.EndHorizontal();

        return false;
    }

    private void HandleActorClicked(TurnBasedEntity actor)
    {
        double clickTimestamp = EditorApplication.timeSinceStartup;

        // If double clicked - select the object.
        if (clickTimestamp - lastClickTimestamp < DOUBLE_CLICK_THRESHOLD)
            Selection.activeGameObject = actor.gameObject;

        // If single click - ping the object (will show it in hierarchy)
        else
            EditorGUIUtility.PingObject(actor.gameObject);


        lastClickTimestamp = clickTimestamp;
    }

    private void HandleDragAndDrop()
    {
        Rect lastRect = GUILayoutUtility.GetLastRect();
        if (lastRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }

            else if (Event.current.type == EventType.DragPerform)
            {
                TurnSystem turnSystem = target as TurnSystem;
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject)
                    {
                        GameObject gameObject = obj as GameObject;
                        if (gameObject.TryGetComponent(out TurnBasedEntity entity))
                            turnSystem.Insert(entity);
                    }
                        
                }
                Event.current.Use();
            }
        }
    }

    private void DrawFields()
    {
        // Foldout header
        SettingsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(SettingsExpanded, "Settings", EditorStyles.foldoutHeader);
        if (SettingsExpanded)
        {
            SerializedProperty autoStart = serializedObject.FindProperty("AutoStart");
            EditorGUILayout.PropertyField(autoStart);

            SerializedProperty autoLoop = serializedObject.FindProperty("AutoLoop");
            EditorGUILayout.PropertyField(autoLoop);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawEvents()
    {
        // Foldout header
        EventsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(EventsExpanded, "Events", EditorStyles.foldoutHeader);
        if (EventsExpanded)
        {
            SerializedProperty turnStarted = serializedObject.FindProperty("TurnStarted");
            EditorGUILayout.PropertyField(turnStarted);

            SerializedProperty turnEnded = serializedObject.FindProperty("TurnEnded");
            EditorGUILayout.PropertyField(turnEnded);

            SerializedProperty orderChanged = serializedObject.FindProperty("OrderChanged");
            EditorGUILayout.PropertyField(orderChanged);

            SerializedProperty onRoundStarting = serializedObject.FindProperty("OnRoundStarting");
            EditorGUILayout.PropertyField(onRoundStarting);

            SerializedProperty onRoundEnded = serializedObject.FindProperty("OnRoundEnded");
            EditorGUILayout.PropertyField(onRoundEnded);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private float CalculateScrollViewHeight()
    {
        TurnSystem turnSystem = target as TurnSystem;

        // Default height
        float scrollViewHeight = ACTOR_ITEM_HEIGHT;

        // If there are actors make enough room for them but
        // limit it to the height of 6 actors.
        if (turnSystem.ActorCount > 0)
        {
            float contentHeight = (turnSystem.ActorCount * ACTOR_ITEM_HEIGHT) + ((turnSystem.ActorCount - 1) * ACTOR_ITEM_PADDING) + (ACTOR_ITEM_PADDING * 2);
            scrollViewHeight = Mathf.Min(contentHeight, MAX_SCROLL_VIEW_HEIGHT);
        }

        return scrollViewHeight;
    }

    private void Repaint(TurnBasedEntity entity)
    {
        Repaint();
    }

    private void HighlightActivePriorityField()
    {
        if (wantedFocus != null)
        {
            double currentTime = EditorApplication.timeSinceStartup;
            double focusDuration = currentTime - holdFocusTimestamp;
            string currentFocus = GUI.GetNameOfFocusedControl();
            bool isFocused = currentFocus == wantedFocus;

            // Hold focus on the control for a limited time so it doesn't get lost during
            // future repaints.
            if (isFocused && focusDuration > FOCUS_HOLD_DURATION)
                wantedFocus = null;

            else
                GUI.FocusControl(wantedFocus);
        }
    }

    private void UpdateDeferredActorPriority()
    {
        if (deferredActor != null)
        {
            deferredActor.Priority = deferredPriority;
            deferredActor = null;
            Repaint();
        }
    }

    private void DeferActorPriorityUpdate(TurnBasedEntity actor, float priority, string controlID)
    {
        holdFocusTimestamp = EditorApplication.timeSinceStartup;
        wantedFocus = controlID;
        deferredActor = actor;
        deferredPriority = priority;
    }
}
