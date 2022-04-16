using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TurnSystem))]
public class TurnSystemEditor : Editor
{
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

    private GUIContent gameObjectIcon = null;
    private Vector2 scrollPosition = Vector2.zero;
    private double lastClickTimestamp = 0f;
    private const double DOUBLE_CLICK_THRESHOLD = 0.3f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ListenForRepaint();
        DrawOrder();
        DrawFields();
        DrawEvents();
        serializedObject.ApplyModifiedProperties();
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
        TurnSystem turnSystem = target as TurnSystem;

        // Header
        OrderExpanded = EditorGUILayout.Foldout(OrderExpanded, "Order", true, EditorStyles.foldoutHeader);
        if (OrderExpanded)
        {
            // Scroll view
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope
            (
                scrollPosition,
                false,
                false,
                GUIStyle.none,
                "VerticalScrollbar",
                "Box",
                GUILayout.Height(CalculateScrollViewHeight())
            ))
            {
                scrollPosition = scrollView.scrollPosition;

                // If there's actors draw them
                if (turnSystem.ActorCount > 0)
                {
                    foreach (var actor in turnSystem.Order)
                    {
                        // Two horizontal layouts are used: an inner one that contains
                        // the button, and an outer one that contains a flexible space
                        // so the button is reduced in size to fit its content.
                        Rect outerRect = EditorGUILayout.BeginHorizontal();
                        {
                            Rect innerRect = EditorGUILayout.BeginHorizontal();
                            {
                                // Make button green if it's this actor's turn
                                Color previous = GUI.color;
                                if (turnSystem.Current == actor)
                                    GUI.color = Color.green;

                                // Ping or select the actor's game object when the button is clicked.
                                if (GUI.Button(innerRect, GUIContent.none))
                                {
                                    double clickTimestamp = EditorApplication.timeSinceStartup;

                                    // If double clicked - select the object.
                                    if (clickTimestamp - lastClickTimestamp < DOUBLE_CLICK_THRESHOLD)
                                        Selection.activeGameObject = actor.gameObject;

                                    // If single click - ping the object.
                                    else
                                        EditorGUIUtility.PingObject(actor.gameObject);


                                    lastClickTimestamp = clickTimestamp;
                                }
                                    
                                GUI.color = previous;

                                // Show game object icon
                                GUILayout.Label(GameObjectIcon, GUILayout.MaxHeight(16));

                                // Show actor information
                                GUILayout.Label(actor.name);
                                GUILayout.Label(":");
                                GUILayout.Label(actor.Priority.ToString());

                                // Padding
                                GUILayout.Space(5);
                            }
                            EditorGUILayout.EndHorizontal();

                            // Absorb the remaining space so the button is sized to
                            // fit its content.
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
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
    }

    private void DrawFields()
    {
        // Foldout header
        SettingsExpanded = EditorGUILayout.Foldout(SettingsExpanded, "Settings", true, EditorStyles.foldoutHeader);
        if (SettingsExpanded)
        {
            SerializedProperty autoStart = serializedObject.FindProperty("AutoStart");
            EditorGUILayout.PropertyField(autoStart);

            SerializedProperty autoLoop = serializedObject.FindProperty("AutoLoop");
            EditorGUILayout.PropertyField(autoLoop);
        }
    }

    private void DrawEvents()
    {
        // Foldout header
        EventsExpanded = EditorGUILayout.Foldout(EventsExpanded, "Events", true, EditorStyles.foldoutHeader);
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
    }

    private float CalculateScrollViewHeight()
    {
        TurnSystem turnSystem = target as TurnSystem;

        // Default height
        float scrollViewHeight = 18;

        // If there are actors make enough room for them but
        // limit it to the height of 6 actors.
        if (turnSystem.ActorCount > 0)
        {
            float contentHeight = turnSystem.ActorCount * 18;
            scrollViewHeight = Mathf.Min(contentHeight, 6 * 18);
        }

        return scrollViewHeight;
    }

    private void Repaint(TurnBasedEntity entity)
    {
        Repaint();
    }
}
