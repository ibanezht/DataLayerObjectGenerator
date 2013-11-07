using System;
using System.Windows;

namespace Heath.Dlog.UI
{
    /// <summary>
    /// Represents a dialog for connecting to a SQL server database instance.
    /// </summary>
    public partial class ConnectWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectWindow"/> class.
        /// </summary>
        public ConnectWindow()
        {
            InitializeComponent();

            Activated += (sender, e) =>
                         {
                             serverTextBox.Text = Environment.MachineName;
                             serverTextBox.Focus();
                         };
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverTextBox.Text) || string.IsNullOrEmpty(databaseTextBox.Text))
                return;

            DialogResult = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #region properties

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>The server.</value>
        public string Server { get { return serverTextBox.Text; } }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        public string Database { get { return databaseTextBox.Text; } }

        #endregion
    }
}