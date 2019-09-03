using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "SpriteUvAnimation")]
public class ShaderSpriteUvAnimationSetupData : ScriptableObject
{
    /// <summary>
    /// для будущего использования
    /// </summary>
    public enum AnimationType { LOOP }//, PING_PONG, SINGLE, SINGLE_AND_DESTROY }

    [Header("Данные анимации")]
    [SerializeField] Sprite spriteSheet;
    [SerializeField] int framesCount;
    [SerializeField] float frameDuration;
    [SerializeField] AnimationType type;
    [SerializeField] int minInitFrame;
    [SerializeField] int maxInitFrame;
    [Header("События анимации")]
    [Tooltip("приостановить анимацию на этом кадре")]
    [SerializeField] int setPauseOnFrame;
    [Tooltip("тригер для какого то события на этом кадре")]
    [SerializeField] int actionOnFrame;

    public float FrameDuration => frameDuration;
    public int FramesCount => framesCount;
    public float HorisontalOffset => spriteSheet.rect.x / spriteSheet.texture.width;
    public float VerticalOffset => spriteSheet.rect.y / spriteSheet.texture.height;
    public float FrameWidth => spriteSheet.rect.width / spriteSheet.texture.width / framesCount;
    public float FrameHeigth => spriteSheet.rect.height / spriteSheet.texture.height;
    public int RamdomInitFrame => UnityEngine.Random.Range(minInitFrame, maxInitFrame + 1);
    public int MinInitFrame => minInitFrame;
    public int MaxInitFrame => maxInitFrame;

    public AnimationType Type { get { return type; } }

    private void OnValidate()
    {
        framesCount = math.clamp(framesCount, 0, framesCount);
        maxInitFrame = math.clamp(maxInitFrame, 0, framesCount);
        minInitFrame = math.clamp(minInitFrame, 0, maxInitFrame);
    }
}