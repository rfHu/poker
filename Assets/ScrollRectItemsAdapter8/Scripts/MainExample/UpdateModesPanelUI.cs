using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
    /// <summary>The behavior of the panel containing the 3 <see cref="BaseParams.UpdateMode"/>s</summary>
    public class UpdateModesPanelUI : MonoBehaviour
    {
        public ScrollRectItemsAdapterExample hor, vert;
        public Toggle a, b, c;

        void Start()
        {
            a.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    hor.Params.updateMode = vert.Params.updateMode = BaseParams.UpdateMode.ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE;
            });
            b.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    hor.Params.updateMode = vert.Params.updateMode = BaseParams.UpdateMode.ON_SCROLL;
            });
            c.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    hor.Params.updateMode = vert.Params.updateMode = BaseParams.UpdateMode.MONOBEHAVIOUR_UPDATE;
            });
        }
    }
}
