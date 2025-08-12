using nadena.dev.ndmf.preview;
using System;
using System.Collections.Immutable;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    public class NDMFUtils
    {
        private ComputeContext _computeContext;
        private ImmutableList<GameObject> _avatars;

        public NDMFUtils(ComputeContext computeContext)
        {
            _computeContext = computeContext;
            _avatars = computeContext.GetAvatarRoots();
        }




        private ImmutableList<GameObject> GetAvatars()
            => _avatars;
    }
}
