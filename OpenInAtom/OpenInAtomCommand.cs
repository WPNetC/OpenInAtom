//------------------------------------------------------------------------------
// <copyright file="OpenInAtomCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;

namespace OpenInAtom
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenInAtomCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7bbc7adc-c40f-4a3c-9d06-2a937a46091f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenInAtomCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenInAtomCommand(Package package)
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
        public static OpenInAtomCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new OpenInAtomCommand(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(SDTE)) as DTE;
            if (dte.SelectedItems.Count != 1)
                return;
            string fullPath = null;
            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.ProjectItem == null)
                    return;

                var projectItem = selectedItem.ProjectItem;
                var fullPathProperty = projectItem?.Properties?.Item("FullPath");
                if (fullPathProperty == null)
                    return;

                fullPath = fullPathProperty.Value.ToString();
            }
            if (fullPath != null)
            {
                var proc = new System.Diagnostics.Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = "atom";
                proc.StartInfo.Arguments = "-n \"" + fullPath + "\"";
                proc.Start();
            }
        }
    }
}
