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
    }
}
