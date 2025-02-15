using GameplayEntities;
using HarmonyLib;
using LLHandlers;
using UnityEngine;

namespace SuperRandomLLB
{
    internal class Patcher
    {
        [HarmonyPatch(typeof(PlayerHandler), nameof(PlayerHandler.AddPlayersToWorld))]
        public static class AddPlayersToWorldPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerHandler __instance)
            {
                for (int i = 3; i >= 0; i--)
                {
                    ALDOKEMAOMB player = ALDOKEMAOMB.BJDPHEHJJJK(i);
                    if (player.NGLDMOLLPLK)
                    {
                        Debug.Log($"[Patch] Checking Player {i}");




                        player.JCCIAMJEODH.SetPlayerState(PlayerState.STANDBY, string.Empty, HitPauseState.NONE, HitstunState.NONE);
                        Debug.Log($"[Patch] Ensured main entity for Player {i} is enabled.");


                        PlayerEntity extraEntity1 = CreateExtraEntity(__instance, player, Character.COP);
                        PlayerEntity extraEntity2 = CreateExtraEntity(__instance, player, Character.ROBOT);


                        extraEntity1.transform.position = player.JCCIAMJEODH.transform.position + new Vector3(2, 0, 0);
                        extraEntity2.transform.position = player.JCCIAMJEODH.transform.position + new Vector3(-2, 0, 0);


                        __instance.world.AddEntity(extraEntity1);
                        __instance.world.AddEntity(extraEntity2);

                        Debug.Log($"[Patch] Added extra entities for Player {i}");
                    }
                }
            }

            private static PlayerEntity CreateExtraEntity(PlayerHandler instance, ALDOKEMAOMB player, Character characterType)
            {
                GameObject gameObject = new GameObject();
                PlayerEntity playerEntity;

                switch (characterType)
                {
                    case Character.COP:
                        playerEntity = gameObject.AddComponent<CopPlayerModel>();
                        break;
                    case Character.ROBOT:
                        playerEntity = gameObject.AddComponent<RobotPlayerModel>();
                        break;
                    default:
                        playerEntity = gameObject.AddComponent<KidPlayerModel>();
                        break;
                }

                playerEntity.character = characterType;
                playerEntity.variant = player.AIINAIDBHJI;
                playerEntity.player = player;
                playerEntity.playerIndex = player.CJFLMDNNMIE;
                instance.playerHandlerData.playerData[player.CJFLMDNNMIE].team = player.HEOKEMBMDIJ;

                playerEntity.Init(instance.playerHandlerData.playerData[player.CJFLMDNNMIE]);
                playerEntity.tf.parent = instance.holder;
                gameObject.name = $"Extra_{characterType}_{playerEntity.playerIndex}";


                playerEntity.SetPlayerState(PlayerState.DISABLED, string.Empty, HitPauseState.NONE, HitstunState.NONE);

                Debug.Log($"[Patch] Created and disabled extra entity: {gameObject.name}");

                playerEntity.UpdateUnityTransform();

                return playerEntity;
            }
        }


    }
}





