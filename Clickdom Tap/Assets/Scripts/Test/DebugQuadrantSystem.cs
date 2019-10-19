using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugQuadrantSystem : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var camera = Camera.main;
        var cameraPosition = camera.transform.position;
        var camHeight = camera.orthographicSize;
        var camWidth = camHeight * camera.aspect;
        var maxX = cameraPosition.x + camWidth;
        var minX = cameraPosition.x - camWidth;
        var maxY = cameraPosition.y + camHeight;
        var minY = cameraPosition.y - camHeight;

        var xStep = QuadrantSystem.xQuadrantSize;
        var yStep = QuadrantSystem.yQuadrantSize;

        for (float x = minX; x < maxX; x += xStep)
        {
            for (float y = minY; y < maxY; y += yStep)
            {
                QuadrantSystem.DrawQuadrant(new Unity.Mathematics.float3(x, y, 0), Color.black);
            }
        }

        var mousePos = Utils.GetMouseWorldPosition(camera);
        QuadrantSystem.DrawQuadrant(mousePos.ToF3(), Color.red);
    }
}