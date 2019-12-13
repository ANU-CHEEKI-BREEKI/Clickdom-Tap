using UnityEngine;

public abstract class ASkillTargetPositionProvider : MonoBehaviour
{
    /// <summary>
    /// get new position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="maxCnt"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public abstract Vector3 GetTargetPosition(Vector3 position, int maxCnt, int index, float scale = 1);
}