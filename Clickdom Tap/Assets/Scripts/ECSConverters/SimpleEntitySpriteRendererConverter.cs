using UnityEngine;
using System.Collections;
using Unity.Entities;
using static SimpleEntityConverter;
using UnityEditor;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor.SceneManagement;

#endif

[RequireComponent(typeof(SimpleEntityConverter), typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class SimpleEntitySpriteRendererConverter : MonoBehaviour, ISimpleEntityConverter
{
    [SerializeField] private AfterConvertionEvent afterConvEvent = AfterConvertionEvent.NOTHING;
    [Space]
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;
    [Space]
    [SerializeField] private bool disableCulling = false;
    [Space]
    [ContextMenuItem(nameof(RecalcRenderScaleBySprite), nameof(RecalcRenderScaleBySprite))]
    [SerializeField] private Vector2 renderScale = Vector2.one;
    [SerializeField] private bool autoSet = true;
    [Space]
    [SerializeField] private bool allowCracks = false;

    private SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

#if UNITY_EDITOR

    private void Reset()
    {
        if (autoSet)
            RecalcRenderScale();
    }

    private void OnValidate()
    {
        if (autoSet)
            RecalcRenderScale();
    }

#endif

    public void ConvertToEntity(Entity entity, EntityManager manager)
    {
        if (mesh == null)
            throw new System.ArgumentNullException(nameof(mesh));

        manager.AddComponentData(entity, new SpriteRendererComponentData()
        {
            uv = ShaderSpriteUvAnimationSetupData.GetUvFor(_renderer.sprite, 1, 0),
            pivot = ShaderSpriteUvAnimationSetupData.GetPivotFor(_renderer.sprite),
            usePivot = true
        });       
        manager.AddComponentData(entity, new SpriteTintComponentData()
        {
            color = _renderer.color
        });
        manager.AddComponentData(entity, new RenderScaleComponentdata()
        {
            value = renderScale
        });
        manager.AddSharedComponentData(entity, new RenderSharedComponentData()
        {
            material = material == null ? _renderer.material : material,
            mesh = mesh
        });
        if (disableCulling)
            manager.AddComponent<DisableRenderCullingTagComponentData>(entity);
        
        if (afterConvEvent == AfterConvertionEvent.DELETE)
            Destroy(_renderer);
        else if (afterConvEvent == AfterConvertionEvent.DEACTIVATE)
            _renderer.enabled = false;

        if(allowCracks)
            manager.AddComponent<SpriteCracksComponentData>(entity);
    }

    private void RecalcRenderScale()
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        var rect = _renderer.sprite.rect;
        renderScale = _renderer.sprite.bounds.size;
    }

    [ContextMenu(nameof(RecalcRenderScaleBySprite))]
    public void RecalcRenderScaleBySprite()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, nameof(RecalcRenderScaleBySprite));
#endif

        RecalcRenderScale();

#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
    }



#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        DrawScaleRectGizmos();
    }

    private void DrawScaleRectGizmos()
    {
        var oldMatrix = Gizmos.matrix;

        var _renderer = GetComponent<SpriteRenderer>();
        var pivot = ShaderSpriteUvAnimationSetupData.GetPivotFor(_renderer.sprite);
        pivot -= Vector2.one / 2;

        var scale = renderScale * transform.lossyScale.Average2D();

        Vector2 pivotedPosition;
        pivotedPosition.x = -pivot.x * scale.x;
        pivotedPosition.y = -pivot.y * scale.y;

        var matrix = Matrix4x4.Translate(transform.position);
        matrix *= Matrix4x4.Rotate(transform.rotation);
        matrix *= Matrix4x4.Translate(pivotedPosition);
        matrix *= Matrix4x4.Scale(scale);

        Gizmos.matrix = matrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldMatrix;
    }

#endif
}
