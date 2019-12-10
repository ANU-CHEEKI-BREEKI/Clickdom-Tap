using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ANU.Utils
{
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
        /// <param name="s">displacement (s >= 0)</param>
        /// <param name="v0">velocity (v0 >= 0)</param>
        /// <param name="a">acceleration</param>
        /// <returns></returns>
        public static float GetTime(float s, float v0, float a)
        {
            if (s < 0)
                throw new ArgumentException(nameof(s));
            if (v0 < 0)
                throw new ArgumentException(nameof(v0));

            // s = v0*t + a*t^2/2
            // at^2 + 2*v0*t - 2*s = 0

            float2 roots;
            if (!ANU.Utils.Math.SolveQuadraticEquation(new Math.QuadraticEquation(a, 2 * v0, -2 * s), out roots))
                return 0;
            else
            {
                roots.x = math.max(0, roots.x);
                roots.y = math.max(0, roots.y);
                return math.min(roots.x, roots.y);
            }
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
            var hasResult = ANU.Utils.Math.SolveQuadraticEquation(new Math.QuadraticEquation(a, b, c), out n2);

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
}
