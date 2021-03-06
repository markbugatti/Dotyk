﻿namespace VSIXProject1
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OptioinsWindowControl.
    /// </summary>
    public partial class OptioinsWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptioinsWindowControl"/> class.
        /// </summary>
        public ComboBox ComboBoxConf;
        public ComboBox ComboBoxVerb;
        public TextBox TextBoxServer;

        public OptioinsWindowControl()
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
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptioins", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "OptioinsWindow");
        }
    }
}