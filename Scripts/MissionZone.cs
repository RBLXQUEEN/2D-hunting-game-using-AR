using System;
using UnityEngine;

/// <summary>
/// Data class representing a geofenced mission area.
/// Assign instances of this in the MissionManager inspector list.
/// </summary>
[Serializable]
public class MissionZone
{
    [Tooltip("Display name shown to the player when they enter this zone.")]
    public string missionName;

    [Tooltip("GPS latitude of the zone center (decimal degrees).")]
    public float latitude;

    [Tooltip("GPS longitude of the zone center (decimal degrees).")]
    public float longitude;

    [Tooltip("Radius in meters. Player must be within this distance to trigger the mission.")]
    public float radiusInMeters = 50f;

    [Tooltip("Prefabs to spawn in AR when this mission is triggered.")]
    public GameObject[] charactersToSpawn;

    [Tooltip("Optional description shown in future UI elements.")]
    [TextArea(2, 4)]
    public string missionDescription;

    [Tooltip("Set to true once the player completes this mission. Prevents retriggering.")]
    public bool isCompleted;
}
