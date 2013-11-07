using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Heath.Dlog.CodeGen;

namespace Heath.Dlog.UI
{
    public partial class MainWindow : IMainWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _presenter = new MainWindowPresenter { View = this };

            languageToolStripComboBox.Items.Add(CSharp);
            languageToolStripComboBox.Items.Add(VisualBasic);
            languageToolStripComboBox.SelectedIndex = 0;

            Loaded += (sender, e) => _presenter.ViewReady();
            Activated += (sender, e) => codeTextBox.Focus();
        }

        #region IMainWindow Members

        /// <summary>
        /// The main SQL server loading method.
        /// </summary>
        /// <param name="serverName">The name of the SQL server being loaded.</param>
        /// <param name="databaseName">The name of the database being loaded.</param>
        public void LoadServerDatabaseUI(string serverName, string databaseName)
        {
            var serverNode = GetServerTreeViewItem(serverName);

            serverObjectsTreeView.Items.Clear();
            serverObjectsTreeView.Items.Add(serverNode);

            var databaseNode = GetDatabaseTreeViewItem(databaseName);

            serverNode.Items.Add(databaseNode);
            serverNode.IsExpanded = true;
        }

        /// <summary>
        /// Loads the tables for the corresponding server object.
        /// </summary>
        /// <param name="serverObject">A server object representing a database.</param>
        /// <param name="tableNames">A collection of all the user tables names contained in the database.</param>
        public void LoadDatabaseTablesUI(ServerObject serverObject, IEnumerable<string> tableNames)
        {
            var tablesNode = (TreeViewItem)serverObject.Control;

            tablesNode.Items.Clear();

            foreach (var tableName in tableNames)
                tablesNode.Items.Add(GetTreeViewItem(tableName, "Images/table_sql.png", ServerObjectType.Table));
        }

        /// <summary>
        /// Loads the views for the corresponding server object.
        /// </summary>
        /// <param name="serverObject">A server object representing a database.</param>
        /// <param name="viewNames">A collection of all the user view names contained in the database.</param>
        public void LoadDatabaseViewsUI(ServerObject serverObject, IEnumerable<string> viewNames)
        {
            var viewsNode = (TreeViewItem)serverObject.Control;

            viewsNode.Items.Clear();

            foreach (var viewName in viewNames)
                viewsNode.Items.Add(GetTreeViewItem(viewName, "Images/table_sql_view.png", ServerObjectType.View));
        }

        #endregion

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected void TreeItemSelected(object sender, RoutedEventArgs e)
        {
            var serverObject = (ServerObject)((TreeViewItem)e.Source).Tag;

            Cursor = Cursors.Wait;
            _presenter.SetServerObject(serverObject);
            Cursor = Cursors.Arrow;
        }

        protected void TreeItemExpanded(object sender, RoutedEventArgs e)
        {
            var serverObject = (ServerObject)((TreeViewItem)e.Source).Tag;

            Cursor = Cursors.Wait;
            _presenter.ExpandServerObject(serverObject);
            Cursor = Cursors.Arrow;
        }

        private void ConnectClicked(object sender, RoutedEventArgs e)
        {
            _presenter.Connect();
        }

        private void GenerateEntityClicked(object sender, RoutedEventArgs e)
        {
            var languageOption = GetLanguageOption();
            var entity = _presenter.GetCurrentServerObjectEntity(languageOption);

            codeTextBox.Text = entity;
        }

        private void GenerateDataObjectClicked(object sender, RoutedEventArgs e)
        {
            var languageOption = GetLanguageOption();
            var dataObject = _presenter.GetCurrentServerObjectDataObject(languageOption);

            codeTextBox.Text = dataObject;
        }

        private void CanExecuteGenerated(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_presenter == null)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = _presenter.CurrentServerObjectType == ServerObjectType.Table || _presenter.CurrentServerObjectType == ServerObjectType.View;
        }

        #region Factory Methods

        private string GetLanguageOption()
        {
            var retval = LanguageOption.CSharp;

            if (languageToolStripComboBox.Text == VisualBasic)
                retval = LanguageOption.VisualBasic;

            return retval;
        }

        private TreeViewItem GetServerTreeViewItem(string name)
        {
            return GetTreeViewItem(name, "Images/server.png", ServerObjectType.Server);
        }

        private TreeViewItem GetDatabaseTreeViewItem(string name)
        {
            var retval = GetTreeViewItem(name, "Images/data.png", ServerObjectType.Database);
            var tablesNode = GetSeparatorTreeViewItem(Properties.Resources.TablesSeparatorNodeName, "Images/folder_closed.png", ServerObjectType.Tables);
            var viewsNode = GetSeparatorTreeViewItem(Properties.Resources.ViewsSeparatorNodeName, "Images/folder_closed.png", ServerObjectType.Views);

            retval.Items.Add(tablesNode);
            retval.Items.Add(viewsNode);

            return retval;
        }

        private TreeViewItem GetSeparatorTreeViewItem(string name, string imageSource, ServerObjectType type)
        {
            var retval = GetTreeViewItem(name, imageSource, type);

            retval.Items.Add(string.Empty);

            return retval;
        }

        private TreeViewItem GetTreeViewItem(string name, string imageSource, ServerObjectType type)
        {
            var retval = new TreeViewItem();
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            if (!string.IsNullOrEmpty(imageSource))
                stackPanel.Children.Add(new Image { Source = new BitmapImage(new Uri(imageSource, UriKind.Relative)), Height = 16, Width = 16 });

            stackPanel.Children.Add(new TextBlock(new Run(string.Format(" {0} ", name))));

            retval.Header = stackPanel;
            retval.Tag = new ServerObject(name, type, retval);
            retval.Selected += TreeItemSelected;
            retval.Expanded += TreeItemExpanded;

            return retval;
        }

        #endregion

        #region fields

        private const string CSharp = "C#";
        private const string VisualBasic = "Visual Basic";

        public static readonly RoutedCommand ConnectCommand = new RoutedCommand("Connect", typeof(MainWindow));
        public static readonly RoutedCommand GenerateEntityCommand = new RoutedCommand("GenerateEntity", typeof(MainWindow));
        public static readonly RoutedCommand GenerateDataObjectCommand = new RoutedCommand("GenerateDataObject", typeof(MainWindow));
        private MainWindowPresenter _presenter;

        #endregion
    }
}