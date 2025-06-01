using GameplayEntities;
using HarmonyLib;
using LLHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperRandom
{
    [HarmonyPatch]
    internal class LoadBundles
    {
        private class SimpleEnumerator : IEnumerable
        {
            public IEnumerator enumerator;
            public Action prefixAction, postfixAction;

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator GetEnumerator()
            {
                prefixAction();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                yield return Characters(null);
                postfixAction();
            }
        }

        private static IEnumerator Characters(object item)
        {
            List<Character> charactersToLoad = [];
            for (int i = 0; i < 4; i++)
            {
                List<Character> characterList = SuperRandomTweak.randomCharacters[i];
                if (characterList == null || characterList.Count == 0)
                    continue;

                foreach (Character character in characterList)
                {
                    if (charactersToLoad.Contains(character))
                        continue;

                    charactersToLoad.Add(character);
                    SuperRandom.Log.LogWarning($"[{i}] PreLoad '{character} Bundle");
                    yield return BundleHandler.CLoadBundle(new Bundle(BundleType.CHARACTER_GAME, character), BundleLoadType.INCL_ASSETS, null);
                }
            }
        }
        [HarmonyPatch(typeof(OGONAGCFDPK), nameof(OGONAGCFDPK.DLIEBHKPBGP))]
        [HarmonyPostfix]
        private static void BundlePatcher(ref IEnumerator __result)
        {
            Action prefixAction = () => { SuperRandom.Log.LogWarning("--> beginning"); };
            Action postfixAction = () => { SuperRandom.Log.LogWarning("--> ending"); };

            var myEnumerator = new SimpleEnumerator()
            {
                enumerator = __result,
                prefixAction = prefixAction,
                postfixAction = postfixAction,
            };
            __result = myEnumerator.GetEnumerator();
        }
    }
}
