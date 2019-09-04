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
        var k = Quaternion.Euler(0, 0, 90) * new Vector3(direction.x, direction.y, 0);
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
            if (!QuadraticEquation(a * 0.5f, v0, -s, out roots))
                return 0;
            else
                return roots.x;
        }

        static float D(float a, float b, float c)
        {
            return b * b - 4 * a * c;
        }

        static bool QuadraticEquation(float a, float b, float c, out float2 res)
        {
            var d = D(a, b, c);
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
            var hasResult = QuadraticEquation(a, b, c, out n2);

            //так как n - это T^2, то n > 0. Более того, нас интересует минимильное вермя.
            if (!hasResult || (n2.x < 0 && n2.y < 0))
                //хз что делать... пускай под 45 грудусов будет...
                return new float2(absoluteVelocity * math.sign(delta.x), absoluteVelocity) / math.SQRT2;

            var n = math.min(n2.x, n2.y);
            if (n < 0) n = math.max(n2.x, n2.y);

            //|T| = Sqrt(n). Отрицательное значение отбрасываем. Т.к. нам надо положительное время...
            var time = math.sqrt(n);

            //ну и подставляем найденное Т в формулы (1) и (2)
            float2 velocity = float2.zero;
            velocity.x = delta.x / time - acceleration.x * time / 2;
            velocity.y = delta.y / time - acceleration.y * time / 2;

            return velocity;
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
