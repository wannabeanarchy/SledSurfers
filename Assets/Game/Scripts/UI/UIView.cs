using UnityEngine;

namespace SledSurfers.UI
{
    public enum ViewType { Window, Popup, Widget }

    public abstract class UIView : MonoBehaviour
    {
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
