using BepInEx.Logging;
using LLBML.GameEvents;
using LLBML.Messages;
using LLBML.Players;
using Multiplayer;
using System.Collections.Generic;
using System.IO;

namespace SuperRandom
{
    internal class Networking
    {
        enum SuperRandomMessages
        {
            SENDCHARACTERLIST = 54321,


        }
        public static void Init()
        {
            MessageApi.RegisterCustomMessage(SuperRandom.Instance.Info, (ushort)SuperRandomMessages.SENDCHARACTERLIST, SuperRandomMessages.SENDCHARACTERLIST.ToString(), RecieveCharacterList);
            LobbyEvents.OnStageSelectOpen += LobbyEvents_OnStageSelectOpen;
        }
        private static void GetRandomList()
        {
            if (SuperRandomTweak.sRToggled == true)
            {

                SuperRandomTweak.SetNumberOfStocks();
                Player.ForAllInMatch((Player player) =>
                {
                    if (player.nr == -1)
                    {
                        player.nr = 0;
                    }
                    SuperRandomTweak.randomCharacters[player.nr] = SuperRandomTweak.GetRandomChars(player.nr);
                });
                SuperRandomTweak.Logger.LogInfo("randomCharacters has been set");
                foreach(var character in SuperRandomTweak.randomCharacters)
                {
                    SuperRandomTweak.Logger.LogInfo($"{character}");

                }

            }

        }

        private static void LobbyEvents_OnStageSelectOpen(HDLIJDBFGKN source, OnStageSelectOpenArgs e)
        {

            GetRandomList();

            Player.ForAllInMatch((Player player) =>
            {
                if (SuperRandomTweak.randomCharacters[player.nr] == null)
                {
                    SuperRandomTweak.Logger.LogInfo($"{player.nr} list is null");
                    return;
                }
                if (SuperRandomTweak.randomCharacters[player.nr].Count == 0)
                {
                    SuperRandomTweak.Logger.LogInfo($"{player.nr} list is empty");
                    return;
                }
                if (player.peer != null)
                {
                    if (player.IsLocalPeer == true)
                    {
                        SuperRandomTweak.Logger.LogInfo($"{player.nr} getting ready to send");
                        SendCharacterList(SuperRandomTweak.randomCharacters[player.nr]);

                    }
                }

            });
        }

        public static void SendCharacterList(List<Character> characterList)
        {
            SuperRandomTweak.Logger.LogInfo("Sending CharacterList");
            byte[] array;
            using (MemoryStream ms = new())
            using (BinaryWriter bw = new(ms))
            {
                foreach (Character character in characterList)
                {
                    bw.Write((int)character);
                }
                array = ms.ToArray();
            }
            P2P.SendOthers(new Message((Msg)SuperRandomMessages.SENDCHARACTERLIST, P2P.localPeer.playerNr, -1, array, -1));
        }



        public static void RecieveCharacterList(Message msg)
        {
            SuperRandomTweak.Logger.LogInfo("Retrieving CharacterList");
            byte[] array = (byte[])msg.ob;
            List<Character> characterList = new List<Character>();
            using (MemoryStream ms = new(array))
            using (BinaryReader br = new(ms))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    characterList.Add((Character)br.ReadInt32());
                }
            }
            foreach(Character character in characterList)
            {
                SuperRandomTweak.Logger.LogInfo($"{character} in {msg.playerNr} list");
            }
            SuperRandomTweak.randomCharacters[msg.playerNr] = characterList;
        }
    }
}
