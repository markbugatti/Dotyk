using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSIXProject1
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OptionsWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x1020;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("37107dd7-b549-4267-8160-4bfb2fd06a14");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OptionsWindowCommand(Package package)
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
                var menuItem = new MenuCommand(this.ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OptionsWindowCommand Instance
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

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new OptionsWindowCommand(package);
        }

        private Project GetCurrentProject(Projects projects, string targetProjName)
        {
            foreach (Project project in projects)
            {
                if (project.Name == targetProjName)
                {
                    return project;
                }
            }
            return null;
        }
        private void FillConfs(OptioinsWindowControl OptioinsWindow, Project project)
        {
            OptioinsWindow.ComboBoxConf.Items.Clear();
            foreach (Configuration item in project.ConfigurationManager)
            {
                bool exist = false;
                foreach (string text in OptioinsWindow.ComboBoxConf.Items)
                {
                    if (item.ConfigurationName == text)
                    {
                        exist = true;
                        break;
                    }
                }
                if(!exist)
                {
                    OptioinsWindow.ComboBoxConf.Items.Add(item.ConfigurationName);
                }
            }
            OptioinsWindow.ComboBoxConf.SelectedValue = project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            EnvDTE.Project project = null;
            // имя текущего проета в решении
            try
            {
                var targetProjName = dte.Solution.Properties.Item("StartupProject").Value.ToString();
                // получить проект по имени
                project = GetCurrentProject(dte.Solution.Projects, targetProjName);
                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
                ToolWindowPane window = this.package.FindToolWindow(typeof(OptionsWindow), 0, true);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
                OptioinsWindowControl OptioinsWindow = window.Content as OptioinsWindowControl;
                // заполнить Configuration name
                FillConfs(OptioinsWindow, project);
                // отобразить окно
                //window.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_RIGHT;
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                //windowFrame.SetFramePos(VSSETFRAMEPOS.SFP_fDockRight, new Guid("4e6c34e8-a1dc-4116-9c56-c14adcb92015"), 10, 10, 60, 60);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch
            {
                MessageBox.Show("Не выбран проект");
            }

        }
    }
}
