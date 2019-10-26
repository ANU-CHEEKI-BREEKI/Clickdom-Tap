using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    public static quaternion XLookTo(this quaternion q, float2 direction)
    {
        var k = Quaternion.Euler(0, 0, +90) * new Vector3(direction.x, direction.y, 0);
        return quaternion.LookRotation(new float3(0, 0, 1), new float3(k.x, k.y, 0));
    }

    public static float2 GetDirectionTo(this float3 thisPos, float2 targetPos)
    {
        return GetDirectionTo(new float2(thisPos.x, thisPos.y), targetPos);
    }

    public static float2 GetDirectionTo(this float2 thisPos, float2 targetPos)
    {
        return targetPos - thisPos;
    }

    public static float2 GetDirectionTo(this float2 thisPos, float3 targetPos)
    {
        return GetDirectionTo(thisPos, new float2(targetPos.x, targetPos.y));
    }

    public static bool EqualsEpsilon(this float3 thisPos, float3 targetPos, float epsilon)
    {
        return math.distancesq(thisPos, targetPos) <= epsilon * epsilon;
    }

    public static bool EqualsEpsilon(this float2 thisPos, float2 targetPos, float epsilon)
    {
        return math.distancesq(thisPos, targetPos) <= epsilon * epsilon;
    }

    public static bool EqualsEpsilon(this float3 thisPos, float2 targetPos, float epsilon)
    {
        return EqualsEpsilon(new float2(thisPos.x, thisPos.y), targetPos, epsilon);
    }

    /// <summary>
    /// сравнить, какая из позиций ближе к thisPos. если targetPos1 - то return -1. если targetPos2 - то return 1. иначе - 0.
    /// </summary>
    /// <param name="thisPos"></param>
    /// <param name="targetPos"></param>
    /// <param name="anotherPos"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static int CompareToAnother(this float3 thisPos, float3 targetPos1, float3 targetPos2)
    {
        var target1DistSq = math.distancesq(thisPos, targetPos1);
        var target2DistSq = math.distancesq(thisPos, targetPos2);

        return target1DistSq < target2DistSq ? -1 : target1DistSq > target2DistSq ? 1 : 0;
    }

    public static float3 GetNormalized(this float3 thisPos)
    {
        return math.normalizesafe(thisPos);
    }

    public static float2 GetNormalized(this float2 thisPos)
    {
        return math.normalizesafe(thisPos);
    }

    public static float3 ToF3(this float2 param, float z = 0)
    {
        return new float3(param.x, param.y, z);
    }

    public static float2 ToF2(this float3 param)
    {
        return new float2(param.x, param.y);
    }

    public static float2 ToF2(this Vector3 param)
    {
        return new float2(param.x, param.y);
    }

    public static Vector3 ToV3(this float2 param, float z = 0)
    {
        return new Vector3(param.x, param.y, z);
    }

    public static float2 GetMouseWorldPosition()
    {
        var cam = Camera.main;
        return GetMouseWorldPosition(cam);
    }

    public static float2 GetMouseWorldPosition(Camera camera)
    {
        var mouseScreenPos = Input.mousePosition;
        var mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
        return new float2(mouseWorldPos.x, mouseWorldPos.y);
    }
}

