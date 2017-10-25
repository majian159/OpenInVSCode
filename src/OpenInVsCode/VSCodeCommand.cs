using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenInVsCode
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class VSCodeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ffddf954-b884-4200-89c9-caaef19bdf23");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSCodeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private VSCodeCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static VSCodeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private static Options _options;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            _options = (Options)package.GetDialogPage(typeof(Options));
            Instance = new VSCodeCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var fileName = GetFileName(dte);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            OpenVsCode(fileName);
        }

        private void OpenVsCode(string fileName)
        {
            var isDirectory = Directory.Exists(fileName);
            var start = new System.Diagnostics.ProcessStartInfo
            {
                FileName = $"\"{_options.PathToExe}\"",
                Arguments = isDirectory ? "." : fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            if (isDirectory)
            {
                var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
                start.WorkingDirectory = Path.GetDirectoryName(dte.Solution.FileName);
            }

            using (System.Diagnostics.Process.Start(start))
            {
            }
        }

        private static string GetFileName(DTE2 dte)
        {
            if (dte.ActiveWindow.Document != null)
                return "\"" + dte.ActiveDocument.FullName + "\"";

            var paths = ProjectHelpers.GetSelectedPaths(dte, _options.OpenSolutionProjectAsRegularFile);
            if (paths == null || !paths.Any())
                return null;
            var builder = new StringBuilder();
            foreach (var path in paths)
            {
                builder
                    .Append("\"")
                    .Append(path)
                    .Append("\" ");
            }
            return builder.Remove(builder.Length - 1, 1).ToString();
        }
    }
}