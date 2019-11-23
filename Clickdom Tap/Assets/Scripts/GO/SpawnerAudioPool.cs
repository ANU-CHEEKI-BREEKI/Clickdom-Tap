using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioPlayer), typeof(ASpawner))]
public class SpawnerAudioPool : MonoBehaviour
{
    private AudioPlayer player;
    private ASpawner spawner;

    private static Dictionary<int, AudioPlayer> pools = new Dictionary<int, AudioPlayer>();

    private void Awake()
    {
        player = GetComponent<AudioPlayer>();
        spawner = GetComponent<ASpawner>();

        if (!pools.ContainsKey(spawner.SquadId))
            pools.Add(spawner.SquadId, player);
    }

    public static void Play(int squadId, int clipId, Vector3 position)
    {
        if (pools == null || !pools.ContainsKey(squadId))
            return;

        var player = pools[squadId];

        player.PlayClipAtPos(clipId, position);
    }
}
