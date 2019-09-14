using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    #region extentions
    public static quaternion XLookTo(this quaternion q, float2 direction)
    {
        var k = Quaternion.Euler(0, 0, -90) * new Vector3(direction.x, direction.y, 0);
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

    public static bool EqualsEpsilon(this float2 thisPos, float2 targetPos, float epsilon)
    {
        return math.distancesq(thisPos, targetPos) <= epsilon * epsilon;
    }

    public static bool EqualsEpsilon(this float3 thisPos, float2 targetPos, float epsilon)
    {
        return EqualsEpsilon(new float2(thisPos.x, thisPos.y), targetPos, epsilon);
    }

    public static float3 GetNormalized(this float3 thisPos)
    {
        return math.normalizesafe(thisPos);
    }

    public static float2 GetNormalized(this float2 thisPos)
    {
        return math.normalizesafe(thisPos);
    }
    
    public static float2 ToF2(this float3 param)
    {
        return new float2(param.x, param.y);
    }

    public static float3 ToF3(this float2 param, float z = 0)
    {
        return new float3(param.x, param.y, z);
    }

    public static float2 ToF2(this Vector3 param)
    {
        return new float2(param.x, param.y);
    }

    public static Vector3 ToV3(this float2 param, float z = 0)
    {
        return new Vector3(param.x, param.y, z);
    }

    #endregion

    public static class Physics
    {

        /// <summary>
        /// Подсчитать перемещение
        /// </summary>
        /// <param name="v0">velocity</param>
        /// <param name="t">time</param>
        /// <param name="a">acceleration</param>
        /// <returns></returns>
        public static float GetDisplacement(float v0, float t, float a)
        {
            return v0 * t + (a * t * t) * 0.5f;
        }

        /// <summary>
        /// Подсчитать скорость с ускорением
        /// </summary>
        /// <param name="v0">velocity</param>
        /// <param name="t">time</param>
        /// <param name="a">acceleretion</param>
        /// <returns></returns>
        public static float GetVelocity(float v0, float t, float a)
        {
            return v0 + a * t;
        }

        /// <summary>
        /// За сколько времени можно переместить на s
        /// </summary>
        /// <param name="s">displacement</param>
        /// <param name="v0">velocity</param>
        /// <param name="a">acceleration</param>
        /// <returns></returns>
        public static float GetTime(float s, float v0, float a)
        {
            float2 roots;
            if (!Math.QuadraticEquation(a * 0.5f, v0, -s, out roots))
                return 0;
            else
                return roots.x;
        }
        
        /// <summary>
        /// Расчитать начальную скорость, чтобы переместить обьект с начала движения до самого конца.
        /// Перемещение будет расчитано с учетом горизонтального и вертикального движения. (как если кинуть камень или запустить стрелу)
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        public static float2 GetVelocity(float3 pos, float2 targetPos, float absoluteVelocity, float2 acceleration)
        {
            //изначально имеем систему:
            //    | delta.x = VxT + AxT^2/2     (1)
            //    < delta.y = VyT + AyT^2/2     (2)
            //    | V = Sqrt(Vx^2 + Vy^2)       (3)

            //сначала подсчитаем перемещение
            var delta = pos.GetDirectionTo(targetPos);//иииизи

            //подсчитаем время, для выполнения перемещения...
            //из (2) получаем Vy и подставляем в (3)
            //получаем: |Vy| = Sqrt(V^2 - (delta.x/T - AxT/2)^2)
            //раскрывая модуль, отбросим отрицательное значение. т.к. нам надо положительное значение скорости Vy
            //подставляем Vy в (2) уравнение и получем уравнение с T^4...
            //считая что T^2 = N, находим N из квадратного уравнения:
            var a = -acceleration.x * acceleration.x - acceleration.y * acceleration.y;
            var b = 4 * (absoluteVelocity * absoluteVelocity + delta.x * acceleration.x + delta.y * acceleration.y);
            var c = 4 * (-delta.x * delta.x - delta.y * delta.y);
            float2 n2;
            var hasResult = Math.QuadraticEquation(a, b, c, out n2);

            //так как n - это T^2, то n > 0. Более того, нас интересует минимильное вермя.
            if (!hasResult || (n2.x <= 0 && n2.y <= 0))
                //хз что делать... пускай под 45 грудусов будет...
                return new float2(absoluteVelocity * math.sign(delta.x), absoluteVelocity) / math.SQRT2;

            var n = math.min(n2.x, n2.y);
            if (n <= 0) n = math.max(n2.x, n2.y);

            //|T| = Sqrt(n). Отрицательное значение отбрасываем. Т.к. нам надо положительное время...
            var time = math.sqrt(n);

            //ну и подставляем найденное Т в формулы (1) и (2)
            float2 velocity = float2.zero;
            velocity.x = delta.x / time - acceleration.x * time / 2;
            velocity.y = delta.y / time - acceleration.y * time / 2;

            return velocity;
        }
                
    }

    public static class Math
    {
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

        public static float Descriminant(float a, float b, float c)
        {
            return b * b - 4 * a * c;
        }

        public static bool QuadraticEquation(float a, float b, float c, out float2 res)
        {
            var d = Descriminant(a, b, c);
            if (d < 0)
            {
                res.x = 0;
                res.y = 0;
                return false;
            }
            else if (d == 0)
            {
                res.x = res.y = (-b) / (2 * a);
            }
            else
            {
                res.x = (-b - math.sqrt(d)) / (2 * a);
                res.y = (-b + math.sqrt(d)) / (2 * a);
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
            return new LineEquation(k, -(k * linePoint1.x - linePoint1.y));
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
            return new LineEquation(k, linePoint.x / k + linePoint.y);
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
