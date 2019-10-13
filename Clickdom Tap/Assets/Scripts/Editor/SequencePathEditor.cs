using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SequencePath))]
public class SequencePathEditor : Editor
{
    SequencePath path;
    bool needRepaint = false;
    SelectionInfo selectionInfo;

    float handleRadius = 0.2f;

    private void OnEnable()
    {
        path = target as SequencePath;
        selectionInfo = new SelectionInfo();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical(EditorStyles.helpBox);
        var label = EditorStyles.label;
        label.wordWrap = true;
        GUILayout.Label(
            "To modify positions use Ctrl and mouse left button to move and add points.\r\nUse Shift and left mouse button to remove positions.",
            label
        );
        GUILayout.EndVertical();
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        var guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();

            var rect = new Rect(0, 0, 9999, 9999);
            if (guiEvent.modifiers == EventModifiers.Control)
            {
                if (selectionInfo.mouseOverPoint)
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
                else
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowPlus);
            }
            else if (guiEvent.modifiers == EventModifiers.Shift)
            {
                if(selectionInfo.mouseOverPoint)
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.ArrowMinus);
            }
        }
        else if (guiEvent.type == EventType.Layout && guiEvent.modifiers != EventModifiers.None)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            //обновляем хэндлы
            if (needRepaint)
                HandleUtility.Repaint();
        }
    }

    private void HandleInput(Event guiEvent)
    {
        var mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        var mousePosition = mouseRay.GetPoint(0);
        mousePosition.z = path.transform.position.z;
        mousePosition = path.transform.InverseTransformPoint(mousePosition);
        
        if (guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Control)
        {
            //добавляем точки
            if (guiEvent.type == EventType.MouseDown)
            {
                HandleLeftMouseDown(mousePosition);
            }
            else if (guiEvent.type == EventType.MouseUp)
            {
                HandleLeftMouseUp(mousePosition);
            }
            else if (guiEvent.type == EventType.MouseDrag)
            {
                HandleLeftMouseDrag(mousePosition);
            }
        }
        else if (guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            if (guiEvent.type == EventType.MouseDown)
                HandleLeftMouseDownShift(mousePosition);
        }
        
        UpdatePointSelection(mousePosition, guiEvent);

    }

    private void HandleLeftMouseDownShift(Vector3 mousePosition)
    {
        if (selectionInfo.mouseOverPoint &&path.LocalPoints.Count > SequencePath.MinPositionsCount)
        {
            Undo.RecordObject(path, "Remove point at Sequence Path");
            path.LocalPoints.RemoveAt(selectionInfo.pointIndex);
            selectionInfo.pointIsSelected = false;
            selectionInfo.mouseOverPoint = false;
            needRepaint = true;
        }
    }

    private void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if (!selectionInfo.mouseOverPoint)
        {
            var newPointIndex = selectionInfo.mouseOverIsLine ? selectionInfo.lineIndex + 1 : path.LocalPoints.Count;
            Undo.RecordObject(path, "Add point at Sequence Path");
            path.LocalPoints.Insert(newPointIndex, mousePosition);
            selectionInfo.pointIndex = newPointIndex;
            selectionInfo.mouseOverIsLine = false;
            selectionInfo.lineIndex = -1;
        }

        selectionInfo.pointIsSelected = true;
        selectionInfo.positionApStartOfDrag = mousePosition;
        needRepaint = true;
    }

    private void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected)
        {
            //устанавливаем Undo
            path.LocalPoints[selectionInfo.pointIndex] = selectionInfo.positionApStartOfDrag;
            Undo.RecordObject(path, "Move point at Sequence Path");
            path.LocalPoints[selectionInfo.pointIndex] = mousePosition;

            selectionInfo.pointIsSelected = false;
            selectionInfo.pointIndex = -1;
            needRepaint = true;
        }
    }

    private void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected)
        {
            path.LocalPoints[selectionInfo.pointIndex] = mousePosition;
            needRepaint = true;
        }
    }

    private void Draw()
    {
        //рисуем хэндлы
        for (int i = 0; i < path.LocalPoints.Count; i++)
        {
            Transform handleTransform = path.transform;
            var point = path.LocalPoints[i];
            point = handleTransform.TransformPoint(point);

            if (i < path.LocalPoints.Count - 1)
            {
                var nextPoint = path.LocalPoints[(i + 1) % path.LocalPoints.Count];
                nextPoint = handleTransform.TransformPoint(nextPoint);

                Handles.color = Color.yellow;
                var quaternion = Quaternion.LookRotation(nextPoint - point, new Vector3(0, 0, 1));
                Handles.ArrowHandleCap(
                    GUIUtility.GetControlID(FocusType.Passive),
                    point,
                    quaternion,
                    handleRadius * 7,
                    EventType.Repaint
                );

                if (selectionInfo.lineIndex == i)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(point, nextPoint);
                }
                else
                {
                    Handles.color = Color.grey;
                    Handles.DrawDottedLine(point, nextPoint, 5);
                }

                
            }

            if (selectionInfo.pointIndex == i)
                Handles.color = selectionInfo.pointIsSelected ? Color.black : Color.red;
            else
                Handles.color = Color.white;
            Handles.DrawSolidDisc(point, Vector3.forward, handleRadius);
        }
        needRepaint = false;
    }

    private void UpdatePointSelection(Vector3 mousePosition, Event guiEvent)
    {
        if (guiEvent.modifiers == EventModifiers.Shift || guiEvent.modifiers == EventModifiers.Control)
        {
            if (!selectionInfo.pointIsSelected)
            {
                int mouseOverPointIndex = -1;
                for (int i = 0; i < path.LocalPoints.Count; i++)
                    if (Vector3.Distance(mousePosition, path.LocalPoints[i]) <= handleRadius)
                        mouseOverPointIndex = i;
                if (mouseOverPointIndex != selectionInfo.pointIndex)
                {
                    selectionInfo.pointIndex = mouseOverPointIndex;
                    selectionInfo.mouseOverPoint = mouseOverPointIndex != -1;
                    needRepaint = true;
                }
            }

            if (selectionInfo.mouseOverPoint)
            {
                selectionInfo.mouseOverIsLine = false;
                selectionInfo.lineIndex = -1;
            }
            else
            {
                int mouseOverLineIndex = -1;
                float closestLineDistance = handleRadius;
                for (int i = 0; i < path.LocalPoints.Count; i++)
                {
                    var nextPoint = path.LocalPoints[(i + 1) % path.LocalPoints.Count];
                    var distToLine = HandleUtility.DistancePointToLineSegment(mousePosition, path.LocalPoints[i], nextPoint);
                    if (distToLine <= closestLineDistance)
                    {
                        mouseOverLineIndex = i;
                        closestLineDistance = distToLine;
                    }
                }

                if (selectionInfo.lineIndex != mouseOverLineIndex)
                {
                    selectionInfo.lineIndex = mouseOverLineIndex;
                    selectionInfo.mouseOverIsLine = mouseOverLineIndex != -1;
                    needRepaint = true;
                }
            }
        }
        else
        {
            selectionInfo.pointIndex = -1;
            selectionInfo.mouseOverPoint = false;
            selectionInfo.pointIsSelected = false;
            selectionInfo.lineIndex = -1;
            selectionInfo.mouseOverIsLine = false;
            needRepaint = true;
        }
    }

    public class SelectionInfo
    {
        public int pointIndex = -1;
        public bool mouseOverPoint = false;
        public bool pointIsSelected = false;

        public Vector3 positionApStartOfDrag;

        public int lineIndex;
        public bool mouseOverIsLine;
    }
}
