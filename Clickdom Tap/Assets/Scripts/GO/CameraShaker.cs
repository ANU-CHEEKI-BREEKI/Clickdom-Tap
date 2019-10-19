using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShaker : MonoBehaviour
{
    private static List<CameraShaker> cameras = new List<CameraShaker>();
    private static Dictionary<CameraShaker, List<ShakeData>> shakes = new Dictionary<CameraShaker, List<ShakeData>>();

    [SerializeField] bool useLocalSettings;
    [SerializeField] ShakeSettings settings = ShakeSettings.Default;

    public bool UseLocalSettings { get { return useLocalSettings; } set { useLocalSettings = value; } }
    public ShakeSettings Settings { get { return settings; } set { settings = value; } }

    public Camera Camera { get; private set; }

    private void Awake()
    {
        Camera = GetComponent<Camera>();

        if (!cameras.Contains(this))
            cameras.Add(this);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        cameras.Remove(this);
        shakes.Remove(this);
    }

    public static void S_ShakeAllCameras(ShakeSettings settings)
    {
        foreach (var camera in cameras)
           camera.Shake(settings);
    }

    public void ShakeAllCameras(ShakeSettings settings)
    {
        CameraShaker.S_ShakeAllCameras(settings);
    }

    public void Shake(ShakeSettings settings)
    {
        if (UseLocalSettings)
            settings = this.Settings;

        if (settings.duration <= 0)
            return;

        List<ShakeData> shakeDatas;
        if (! shakes.ContainsKey(this))
            shakes[this] = new List<ShakeData>();
        shakeDatas = shakes[this];

        shakeDatas.Add(new ShakeData(settings));

        //start new coroutine
        if(shakeDatas.Count == 1)
            StartCoroutine(ShakeCamera(Camera, shakeDatas));
        //else we olready have coroutine with shakeDatas list
    }

    private static IEnumerator ShakeCamera(Camera camera, List<ShakeData> shakeDatas)
    {
        if (shakeDatas.Any())
        {
            var cachedCameraTransform = camera.transform;

            const int smoothFramesCount = 3;
            var smoothFrame = 0;

            var nextPosition = Vector3.zero;
            var previousPosition = Vector3.zero;
            var startPosition = cachedCameraTransform.position;

            while (shakeDatas.Any())
            {
                if (smoothFrame == 0)
                {
                    //calc affected magnitude
                    foreach (var data in shakeDatas)
                        data.affectedMagnitude = data.settings.magnitude * data.settings.affected.Evaluate(data.elapsedLifetime / data.settings.duration);

                    //take data with max affected magnitude 
                    var maxMagnitudeData = shakeDatas.OrderByDescending(d => d.affectedMagnitude).First();

                    //calc fmooth displacement
                    var displacement = new Vector3()
                    {
                        x = UnityEngine.Random.Range(-1f, 1f) * maxMagnitudeData.affectedMagnitude,
                        y = UnityEngine.Random.Range(-1f, 1f) * maxMagnitudeData.affectedMagnitude,
                        z = 0
                    };

                    //calc prev and next positions
                    previousPosition = cachedCameraTransform.position;
                    nextPosition = startPosition + displacement;
                }

                //apply smooth displacement by lerp prev and next pos by smooth frame
                cachedCameraTransform.position = Vector3.Lerp(
                    previousPosition,
                    nextPosition,
                    (float)smoothFrame / smoothFramesCount
                );

                //increace loop smooth frame
                smoothFrame = (smoothFrame + 1) % smoothFramesCount;

                //increace elapced lifetime
                var deltaTime = Time.deltaTime;
                foreach (var data in shakeDatas)
                    data.elapsedLifetime += deltaTime;

                //remove shake data wich was ended
                shakeDatas.RemoveAll(d => d.elapsedLifetime >= d.settings.duration);

                yield return null;
            }

            //set start position for correct ending shake
            cachedCameraTransform.position = startPosition;
        }
    }

    private class ShakeData
    {
        public ShakeSettings settings;
        public float elapsedLifetime = 0;
        public float affectedMagnitude = 0;

        public ShakeData(ShakeSettings settings)
        {
            this.settings = settings;
        }
    }

    [Serializable]
    public struct ShakeSettings
    {
        public float duration;
        public float magnitude;
        public AnimationCurve affected;

        public static ShakeSettings Default { get; } = new ShakeSettings()
        {
            duration = 2f,
            magnitude = 2f,
            affected = new AnimationCurve()
            {
                keys = new Keyframe[] 
                {
                    new Keyframe(0, 1),
                    new Keyframe(1, 1)
                },
                postWrapMode = WrapMode.Clamp,
                preWrapMode = WrapMode.Clamp
            }
        };
    }
}
