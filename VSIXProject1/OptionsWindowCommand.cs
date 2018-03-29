using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Windows;
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
        public const int CommandId = 4130;
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7d9e0309-a18d-4cbd-935b-37b10720c19b");

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
        private void FullConfs(OptionsWindowControl optionsWindow, Project project, string confName)
        {
            optionsWindow.ComboBoxConf.Items.Clear();
            foreach (Configuration item in project.ConfigurationManager)
            {
                optionsWindow.ComboBoxConf.Items.Add(item.ConfigurationName);
            }
            optionsWindow.ComboBoxConf.SelectedValue = confName;
        }
        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(OptionsWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }
            // елси окно создалось
            else
            {
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
                OptionsWindowControl optionsWindow = window.Content as OptionsWindowControl;
                DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
                EnvDTE.Project project = null;
                // имя текущего проета в решении
                var targetProjName = dte.Solution.Properties.Item("StartupProject").Value.ToString();
                try
                {
                    // получить проект по имени
                    project = GetCurrentProject(dte.Solution.Projects, targetProjName);
                    Configuration conf = project.ConfigurationManager.ActiveConfiguration;
                    string platformTarget = conf.Properties.Item("PlatformTarget").Value.ToString();
                    var confName = conf.ConfigurationName;

                    //if (System.IO.Directory.Exists(Path.Combine("bin", platformTarget, confName)))
                    //    MessageBox.Show(conf.Properties.Item("PlatformTarget").Value.ToString());
                    //FullConfs(optionsWindow, project, confName);
                    // получить путь к Appx\manifest;
                    string str = conf.Properties.Item("OutputPath").Value.ToString();
                    str += @"AppX\AppxManifest.xml";
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error: " + ex.Message.ToString());
                }
            }  
        }
    }
}
