using System;
using System.ComponentModel.Design;
using System.Globalization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Management.Automation;
using System.IO;
using System.Windows;
using System.Xml;

namespace VSIXProject1
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command3
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3d7b320f-2715-4286-bfb4-c68a9030d8a9");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command3"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Command3(Package package)
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
        public static Command3 Instance
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
            Instance = new Command3(package);
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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            EnvDTE.Project project = null;
            // имя текущего проета в решении
            try
            {
                var targetProjName = dte.Solution.Properties.Item("StartupProject").Value.ToString();
                project = GetCurrentProject(dte.Solution.Projects, targetProjName);
                Configuration conf = project.ConfigurationManager.ActiveConfiguration;
                // текущий путь
                string str = conf.Properties.Item("OutputPath").Value.ToString();
                //string str1 = project.FullName;
                //str += @"AppX\AppxManifest.xml";
                var path = Path.Combine(Path.GetDirectoryName(project.FullName), str, @"AppxManifest.xml");
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(path);
                XmlNodeList xmlNodes = xmlDocument.GetElementsByTagName("Package");
                
                    foreach (XmlNode xmlNode in xmlNodes.Item(0).ChildNodes)
                    {

                        if (xmlNode.Name.Equals("Applications"))
                        {
                            var app = xmlNode.FirstChild as XmlElement;
                            var id = app?.GetAttribute("id") as string;
                            for (int i = 0; i < 9; i++)
                            {
                                XmlNode copyApp = app.Clone();
                                ///todo change id
                                string val = app.Attributes["Id"].Value + i.ToString();
                                copyApp.Attributes["Id"].Value = val;
                                xmlNode.AppendChild(copyApp);
                            }

                        }
                }
                xmlDocument.Save(path);

                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddCommand("Add-AppxPackage");
                    PowerShellInstance.AddParameter("register");
                    PowerShellInstance.AddArgument(path);
                    PowerShellInstance.Invoke();
                }

                MessageBox.Show("Success deployed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("error: " + ex.Message);
            }

            //  FormOptions f = new FormOptions();
            //            f.Show();


        }
    }
}
