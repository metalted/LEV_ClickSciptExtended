using UnityEngine;

namespace LEV_ClickSciptExtended
{
    public class LEV_ClickScriptXObserver : MonoBehaviour
    {
        public void OnDestroy()
        {
            LEV_ClickScriptX.ObserverDestroyed();
        }
    }
}
