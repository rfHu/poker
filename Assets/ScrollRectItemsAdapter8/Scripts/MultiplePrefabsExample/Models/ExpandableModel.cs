﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models
{
    /// <summary>A model representing an expandable item, including an <see cref="expanded"/> view-state-related property to keep track of whether it's expanded or not. It also includes an <see cref="imageURL"/> to be loaded into the view</summary>
    public class ExpandableModel : BaseModel
    {
        #region Data Fields
        public string imageURL;
        #endregion

        #region View State
        /// <summary>It's very convenient to place this here instead of using an array of bools for all models</summary>
        [NonSerialized]
        public bool expanded;
        #endregion
    }
}
