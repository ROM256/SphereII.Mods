﻿using Audio;
using Harmony;
using System;
using UnityEngine;

// This class populates a static variable that will help us link Sound Data with Buff / Quests.
public class SphereII_GiveBuffOrQuestBySound
{
    private static string AdvFeatureClass = "AdvancedSoundFeatures";
    public static void CheckForBuffOrQuest( string soundGroupName, Vector3 position)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Searching for " + soundGroupName);

        if(SoundDataNodeClassSDX.SoundDataSDXInfo.ContainsKey(soundGroupName))
        {
            AdvLogging.DisplayLog(AdvFeatureClass, "Found Sound Node. Checking for buffs");

            // use xmlData to grab the sound node information, which can contain how far away the sound can be heard.
            XmlData xmlData;
            if(!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
                return;

            int Radius = Utils.Fastfloor(xmlData.distantFadeEnd);
            SoundDataNodeClassSDX.SoundDataSDX data = SoundDataNodeClassSDX.SoundDataSDXInfo[soundGroupName];

            if(data.Buff != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, ": Found Buff for Sound Node: " + data.Buff);
                EntityUtilities.AddBuffToRadius(data.Buff, position, Radius);
            }
            AdvLogging.DisplayLog(AdvFeatureClass, "Scanning for quest");
            if(data.Quest != null)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, "Adding Quest " + data.Quest + " to surrounding entities");
                EntityUtilities.AddQuestToRadius(data.Quest, position, Radius);
            }
        }
        return ;
    }
}

[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(int) })]
public class SphereII_Audio_Manager_Play
{
    static bool Prefix(Audio.Manager __instance, Vector3 position, string soundGroupName)
    {
        XmlData xmlData;
        if(!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
            return true;

        if(xmlData.Update())
            SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGroupName,position);
        return true;
    }
}



[HarmonyPatch(typeof(Audio.Manager))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Entity), typeof(string), typeof(float), typeof(bool) })]
public class SphereII_Audio_Server_Play
{
    static bool Prefix(Audio.Manager __instance, Entity entity, string soundGroupName)
    {
        if(entity == null)
            return true;

        XmlData xmlData;
        if(!Manager.audioData.TryGetValue(soundGroupName, out xmlData))
            return true;

        if(xmlData.Update())
            SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest( soundGroupName, entity.position);
        return true;
    }
}

[HarmonyPatch(typeof(Audio.Client))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(int), typeof(string), typeof(float) })]
public class SphereII_Audio_Client_Play_1
{
    static bool Prefix(int playOnEntityId, string soundGoupName, float _occlusion)
    {
        EntityAlive myEntity = GameManager.Instance.World.GetEntity(playOnEntityId) as EntityAlive;
        if (myEntity == null)
            return true;

        XmlData xmlData;
        if (!Manager.audioData.TryGetValue(soundGoupName, out xmlData))
            return true;

        if (xmlData.Update())
            SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGoupName, myEntity.position);
        return true;
    }
}


[HarmonyPatch(typeof(Audio.Client))]
[HarmonyPatch("Play")]
[HarmonyPatch(new Type[] { typeof(Vector3), typeof(string), typeof(float), typeof(int) })]
public class SphereII_Audio_Client_Play_2
{
    static bool Prefix( Vector3 position, string soundGoupName)
    {
        XmlData xmlData;
        if (!Manager.audioData.TryGetValue(soundGoupName, out xmlData))
            return true;

        if (xmlData.Update())
            SphereII_GiveBuffOrQuestBySound.CheckForBuffOrQuest(soundGoupName, position);
        return true;
    }
}