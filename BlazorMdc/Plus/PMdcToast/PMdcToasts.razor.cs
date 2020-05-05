﻿//
//  2020-04-10  Mark Stega
//              Created from github.com/ChrisSainty/Blazored.Toast by Chris Sainty
//

using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlazorMdc
{
    public partial class PMdcToasts
    {
        [Inject] private IPmdcToastService ToastService { get; set; }

        //[Parameter] public string InfoClass { get; set; }
        //[Parameter] public string InfoIcon { get; set; }
        //[Parameter] public string SuccessClass { get; set; }
        //[Parameter] public string SuccessIcon { get; set; }
        //[Parameter] public string WarningClass { get; set; }
        //[Parameter] public string WarningIcon { get; set; }
        //[Parameter] public string ErrorClass { get; set; }
        //[Parameter] public string ErrorIcon { get; set; }
        //[Parameter] public PMdcToastPosition Position { get; set; } = PMdcToastPosition.TopRight;
        //[Parameter] public int Timeout { get; set; } = 5;


        internal List<ToastInstance> ToastList { get; set; } = new List<ToastInstance>();
        

        private string positionClass => $"bmdc-toast__{ToastService.Configuration.Position.ToString().ToLower()}";
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);


        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
        }


        private PMdcToastSettings BuildToastSettings(PMdcToastServiceConfiguration configuration, PMdcToastLevel level, RenderFragment message, string heading)
        {
            return level switch
            {
                PMdcToastLevel.Error => new PMdcToastSettings(string.IsNullOrWhiteSpace(heading) ? configuration.ErrorDefaultHeading : heading, message, "bmdc-toast__error", configuration.ErrorIcon),
                PMdcToastLevel.Info => new PMdcToastSettings(string.IsNullOrWhiteSpace(heading) ? configuration.InfoDefaultHeading : heading, message, "bmdc-toast__info", configuration.InfoIcon),
                PMdcToastLevel.Success => new PMdcToastSettings(string.IsNullOrWhiteSpace(heading) ? configuration.SuccessDefaultHeading : heading, message, "bmdc-toast__success", configuration.SuccessIcon),
                PMdcToastLevel.Warning => new PMdcToastSettings(string.IsNullOrWhiteSpace(heading) ? configuration.WarningDefaultHeading : heading, message, "bmdc-toast__warning", configuration.WarningIcon),
                _ => throw new InvalidOperationException(),
            };
        }


        private void ShowToast(PMdcToastServiceConfiguration configuration, PMdcToastLevel level, RenderFragment message, string heading)
        {
            InvokeAsync(async () =>
            {
                var settings = BuildToastSettings(configuration, level, message, heading);
                var toastInstance = new ToastInstance
                {
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.Now,
                    Settings = settings
                };

                await semaphoreSlim.WaitAsync();

                try
                {
                    ToastList.Add(toastInstance);
                }
                finally
                {
                    semaphoreSlim.Release();
                }

                var timeout = configuration.Timeout;
                var toastTimer = new System.Timers.Timer(timeout);
                toastTimer.Elapsed += (sender, args) => { CloseToast(toastInstance.Id); };
                toastTimer.AutoReset = false;
                toastTimer.Start();

                StateHasChanged();
            });

        }


        public void CloseToast(Guid toastId)
        {
            InvokeAsync(async () =>
            {
                
                await semaphoreSlim.WaitAsync();

                try
                {
                    var toastInstance = ToastList.SingleOrDefault(x => x.Id == toastId);

                    if (toastInstance is null)
                    {
                        return;
                    }

                    toastInstance.Settings.Status = ToastStatus.FadeOut;
                    StateHasChanged();
                }
                finally
                {
                    semaphoreSlim.Release();
                }

                var toastTimer = new System.Timers.Timer(500);
                toastTimer.Elapsed += (sender, args) => { RemoveToast(toastId); };
                toastTimer.AutoReset = false;
                toastTimer.Start();

                StateHasChanged();
            });
        }


        public void RemoveToast(Guid toastId)
        {
            InvokeAsync(async () =>
            {
                await semaphoreSlim.WaitAsync();

                try
                {
                    var toastInstance = ToastList.SingleOrDefault(x => x.Id == toastId);
                    
                    if (toastInstance is null)
                    {
                        return;
                    }

                    toastInstance.Settings.Status = ToastStatus.Hide;

                    if (ToastList.Where(x => x.Settings.Status == ToastStatus.FadeOut).Count() == 0)
                    {
                        ToastList.RemoveAll(x => x.Settings.Status == ToastStatus.Hide);
                    }

                    StateHasChanged();
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            });
        }
    }
}
