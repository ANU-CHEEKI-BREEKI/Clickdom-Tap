﻿using System;
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
    public static string ToShortFormattedString(this float number)
    {
        var numLen = ((int)number).ToString().Length;
        var periods = Mathf.Clamp(numLen - 2, 0, 255);
        var digits = (int)Mathf.Pow(10, periods);

        var div = 1;
        var postfix = "";

        if(periods >= 9)
        {
            div = 1_000_000_000;
            postfix = "B";
        }
        else if (periods >= 6)
        {
            div = 1_000_000;
            postfix = "M";
        }
        else if(periods >= 3)
        {
            div = 1_000;
            postfix = "K";
        }

        return (System.Math.Round((double)(number / digits), 0) * digits / div) + postfix;
    }

    public static Vector3 RewriteX (this Vector3 thisVector, float x)
    {
        thisVector.x = x;
        return thisVector;
    }

    public static Vector3 RewriteY(this Vector3 thisVector, float y)
    {
        thisVector.y = y;
        return thisVector;
    }

    public static Vector3 RewriteZ(this Vector3 thisVector, float z)
    {
        thisVector.z = z;
        return thisVector;
    }


    public static float Average2D(this Vector3 vector)
    {
        return (vector.x + vector.y) / 2;
    }

    public static float Average(this float2 vector)
    {
        return (vector.x + vector.y) / 2;
    }

    public static float Average(this float3 vector, bool safety = false)
    {
        if (!safety)
            return (vector.x + vector.y + vector.z) / 3;
        else
        {
            var x = (!float.IsInfinity(vector.x) && !float.IsNaN(vector.x)) ? vector.x : 1f;
            var y = (!float.IsInfinity(vector.y) && !float.IsNaN(vector.y)) ? vector.y : 1f;
            var z = (!float.IsInfinity(vector.z) && !float.IsNaN(vector.z)) ? vector.z : 1f;

            return (x + y + z) / 3;
        }
    }

    public static quaternion XLookTo(this quaternion q, float2 direction)
    {
        var k = Quaternion.Euler(0, 0, +90) * new Vector3(direction.x, direction.y, 0);
        return quaternion.LookRotation(new float3(0, 0, 1), new float3(k.x, k.y, 0));
    }

    public static float2 GetDirectionTo(this Vector2 thisPos, Vector2 targetPos)
    {
        return GetDirectionTo((float2)thisPos, (float2)targetPos);
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

    public static Vector2 ScreenToWorld(this Vector2 screenPos)
    {
        var cam = Camera.main;
        return ScreenToWorld(screenPos, cam);
    }

    public static Vector2 ScreenToWorld(this Vector2 screenPos,  Camera camera)
    {
        var mouseScreenPos = screenPos;
        var mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
        return new float2(mouseWorldPos.x, mouseWorldPos.y);
    }

    public static int GetFirstSetBitPos(this int n)
    {
        return (int)((Math.Log10(n & -n))
                / Math.Log10(2)) + 1;
    }
}

