using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models
{
    /// <summary>
    /// Base class for the 2 models used in the MultiplePrefabsExample scene. Contains a title as the only data field and some other fields for the view state
    /// </summary>
    public abstract class BaseModel
    {
        #region Data Fields
        /// <summary>Common data field for all derived models</summary>
        public string title;
        #endregion

        #region View State
        /// <summary>The actual displayed size. It's related to the visual state and not a data field per-se, but the gains in performance are huge if it's declared here, compared to being managed in a separate array or class</summary>
        [NonSerialized] public float visualSize;
        /// <summary>Assigned in the constructor. Also related to the visual state, like <see cref="visualSize"/> (see it for more details about why the view state is to be stored in the model)</summary>
        public Type cachedType { get; private set; }
        #endregion


        public BaseModel()
        { cachedType = GetType(); }
    }
}
