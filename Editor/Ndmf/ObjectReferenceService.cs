using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

namespace net.puk06.ColorChanger.Services
{
    internal static class ObjectReferenceService
    {
        internal static void RegisterReplacements<TKey, TValue>(Dictionary<TKey, TValue> objectDictionary)
            where TKey : Object
            where TValue : Object
        {
            foreach (KeyValuePair<TKey, TValue> objectKpv in objectDictionary)
            {
                ObjectRegistry.RegisterReplacedObject(objectKpv.Key, objectKpv.Value);
            }
        }
    }
}
