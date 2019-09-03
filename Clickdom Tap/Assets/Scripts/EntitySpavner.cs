using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;
using Unity.Jobs;
using Unity.Burst;

public struct ArcherTagComponentData : IComponentData { }

public class EntitySpavner : MonoBehaviour
{
    [SerializeField] Sprite zombieSprite;
    [SerializeField] Material defaultMaterial;

    [SerializeField] public Mesh quadMesh;
    [SerializeField] public Material animatedMeterial;
    [Space]
    [SerializeField] Sprite arrowSprite;
    [SerializeField] Material defMeterial;
    [SerializeField] public Mesh arrowMesh;
    [SerializeField] public Material arrowMeterial;

    Mesh arrowQuad = null;
    public Mesh ArrowQuad { get { if (arrowQuad == null) arrowQuad = CreateMeshFor(arrowSprite); return arrowQuad;} }
    Material arrowMaterial = null;
    public Material ArrowMaterial { get { if (arrowMaterial == null) arrowMaterial = CreateMaterialForTexture(defMeterial, CreateTextureForSprite(arrowSprite)); return arrowMaterial; } }

    [Space]
    [SerializeField] int entityCount = 1000;
    [SerializeField] ShaderSpriteUvAnimationSetupData archerAnimation;

    public static EntitySpavner Instance { get; private set; }

    EntityManager entityManager;
    public EntityManager EntityManager => entityManager;

    private void Awake()
    {
        Instance = this;
        entityManager = World.Active.EntityManager;
    }

    void Start()
    {
        var archetipe = entityManager.CreateArchetype(
            typeof(ArcherTagComponentData),
            typeof(Translation),
           // typeof(MovementComponentData),
            typeof(VelocityAbsoluteComponentData),
            typeof(SpriteSheetAnimationComponentData),
            typeof(Scale),
            typeof(ScaleByPositionComponentData)
          );

        var entities = new NativeArray<Entity>(entityCount, Allocator.Temp);
        entityManager.CreateEntity(archetipe, entities);

        foreach (var entity in entities)
        {
            entityManager.SetComponentData(entity, new Translation()
            {
                Value = new float3(UnityEngine.Random.Range(-7f, 0f), UnityEngine.Random.Range(-4f, 4f), 0f)
            });

            //entityManager.SetComponentData(entity, new MovementComponentData()
            //{
            //    positionToMove = new float2(UnityEngine.Random.Range(-6f, 6f), UnityEngine.Random.Range(-6f, 6f)),
            //    isMoving = true                
            //});

            entityManager.SetComponentData(entity, new VelocityAbsoluteComponentData()
            {
                value = UnityEngine.Random.Range(0f, 2f)
            });

            entityManager.SetComponentData(entity, new SpriteSheetAnimationComponentData()
            {
                currentFrame = archerAnimation.RamdomInitFrame,
                frameCount = archerAnimation.FramesCount,
                frameDuration = archerAnimation.FrameDuration,
                horisontalOffset = archerAnimation.HorisontalOffset,
                verticalOffset = archerAnimation.VerticalOffset,
                frameTimer = 0,
                frameHeight = archerAnimation.FrameHeigth,
                frameWidth = archerAnimation.FrameWidth
            });

            entityManager.SetComponentData(entity, new Scale()
            {
                Value = 1f
            });

            entityManager.SetComponentData(entity, new ScaleByPositionComponentData()
            {
                minScale = 0.2f,
                maxScale = 1f
            });

            
        }

        entities.Dispose();
    }

    Mesh CreateMeshFor(Sprite sprite)
    {
        return new Mesh()
        {
            vertices = sprite.vertices.Select(v => (Vector3)v).ToArray(),
            uv = sprite.uv,
            triangles = sprite.triangles.Select(t => (int)t).ToArray()
        };
    }

    Mesh CreateMeshFor(float width, float height)
    {
        var halfHeight = (float)height / 2;
        var halfWidth = (float)width / 2;

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

    Texture CreateTextureForSprite(Sprite sprite)
    {
        var croppedTexture = new Texture2D((int)sprite.textureRect.width, (int)sprite.textureRect.height);
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

    Material CreateMaterialForTexture(Material defaultMaterial, Texture texture)
    {
        return new Material(defaultMaterial) { mainTexture = texture };
    }
}
