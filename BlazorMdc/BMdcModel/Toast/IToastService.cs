﻿using System;

namespace BMdcModel
{
    /// <summary>
    /// Interface for the BlazorMdc toast service, developed from the code base of Blazored Toast by Chris Sainty.
    /// Works in conjunction with a <see cref="PMdcToastAnchor"/> that must be placed in either App.razor or
    /// MainLayout.razor to avoid an exception being thrown when you first attempt to show a toast notification.
    /// 
    /// <para>
    /// Throws a <see cref="System.InvalidOperationException"/> if
    /// <see cref="ShowToast(eToastLevel, string, string, eToastCloseMethod?, string, string, IIconFoundry?, bool?, uint?)"/>
    /// is called without a <see cref="PMdcToastAnchor"/> component used in the app.
    /// </para>
    /// <example>
    /// <para>You can optionally add configuration when you add this to the service collection:</para>
    /// <code>
    /// services.AddPMdcToastService(new PMdcToastServiceConfiguration()
    /// {
    ///     Postion = PMdcToastPosition.TopRight,
    ///     CloseMethod = PMdcToastCloseMethod.Timeout,
    ///     ... etc
    /// });
    /// </code>
    /// </example>
    /// </summary>
    public interface IToastService
    {
        /// <summary>
        /// Toast service configuration
        /// </summary>
        public ToastServiceConfiguration Configuration { get; set; }

        /// <summary>
        /// A event that will be invoked when showing a toast
        /// </summary>
        internal event Action<eToastLevel, ToastSettings> OnAdd;

        /// <summary>
        /// Shows a toast using the supplied settings
        /// </summary>
        /// <param name="closeMethod">close method</param>
        /// <param name="cssClass">additional css applied to toast</param>
        /// <param name="heading">Text used in the heading of the toast</param>
        /// <param name="icon">Icon name</param>
        /// <param name="level">Severity of the toast (info, error, etc)</param>
        /// <param name="message">Body text in the toast</param>
        /// <param name="showIcon">Show or hide icon</param>
        /// <param name="timeout">Length of time before autodismiss</param>
#nullable enable annotations
        void ShowToast(
            eToastLevel level,
            string message,
            string heading = null,
            eToastCloseMethod? closeMethod = null,
            string cssClass = null,
            string iconName = null,
            IIconFoundry? iconFoundry = null,
            bool? showIcon = null,
            uint? timeout = null);
#nullable restore annotations
    }
}
