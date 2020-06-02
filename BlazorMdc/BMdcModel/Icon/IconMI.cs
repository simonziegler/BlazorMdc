﻿using System.Collections.Generic;

namespace BMdcModel
{
    /// <summary>
    /// Material Icons icon.
    /// </summary>
    internal class IconMI : IIcon
    {
        /// <inheritdoc />
        public string Class
        {
            get
            {
                return "material-icons" + Theme switch
                {
                    eIconMITheme.Filled => "",
                    eIconMITheme.Outlined => "-outlined",
                    eIconMITheme.Round => "-round",
                    eIconMITheme.TwoTone => "-two-tone",
                    eIconMITheme.Sharp => "-sharp",
                    _ => throw new System.NotImplementedException(),
                };
            }
        }


        /// <inheritdoc />
        public string Text => IconName.ToLower();

        
        private readonly Dictionary<string, object> _attributes = new Dictionary<string, object>();
        /// <inheritdoc />
        public IDictionary<string, object> Attributes => _attributes;


        /// <inheritdoc />
        public string IconName { get; }


        /// <inheritdoc />
        public bool RequiresColorFilter => Theme == eIconMITheme.TwoTone;


        /// <summary>
        /// The Material Icons theme.
        /// </summary>
        public eIconMITheme Theme { get; }


#nullable enable annotations
        public IconMI(CascadingDefaults cascadingDefaults, string iconName, IconFoundryMI? foundry = null)
        {
            IconName = iconName;
            Theme = cascadingDefaults.AppliedIconMITheme(foundry?.Theme);
        }
#nullable restore annotations
    }
}
