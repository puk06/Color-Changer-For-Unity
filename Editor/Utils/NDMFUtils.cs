using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    internal static class NDMFUtils
    {
        /// <summary>
        /// 与えられたオブジェクトの元のオブジェクトを取得します。
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        internal static ObjectReference GetReference(Object texture)
            => ObjectRegistry.GetReference(texture);

        /// <summary>
        /// ObjectRegistryにDictionaryのKeyとValueを登録します。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="processed"></param>
        internal static void RegisterReplacements<TKey, TValue>(Dictionary<TKey, TValue> processed)
            where TKey : Object
            where TValue : Object
        {
            foreach (var kv in processed)
            {
                ObjectRegistry.RegisterReplacedObject(kv.Key, kv.Value);
            }
        }
    }
}
