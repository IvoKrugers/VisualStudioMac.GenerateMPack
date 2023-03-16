using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Addins;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Desktop;

namespace VisualStudioMac.GenerateMPack.Helpers
{
    public static class ConsoleHelper
    {

        static PlatformService _platformService;

        public static void CleanUp()
        {
            _platformService = null;
        }

        public static async Task<ProcessAsyncOperation> StartProcessAsync(string command, string arguments, string workingDirectory, IDictionary<string, string> environmentVariables, string title, bool pauseWhenFinished)
        {
            if (_platformService is null)
            {
                object[] platforms = AddinManager.GetExtensionObjects("/MonoDevelop/Core/PlatformService");
                if (platforms.Length > 0)
                    _platformService = (PlatformService)platforms[0];
                else
                {
                    _platformService = new DefaultPlatformService();
                    System.Diagnostics.Debug.Print("A platform service implementation has not been found.");
                }
                _platformService.Initialize();
            }

            if (_platformService.CanOpenTerminal)
            {
                _platformService.LoadNativeToolkit();

                //    Runtime.ProcessService.SetExternalConsoleHandler(platformService.StartConsoleProcess);
                var result = _platformService.StartConsoleProcess(command, arguments, workingDirectory, environmentVariables, title, pauseWhenFinished);
                
                await result.Task;
                //return new ProcessAsyncOperation { ExitCode = result.ExitCode };
                return result;
            }

            //return new ProcessAsyncOperation();
            return new ProcessAsyncOperation(Task.CompletedTask, null);
        }
    }
}