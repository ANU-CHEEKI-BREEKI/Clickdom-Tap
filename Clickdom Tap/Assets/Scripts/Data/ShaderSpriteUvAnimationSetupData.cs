using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using System;

[CreateAssetMenu(fileName = "SpriteUvAnimation")]
public class ShaderSpriteUvAnimationSetupData : ScriptableObject
{
    /// <summary>
    /// для будущего использования
    /// </summary>
    public enum AnimationType { LOOP }//, PING_PONG, SINGLE, SINGLE_AND_DESTROY }

    [Header("Рендер")]
    [SerializeField] Material material;
    [SerializeField] Mesh mesh;

    [Header("Данные анимации")]
    [SerializeField] Sprite spriteSheet;
    [SerializeField] int framesCount;
    [SerializeField] float frameDuration;
    [SerializeField] AnimationType type;
    [SerializeField] int minInitFrame;
    [SerializeField] int maxInitFrame;
    [SerializeField] LayerMask layer;
    [Header("События анимации")]
    [Tooltip("нужно ли приостанавливать анимацию")]
    [SerializeField] bool needPauseOnFrame;
    [Tooltip("приостановить анимацию на этом кадре")]
    [SerializeField] PauseData pauseData;
    [Tooltip("нужен ли триггер тригер для какого то события")]
    [SerializeField] bool needActionOnFrame;
    [Tooltip("тригер для какого то события на этом кадре")]
    //[SerializeField] int actionOnFrame;
    [SerializeField] ActionData actionData;

    public float FrameDuration => frameDuration;
    public int FramesCount => framesCount;
    public float HorisontalOffset => spriteSheet.rect.x / spriteSheet.texture.width;
    public float VerticalOffset => spriteSheet.rect.y / spriteSheet.texture.height;
    public float FrameWidth => spriteSheet.rect.width / spriteSheet.texture.width / framesCount;
    public float FrameHeigth => spriteSheet.rect.height / spriteSheet.texture.height;
    public int RamdomInitFrame => UnityEngine.Random.Range(minInitFrame, maxInitFrame + 1);
    public int MinInitFrame => minInitFrame;
    public int MaxInitFrame => maxInitFrame;
    public Material Material => material;
    public Mesh Mesh => mesh;
    /// <summary>
    /// get uv for 0 frame of current initiated spritesheet
    /// </summary>
    public Vector4 UV => GetUvForFrame(0);
    public Vector4 RandomUV => GetUvForFrame(UnityEngine.Random.Range(0, FramesCount));

    public Vector4 GetUvForFrame(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= FramesCount)
            throw new ArgumentException(nameof(frameIndex) + $"equals {frameIndex}");

        return new Vector4(
            FrameWidth,
            FrameHeigth,
            HorisontalOffset + FrameWidth * frameIndex,
            VerticalOffset
        );
    }

    public bool NeedPauseOnSomeFrames => needPauseOnFrame;
    public PauseData PauseData => pauseData;

    public bool NeedActionOnSomeFrames => needActionOnFrame;
    public ActionData ActionData => actionData;

    Dictionary<Sprite, Mesh> meshes = new Dictionary<Sprite, Mesh>();
    public Mesh GeneratedMesh
    {
        get
        {
            if (!meshes.ContainsKey(spriteSheet))
                meshes.Add(spriteSheet, SpriteToMesh(spriteSheet));
            return meshes[spriteSheet];
        }
    }
    public AnimationType Type { get { return type; } }
       
    private Mesh CreateMeshFor(Sprite sprite)
    {
        return new Mesh()
        {
            vertices = sprite.vertices.Select(v => (Vector3)v).ToArray(),
            uv = sprite.uv,
            triangles = sprite.triangles.Select(t => (int)t).ToArray()
        };
    }

    private Mesh CreateMeshFor(float width, float height)
    {
        var halfHeight = height / 2;
        var halfWidth = width / 2;

        var vertices = new Vector3[4];
        var uv = new Vector2[4];
        var triangles = new int[6];

        //0, 0
        //0, 1
        //1, 1
        //1, 0

        vertices[0] = new Vector3(-halfWidth, -halfHeight);
        vertices[1] = new Vector3(-halfWidth, +halfHeight);
        vertices[2] = new Vector3(+halfWidth, +halfHeight);
        vertices[3] = new Vector3(+halfWidth, -halfHeight);

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        return new Mesh()
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };

    }

    private Mesh SpriteToMesh(Sprite sprite)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(Array.ConvertAll(sprite.vertices, i => (Vector3)i).ToList());
        mesh.SetUVs(0, sprite.uv.ToList());
        mesh.SetTriangles(Array.ConvertAll(sprite.triangles, i => (int)i).ToList(), 0);
        return mesh;
    }

    private Texture CreateTextureForSprite(Sprite sprite)
    {
        var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        );
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    private Material CreateMaterialForTexture(Material defaultMaterial, Texture texture)
    {
        return new Material(defaultMaterial) { mainTexture = texture };
    }

    private void OnValidate()
    {
        framesCount = math.clamp(framesCount, 0, framesCount);
        maxInitFrame = math.clamp(maxInitFrame, 0, framesCount);
        minInitFrame = math.clamp(minInitFrame, 0, maxInitFrame);
    }
}