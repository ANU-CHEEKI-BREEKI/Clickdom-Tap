using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ANU.Utils
{
    public static class Math
    {
        [Serializable]
        public struct LineEquation
        {
            public float k;
            public float b;

            public LineEquation(float k = 0, float b = 0)
            {
                this.k = k;
                this.b = b;
            }
        }

        /// <summary>
        /// ax^2 + bx + c = 0
        /// </summary>
        [Serializable]
        public struct QuadraticEquation
        {
            public float a;
            public float b;
            public float c;

            /// <summary>
            /// ax^2 + bx + c = 0
            /// </summary>
            public QuadraticEquation(float a = 0, float b = 0, float c = 0)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public float Solve(float x)
            {
                return a * x * x + b * x + c;
            }
        }

        public static float Descriminant(QuadraticEquation equation)
        {
            var a = equation.a;
            var b = equation.b;
            var c = equation.c;

            return b * b - 4 * a * c;
        }

        public static bool SolveQuadraticEquation(QuadraticEquation equation, out float2 res)
        {
            var a = equation.a;
            var b = equation.b;
            var c = equation.c;

            if (a != 0)
            {
                var d = Descriminant(equation);
                if (d < 0)
                {
                    res.x = 0;
                    res.y = res.x;
                    return false;
                }
                else if (d == 0)
                {
                    res.x =  (-b) / (2 * a);
                    res.y = res.x;
                }
                else
                {
                    res.x = (-b - math.sqrt(d)) / (2 * a);
                    res.y = (-b + math.sqrt(d)) / (2 * a);
                }
            }
            else 
            {
                if (b != 0)
                {
                    //bx + c = 0
                    //bx = -c
                    //x = -c/b
                    res.x =  (-c) / (b);
                    res.y = res.x;
                }
                else
                {
                    res.x = 0;
                    res.y = res.x;
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// if line equation is: [y = kx + b], returns [k] and [b] by passed (x1,y1) and (x2,y2). if line is vertical returns (NaN, x)
        /// </summary>
        public static LineEquation GetLineEquation(float2 linePoint1, float2 linePoint2)
        {
            if (linePoint1.x == linePoint2.x)
                return new LineEquation(float.NaN, linePoint1.x);

            var k = (linePoint1.y - linePoint2.y) / (linePoint1.x - linePoint2.x);
            return new LineEquation(k, linePoint1.y - k * linePoint1.x);
        }

        /// <summary>
        /// if line equation is: [y = kx + b]. if line is vertical returns (0, y), if horisontal returns (NaN,x)
        /// </summary>
        public static LineEquation GetPerpendicularLineEquation(float2 linePoint, LineEquation lineEquation)
        {
            if (float.IsNaN(lineEquation.k))
                return new LineEquation(0, linePoint.y);

            if (lineEquation.k == 0)
                return new LineEquation(float.NaN, linePoint.x);

            var k = -1f / lineEquation.k;
            return new LineEquation(k, linePoint.y - k * linePoint.x);
        }

        /// <summary>
        /// if line equation is: [y = kx + b]. if lines matches or parallel retrns (NaN,NaN)
        /// </summary>
        public static float2 GetLinesCrossPoint(LineEquation line1Equation, LineEquation line2Equation)
        {
            //если одна вертикальная а вторая нет, то считаем на прямую
            if (float.IsNaN(line1Equation.k) && !float.IsNaN(line2Equation.k))
            {
                return new float2(
                    line1Equation.b,
                    line2Equation.k * line1Equation.b + line2Equation.b
                );
            }
            else if (!float.IsNaN(line1Equation.k) && float.IsNaN(line2Equation.k))
            {
                return new float2(
                    line2Equation.b,
                    line1Equation.k * line2Equation.b + line1Equation.b
                );
            }
            else if (float.IsNaN(line1Equation.k) && float.IsNaN(line2Equation.k))
            {
                return new float2(float.NaN, float.NaN);
            }
            else
            {
                var cross = new float2();
                cross.x = (line2Equation.b - line1Equation.b) / (line1Equation.k - line2Equation.k);
                cross.y = line1Equation.k * cross.x + line1Equation.b;
                return cross;
            }
        }


        public static float2 GetLinesCrossPoint(float2 line1Start, float2 line1End, float2 line2Start, float2 line2End)
        {
            var l1Eq = GetLineEquation(line1Start, line1End);
            var l2Eq = GetLineEquation(line2Start, line2End);
            return GetLinesCrossPoint(l1Eq, l2Eq);
        }

        public static bool PointInsideRect(float2 firstCorner, float2 secondCorner, float2 point)
        {
            //возможные варианты входных параметров
            //  f----     ----f    s----    ----s 
            //  | 1 |     | 2 |    | 3 |    | 4 |
            //  ----s     s----    ----f    f----

            Rect rect;

            //1 variant
            if (firstCorner.x <= secondCorner.x && firstCorner.y >= secondCorner.y)
                rect = new Rect(
                    new Vector2(firstCorner.x, secondCorner.y),
                    new Vector2(secondCorner.x - firstCorner.x, firstCorner.y - secondCorner.y)
                );
            //2 variant
            else if (firstCorner.x > secondCorner.x && firstCorner.y >= secondCorner.y)
                rect = new Rect(
                   secondCorner,
                   firstCorner - secondCorner
               );
            // 3 variant
            else if (firstCorner.x > secondCorner.x && firstCorner.y < secondCorner.y)
                rect = new Rect(
                   new Vector2(secondCorner.x, firstCorner.y),
                   new Vector2(firstCorner.x - secondCorner.x, secondCorner.y - firstCorner.y)
               );
            // 4 variant
            else
                rect = new Rect(
                    firstCorner,
                    secondCorner - firstCorner
                );

            return rect.Contains(point);
        }

        public static bool IsSegmentIntersectsPoint(float2 segmentStart, float2 segmentEnd, float2 point, float radius = 0)
        {
            //получить уравнеие прямой
            var lineEquation = GetLineEquation(segmentStart, segmentEnd);
            //получить уравнение перпендикулярной прямой
            var perpendicularLineEquation = GetPerpendicularLineEquation(point, lineEquation);
            //найти точку пересечения прямых
            var crossPoint = GetLinesCrossPoint(lineEquation, perpendicularLineEquation);//совпадать прямые не могут!
                                                                                         //если пересеччени нет, то это фалс
            if (float.IsNaN(crossPoint.x) || float.IsNaN(crossPoint.y))
                return false;
            //если точки совпадают, то эта тру
            if (point.Equals(crossPoint))
                return true;
            //определить находится ли пересечение в отрезке
            if (!PointInsideRect(segmentStart, segmentEnd, point))
                return false;
            //найти расстояние от точки пересечения до point
            var sqrRadius = radius * radius;
            var sqrDist = math.distancesq(crossPoint, point);
            //если оно не больше radius, то норм
            return sqrDist <= sqrRadius;
        }

        /// <summary>
        /// under or left
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="trueResultIfLineIncludesPoint"></param>
        /// <returns></returns>
        public static bool PointUnderOrLeftLine(float2 point, LineEquation line, bool trueResultIfLineIncludesPoint = true)
        {
            if (float.IsNaN(line.k))
            {
                return trueResultIfLineIncludesPoint ? point.x <= line.b : point.x < line.b;
            }
            else
            {
                var lineY = line.k * point.x + line.b;
                return trueResultIfLineIncludesPoint ? point.y <= lineY : point.y < lineY;
            }
        }

        [Obsolete]
        public static float AngleRadians(float2 direction)
        {
            var res = math.PI / 2;
            if (direction.x != 0)
                res = math.atan(direction.y / direction.x);
            return res;
        }

        public static float AngleDegrees(float2 direction)
        {
            var res = Vector2.Angle(direction, Vector2.up);
            if (direction.x > 0)
                res = 360 - res;
            return res;
        }

        public static float UnityAngleDegreesTodefaultMath(this float unityAngle)
        {
            return unityAngle + 90;
        }
    }
}
