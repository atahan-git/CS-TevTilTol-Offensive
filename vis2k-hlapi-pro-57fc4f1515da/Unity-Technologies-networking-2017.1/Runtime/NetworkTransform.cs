// vis2k
#if ENABLE_UNET

namespace UnityEngine.Networking
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkTransform")]
    public class NetworkTransform : NetworkTransformBase
    {
        protected override Component targetComponent { get { return transform; } }
    }
}
#endif //ENABLE_UNET
