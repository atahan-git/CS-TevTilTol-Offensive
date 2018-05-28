// vis2k
#if ENABLE_UNET

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/NetworkTransformChild")]
    public class NetworkTransformChild : NetworkTransformBase
    {
        public Transform target;
        protected override Component targetComponent { get { return target; } }
    }
}
#endif //ENABLE_UNET
