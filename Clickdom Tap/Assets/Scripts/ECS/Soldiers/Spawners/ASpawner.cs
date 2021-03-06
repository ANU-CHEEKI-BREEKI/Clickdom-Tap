﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public abstract class ASpawner : MonoBehaviour, ICountSettable, IFrequencySettable, ISpeedSettable
{
    [SerializeField] protected float spawnRandRange = 0.2f;
    [SerializeField] protected float velocity = 2;
    [Space]
    [SerializeField] protected ScaleByPositionSettings scaleByPosSettings;
    [SerializeField] protected ZByYSettings ZbyYSettings;
    [Space]
    [SerializeField] protected bool flipHorisontalScale = false;
    [Space]
    [SerializeField] protected AnimationList animationProvider;
    [Space]
    [SerializeField] protected int actionSoundClipId;
    [SerializeField] protected bool randClipId = false;
    [SerializeField] protected int actionSoundMaxClipId;
    [Space]
    [SerializeField] protected int deathSoundClipId;
    [SerializeField] protected bool deathRandClipId = false;
    [SerializeField] protected int deathSoundMaxClipId;
    [Header("cast shadows")]
    [Tooltip("duplicate sprite and display it as shadow")]
    [SerializeField] public bool castSpriteShadows = false;
    [SerializeField] protected bool shiftShadows = false;
    [Header("shared shadow settings")]
    [SerializeField] protected ShadowSettings shadowSettings;
    [Header("default shadow settings")]
    [SerializeField] protected bool useLocalDefauldShadowSetting = false;
    [SerializeField] private CastSpritesShadowComponentData shadowData = new CastSpritesShadowComponentData()
    {
        color = Color.black,
        positionPercentOffset = new float3(0, -0.65f, 0),
        scale = new float2(1, -0.581f)
    };
    [Header("update pause for animations")]
    [SerializeField] private AnimationType[] forAnimation = new AnimationType[1];
    [Space]
    
    [SerializeField] protected int squadId = 0;
    [SerializeField] protected Transform squadPosition;
    [SerializeField] protected SquadData squadData;
    [SerializeField] protected FactionComponentData.Faction faction;
    [Space]
    [SerializeField] private float health = 1;
    [Space]
    [SerializeField] private Color tint = Color.white;
    [Space]
    [SerializeField] private bool updateAnimationStatesBuTriggers = false;


    private EntityArchetype archetype;
    private EntityManager manager;

    private SquadTagSharedComponentData squadTag;
    private RenderSharedComponentData renderData;
    private AnimationListSharedComponentData animationData;

    private float timer = 0;
    [Space]
    [Header("spawn rate in units per second")]
    [SerializeField] [Range(0.001f, 60)] private float spavnFrequency = 1;
    [SerializeField] private int maxEntityCoun = 0;
    
    public int SquadId => squadId;
    public AnimationListSharedComponentData AnimationData => animationData;

    [ContextMenu("SetUniqueSquadId")]
    private void SetUniqueSquadId()
    {
        var spawners = FindObjectsOfType<ASpawner>();

        if(spawners == null || spawners.Length == 0)
        {
            squadId = 1;
            return;
        }

        var ids = spawners.Select(s=>s.squadId);
        var max = ids.OrderBy(id => id).Last() + 1;
        for (int i = 1; i <= max; i++)
        {
            if(!ids.Contains(i))
            {
                squadId = i;
                return;
            }
        }
    }

    protected virtual void Start()
    {
        manager = World.Active.EntityManager;
        archetype = manager.CreateArchetype(
            typeof(Translation),
            typeof(LinearMovementComponentData),
            typeof(VelocityAbsoluteComponentData),
            typeof(SpriteSheetAnimationComponentData),
            typeof(SpriteRendererComponentData),
            typeof(SpriteTintComponentData),
            typeof(Scale),
            typeof(RenderScaleComponentdata),
            typeof(ScaleByPositionComponentData),
            typeof(SquadTagSharedComponentData),
            typeof(SquadComponentData),
            typeof(RenderSharedComponentData),
            typeof(FactionComponentData),
            typeof(CacheByQuadrandSystemTag),
            typeof(ProcessProjectileCollisionTag),
            typeof(AnimationListSharedComponentData),
            typeof(AnimationPauseComponentData),
            typeof(ActionOnAnimationFrameComponentData),
            typeof(DestroyWithHealthComponentData),
            typeof(HealthComponentData),
            typeof(AnimatorStatesComponentData), 
            typeof(FlibHorisontalByMoveDirTagComponentData),
            typeof(FlibHorisontalByTargetTagComponentData),
            typeof(ZbyYComponentData),
            typeof(AudioClipComponentData), 
            typeof(DeathAudioClipComponentData),
            typeof(CastSpritesShadowComponentData)
        );

        squadTag = DataToComponentData.ToComponentData(squadData, squadId, squadPosition.position);
        renderData = new RenderSharedComponentData()
        {
            material = animationProvider.Material,
            mesh = animationProvider.Mesh,
        };
        animationData = new AnimationListSharedComponentData()
        {
            animations = animationProvider.Animations,
            pauses = animationProvider.PausesData,
            actions = animationProvider.ActionsData
        };
    }

    protected void Update()
    {
        var cnt = squadTag.unitCount == null ? 0 : squadTag.unitCount.value;
        if (cnt < maxEntityCoun)
        {
            var delay = spavnFrequency == 0 ? 1 : 1 / spavnFrequency;
            timer += Time.deltaTime;
            while (timer > delay && cnt < maxEntityCoun)
            {
                timer -= delay;
                Spawn(1);
                cnt++;
            }
        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (squadPosition != null)
        {
            Gizmos.DrawLine(transform.position, squadPosition.position);
            Gizmos.DrawWireSphere(squadPosition.position, 0.2f);

            if (squadData != null)
            {
                float radius = 5;
                int horisonlalFlip = squadData.Data.directionLeftToRight ? 1 : -1;
                int verticalFlip = squadData.Data.directionBottomToTop ? 1 : -1;

                var pos = new Vector3(
                        squadPosition.position.x + Mathf.Cos((squadData.Data.rotationDegrees + 90) * Mathf.Deg2Rad) * radius * horisonlalFlip,
                        squadPosition.position.y + Mathf.Sin((squadData.Data.rotationDegrees + 90) * Mathf.Deg2Rad) * radius * verticalFlip,
                        squadPosition.position.z
                );

                Gizmos.DrawLine(squadPosition.position, pos);
            }
        }
    }
#endif

    public void SpawnImmidiate()
    {

        if (squadTag.unitCount != null)
        {
            var cnt = maxEntityCoun - squadTag.unitCount.value;
            Spawn(cnt);
        }
    }

    public void Spawn(int entityCount)
    {
        var entities = new NativeArray<Entity>(entityCount, Allocator.Temp);
        manager.CreateEntity(archetype, entities);
        
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            SetEntityComponentsData(entity, manager);
            SetEntitySharedComponentsData(entity, manager);
            EndInitEntityData(entity, manager);
#if UNITY_EDITOR
            manager.SetName(entity, GenerateEntityName());
#endif
        }

        entities.Dispose();
    }

    protected virtual void SetEntityComponentsData(Entity entity, EntityManager manager)
    {
        var translation = this.transform.position;
        manager.SetComponentData(entity, new Translation()
        {
            Value = new float3(
                    translation.x + UnityEngine.Random.Range(-spawnRandRange, spawnRandRange),
                    translation.y + UnityEngine.Random.Range(-spawnRandRange, spawnRandRange),
                    0f
                )
        });
        manager.SetComponentData(entity, new VelocityAbsoluteComponentData()
        {
            value = velocity
        });
        manager.SetComponentData(entity, new Scale()
        {
            Value = 1f
        });
        manager.SetComponentData(entity, new RenderScaleComponentdata()
        {
            value = new float2(flipHorisontalScale ? -1 : 1, 1)
        });
        manager.SetComponentData(entity, DataToComponentData.ToComponentData(scaleByPosSettings));
        manager.SetComponentData(entity, new FactionComponentData() { value = faction });
        manager.SetComponentData(entity, new LinearMovementComponentData()
        {
            doMoving = true
        });
        manager.SetComponentData(entity, new HealthComponentData()
        {
            value = health
        });
        manager.SetComponentData(entity, new DestroyWithHealthComponentData()
        {
            delay = 1f
        });
        manager.SetComponentData(entity, new SpriteTintComponentData()
        {
            color = tint
        });
        manager.SetComponentData(entity, DataToComponentData.ToComponentData(ZbyYSettings));
        manager.SetComponentData(entity, new SpriteRendererComponentData()
        {
            usePivot = true,
            pivot = new Vector2(0.5f, 0)
        });
        manager.SetComponentData(entity, new FlibHorisontalByMoveDirTagComponentData()
        {
            defaultFlipped = flipHorisontalScale
        });
        manager.SetComponentData(entity, new FlibHorisontalByTargetTagComponentData()
        {
            defaultFlipped = flipHorisontalScale
        });
        if (updateAnimationStatesBuTriggers)
            manager.AddComponent<AnimatorStateLastTriggeredAnimationComponentData>(entity);
        CastSpritesShadowComponentData shd;
        if (!useLocalDefauldShadowSetting && shadowSettings != null)
            shd = DataToComponentData.ToComponentData(shadowSettings);
        else
            shd = shadowData;
        shd.disableCastShadow = !castSpriteShadows;
        manager.SetComponentData(entity, shd);

        if (shiftShadows)
        {
            manager.AddComponentData(entity, new ShiftCastShadowsTagComponentData()
            {
                positionUnitsOffsetDefaultValue = shd.positionUnitsOffset
            });
        }

        manager.SetComponentData(entity, new AudioClipComponentData()
        {
            audioSourcePoolId = squadId,
            randRangeId = randClipId,
            maxClipId = actionSoundMaxClipId,
            clipId = actionSoundClipId
        });

        manager.SetComponentData(entity, new DeathAudioClipComponentData()
        {
            audio = new AudioClipComponentData()
            {
                audioSourcePoolId = squadId,
                randRangeId = deathRandClipId,
                maxClipId = deathSoundMaxClipId,
                clipId = deathSoundClipId
            }
        });
    }

    protected virtual void SetEntitySharedComponentsData(Entity entity, EntityManager manager)
    {
        manager.SetSharedComponentData(entity, squadTag);
        manager.SetSharedComponentData(entity, renderData);
        manager.SetSharedComponentData(entity, animationData);
    }

    protected virtual void EndInitEntityData(Entity entity, EntityManager manager)
    {
        AnimationSetterUtil.SetAnimation(manager, entity, AnimationType.IDLE, 4);
    }
    
    protected virtual string GenerateEntityName()
    {
        return "Soldier";
    }

    public void SetMaxEntityCount(int newCount)
    {
        maxEntityCoun = newCount;
    }

    public void IncreaceMaxEntityCount(int addCount)
    {
        maxEntityCoun += addCount;
    }

    public void SetMaxEntityCount(float newCount)
    {
        SetMaxEntityCount((int)newCount);
    }

    public void IncreaceMaxEntityCount(float addCount)
    {
        IncreaceMaxEntityCount((int)addCount);
    }

    public void SetSpawnFrequency(float newFrequency)
    {
        spavnFrequency = newFrequency;
    }

    public void IncreaceSpawnFrequency(float addFrequency)
    {
        spavnFrequency += addFrequency;
    }

    #region adapter
    void ICountSettable.SetCount(float count)
    {
        SetMaxEntityCount(count);
    }

    void IFrequencySettable.SetFrequency(float frequency)
    {
        SetSpawnFrequency(frequency);
    }

    public void SetSpeed(float speed)
    {
        //скорость атаки
        //в ударах в минуту

        var pauseDuration = 1f;
        if (speed != 0)
            pauseDuration = 60f / speed;

        foreach (var animation in forAnimation)
            animationData.pauses[(int)animation].value.pauseDuration = pauseDuration;
    }
    #endregion
}