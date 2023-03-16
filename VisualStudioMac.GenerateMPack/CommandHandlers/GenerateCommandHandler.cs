using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core.Collections;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using NetworkExtension;
using PencilKit;
using VisualStudioMac.GenerateMPack.Helpers;

namespace VisualStudioMac.GenerateMPack.CommandHandlers
{
    public class GenerateCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Enabled = true;
        }

        protected override void Run()
        {
            // Do stuff
            _ = RunAsync();
        }

        private async Task RunAsync()
        {
            var vsmPath = "/Applications/Visual Studio.app/";
            var vsToolPath = vsmPath + "Contents/MacOS/vstool";

            var dllFilename = GetDllFullFilename();

            if (!string.IsNullOrEmpty(dllFilename))
            {
                var dir = IdeApp.ProjectOperations.CurrentSelectedProject.BaseDirectory.ParentDirectory.FullPath.ToString();

                // Remove existing *.mpack in solution folder
                var files = Directory.GetFiles(dir, "*.mpack");
                foreach (var file in files)
                {
                    File.Delete(file);
                }

                // Create mpack
                //var result = await ConsoleHelper.StartProcessAsync($"'/Applications/Visual Studio.app/Contents/MacOS/vstool' setup pack {dllFilename}", "", dir, new Dictionary<string, string>(), "Generate .mpack", false);
                //result = await ConsoleHelper.StartProcessAsync($"'/Applications/Visual Studio.app/Contents/MacOS/vstool'", $"setup pack '{dllFilename}'", dir, new Dictionary<string, string>(), "Generate .mpack", false);

                var result = await RunBashCommandAsync($"/Applications/Visual\\ Studio.app/Contents/MacOS/vstool setup pack {dllFilename}");

                // Move .mpack to solution folder
                files = Directory.GetFiles(vsmPath, "*.mpack");
                foreach (var file in files)
                {
                    if (file.Contains(IdeApp.ProjectOperations.CurrentSelectedProject.Name))
                    {
                        File.Move(file, dir + "/" + Path.GetFileName(file));
                        MessageService.ShowMessage(".mpack file generated.", $"File {Path.GetFileName(file)}\nhas been generated in folder:\n{dir}.");
                    }
                }
            }
        }

        public Task<string> RunBashCommandAsync(string command)
        {
            var tcs = new TaskCompletionSource<string>();

            Process process = new Process();

            Debug.WriteLine(command);

            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.EnableRaisingEvents = true;

            process.Exited += (sender, e) =>
            {
                string output = process.StandardOutput.ReadToEnd();
                tcs.SetResult(output);
                process.Dispose();
            };
            process.OutputDataReceived += (s, e) => Debug.WriteLine($"Output: {e.Data}");
            process.ErrorDataReceived += (s, e) => Debug.WriteLine($"Error: {e.Data}");
            process.Start();

            return tcs.Task;
        }

        private string GetDllFullFilename()
        {
            var dir = IdeApp.ProjectOperations.CurrentSelectedProject.BaseDirectory.ToString();
            dir = Path.Combine(dir, "bin");
            var filename = IdeApp.ProjectOperations.CurrentSelectedProject.Name + ".dll";

            if (Directory.Exists(dir))
            {

                if (File.Exists(Path.Combine(dir, filename)))
                {
                    return Path.Combine(dir, filename);

                }
                else if (Directory.Exists(Path.Combine(dir, "release")))
                {
                    dir = Path.Combine(dir, "release");
                    if (File.Exists(Path.Combine(dir, filename)))
                    {
                        return Path.Combine(dir, filename);
                    }
                }
            }

            return string.Empty;
        }
    }
}