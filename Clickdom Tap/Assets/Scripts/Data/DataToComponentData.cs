using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public static class DataToComponentData
{
    public static SpriteSheetAnimationComponentData ToComponentData(ShaderSpriteUvAnimationSetupData animationData)
    {
        return new SpriteSheetAnimationComponentData()
        {
            currentFrame = animationData.RamdomInitFrame,
            frameCount = animationData.FramesCount,
            frameDuration = animationData.FrameDuration,
            horisontalOffset = animationData.HorisontalOffset,
            verticalOffset = animationData.VerticalOffset,
            out_frameTimer = 0,
            frameHeight = animationData.FrameHeigth,
            frameWidth = animationData.FrameWidth,
        };
    }
    
    public static SquadTagSharedComponentData ToComponentData(SquadData squadData, int squadId, Vector3 formationCenter)
    {
        var res = new SquadTagSharedComponentData()
        {
            data = squadData.Data,
            id = new SquadTagSharedComponentData.RefInt() { value = squadId },
            unitCount = new SquadTagSharedComponentData.RefInt() { value = 0 }
        };
        res.data.formationCenter = new float2(formationCenter.x, formationCenter.y);
        return res;
    }
}
