﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BMdcPlus
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a BlazorMdc <see cref="IPMdcToastService"/> to the service collection to manage toast messages.
        /// <example>
        /// <para>You can optionally add configuration:</para>
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
        public static IServiceCollection AddBMdcPlusToastService(this IServiceCollection services, BMdcModel.ToastServiceConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new BMdcModel.ToastServiceConfiguration();
            }

            return services.AddScoped<IToastService, ToastService>(serviceProvider => new ToastService(configuration));
        }


        /// <summary>
        /// Adds a BlazorMdc <see cref="IAnimatedNavigationManager"/> to the service collection to apply
        /// fade out/in animation to Blazor page navigation.
        /// <example>
        /// <para>You can optionally add configuration:</para>
        /// <code>
        /// services.AddPMdcAnimatedNavigationManager(new PMdcAnimatedNaviationManagerConfiguration()
        /// {
        ///     ApplyAnimation = true,
        ///     AnimationTime = 300   /* milliseconds */
        /// });
        /// </code>
        /// </example>
        /// </summary>
        public static IServiceCollection AddBMdcPlusAnimatedNavigationManager(this IServiceCollection services, AnimatedNaviationManagerConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new AnimatedNaviationManagerConfiguration();
            }

            return services.AddScoped<IAnimatedNavigationManager, AnimatedNavigationManager>(serviceProvider => new AnimatedNavigationManager(serviceProvider.GetRequiredService<NavigationManager>(), configuration));
        }
    }
}
