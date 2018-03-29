using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using EnvDTE100;
using System.Windows.Forms;
using VSLangProj;
using VSLangProj2;
using VSLangProj80;
//using VSLangProj90;
//using VSLangProj100;

namespace VSIXProject1
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OptionsWindowControl.
    /// </summary>
    public partial class OptionsWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsWindowControl"/> class.
        /// </summary>
        public ComboBox ComboBoxConf;
        public ComboBox ComboBoxVerb;
        public TextBox TextBoxServer;

        public OptionsWindowControl()
        {
            this.InitializeComponent();
            ComboBoxConf = FindName("Configuration") as ComboBox;
            ComboBoxVerb = FindName("Verbosity") as ComboBox;
        }
        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "OptionsWindow");
        }
    }
}