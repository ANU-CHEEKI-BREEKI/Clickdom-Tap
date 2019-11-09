using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

//[UpdateAfter(typeof(VolleySquadsCollectorSystem))]
public class VolleyAnimationPauserSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(
        typeof(SquadTagSharedComponentData), 
        typeof(VolleyAnimationPauseTagComponentData))]
    struct DisableResumerJob : IJobForEachWithEntity<AnimationPauseComponentData, AnimatorStatesComponentData, SpriteSheetAnimationComponentData>
    {
        public void Execute(Entity entity, int index, ref AnimationPauseComponentData pause, ref AnimatorStatesComponentData animator, ref SpriteSheetAnimationComponentData animation)
        {
            if (!animator.shooting)
                return;

            if ((animation.out_FrameChangedEventFlag && animation.currentFrame == 0) || animator.out_StateChangedEventFlag)
                pause.needResume = false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new DisableResumerJob
        {
        };
        return job.Schedule(this, inputDeps);
    }
}