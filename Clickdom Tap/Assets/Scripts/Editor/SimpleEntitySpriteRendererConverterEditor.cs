using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(SimpleEntitySpriteRendererConverter))]
//public class SimpleEntitySpriteRendererConverterEditor : Editor
//{
//    private bool drawRectEditor = false;

//    private static GUIStyle toggleButtonStyleNormal = null;
//    private static GUIStyle toggleButtonStyleToggled = null;
//    private const string EDIT_RENDER_SCALE_BUTTON_TEXT = "edit render scale";

//    private SimpleEntitySpriteRendererConverter _target;

//    private void OnEnable()
//    {
//        _target = target as SimpleEntitySpriteRendererConverter;

//        drawRectEditor = false;
//    }

//    private void OnDisable()
//    {
//        drawRectEditor = false;
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        InitStyles();

//        EditorGUILayout.Space();
//        if (GUILayout.Button(
//            EDIT_RENDER_SCALE_BUTTON_TEXT,
//            drawRectEditor ? toggleButtonStyleToggled : toggleButtonStyleNormal
//        ))
//        {
//            drawRectEditor = !drawRectEditor;
//        }
//    }

//    private void OnSceneGUI()
//    {
//        if (!drawRectEditor)
//            return;

//        var guiEvent = Event.current;


//        if (guiEvent.type == EventType.Repaint)
//        {
//            Draw();
//        }
//        else if (guiEvent.type == EventType.Layout && guiEvent.modifiers != EventModifiers.None)
//        {
//            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
//        }
//        else
//        {
//            HandleInput(guiEvent);
//            //обновляем хэндлы
//            //if (needRepaint)
//                HandleUtility.Repaint();
//        }

        
//    }

//    private void HandleInput(Event guiEvent)
//    {
//        var mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
//        var mousePosition = mouseRay.GetPoint(0);
//        mousePosition.z = _target.transform.position.z;
//        mousePosition = _target.transform.InverseTransformPoint(mousePosition);

//        //if (guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Control)
//        //{
//        //    //добавляем точки
//        //    if (guiEvent.type == EventType.MouseDown)
//        //    {
//        //        HandleLeftMouseDown(mousePosition);
//        //    }
//        //    else if (guiEvent.type == EventType.MouseUp)
//        //    {
//        //        HandleLeftMouseUp(mousePosition);
//        //    }
//        //    else if (guiEvent.type == EventType.MouseDrag)
//        //    {
//        //        HandleLeftMouseDrag(mousePosition);
//        //    }
//        //}
//        //else if (guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
//        //{
//        //    if (guiEvent.type == EventType.MouseDown)
//        //        HandleLeftMouseDownShift(mousePosition);
//        //}

//        //UpdatePointSelection(mousePosition, guiEvent);

//    }

//    private void Draw()
//    {
//        var rendScaleProperty = serializedObject.FindProperty("renderScale");
//        var renderScale = rendScaleProperty.vector2Value;

//        Handles.DrawWireCube(
//            (target as MonoBehaviour).transform.position,
//            renderScale
//        );
//        var rect = new Rect(Vector2.zero, Vector2.one);
//        var color = Color.green;
//        var outlineColor = color;
//        var pos = (Vector2)_target.transform.position;
//        Handles.DrawSolidRectangleWithOutline(
//            new Rect(rect)
//            {
//                x = pos.x + renderScale.x,
//                y = pos.y
//            },
//            color,
//            outlineColor
//        );
//        Handles.DrawSolidRectangleWithOutline(
//            new Rect(rect)
//            {
//                x = pos.x - renderScale.x,
//                y = pos.y
//            },
//            color,
//            outlineColor
//       );
//        Handles.DrawSolidRectangleWithOutline(
//            new Rect(rect)
//            {
//                x = pos.x,
//                y = pos.y + renderScale.y
//            },
//            color,
//            outlineColor
//       );
//        Handles.DrawSolidRectangleWithOutline(
//            new Rect(rect)
//            {
//                x = pos.x,
//                y = pos.y - renderScale.y
//            },
//            color,
//            outlineColor
//       );
//    }

//    private static void InitStyles()
//    {
//        if (toggleButtonStyleNormal == null)
//        {
//            toggleButtonStyleNormal = "Button";
//            toggleButtonStyleToggled = new GUIStyle(toggleButtonStyleNormal);
//            toggleButtonStyleToggled.normal.background = toggleButtonStyleToggled.active.background;
//        }
//    }
//}
