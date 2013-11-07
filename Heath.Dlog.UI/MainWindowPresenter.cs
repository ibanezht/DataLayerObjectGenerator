using System.Collections.Generic;
using System.Linq;
using Heath.Dlog.CodeGen;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace Heath.Dlog.UI
{
    /// <summary>
    /// 
    /// </summary>
    internal class MainWindowPresenter : Presenter<IMainWindow>
    {
        public override void ViewReady()
        {
            Connect();
        }

        private void SetCurrentServer()
        {
            if (_currentServer != null && string.Compare(_currentServer.Name, _currentServerObject.Name) == 0)
                return;

            _currentServer = new Smo.Server(_currentServerObject.Name);
        }

        private void SetCurrentDatabase()
        {
            _currentDatabase = _currentServer.Databases[_currentServerObject.Name];
        }

        private void SetCurrentTable()
        {
            _currentTable = _currentDatabase.Tables[_currentServerObject.Name];
        }

        private void SetCurrentView()
        {
            _currentView = _currentDatabase.Views[_currentServerObject.Name];
        }

        /// <summary>
        /// Opens a dialog for choosing a SQL server and loads the chosen server into the view.
        /// </summary>
        public void Connect()
        {
            var connectWindow = new ConnectWindow();
            var result = connectWindow.ShowDialog();

            if (result.HasValue && result.Value)
                View.LoadServerDatabaseUI(connectWindow.Server, connectWindow.Database);
        }

        /// <summary>
        /// Called from the view noting a user selecting a ;node.
        /// </summary>
        /// <param name="serverObject">The server object that the user selected.</param>
        public void SetServerObject(ServerObject serverObject)
        {
            _currentServerObject = serverObject;

            switch (_currentServerObject.Type)
            {
                case ServerObjectType.Server:
                    SetCurrentServer();
                    break;

                case ServerObjectType.Database:
                    SetCurrentDatabase();
                    break;

                case ServerObjectType.Table:
                    SetCurrentTable();
                    break;

                case ServerObjectType.View:
                    SetCurrentView();
                    break;
            }
        }

        /// <summary>
        /// Called from the view noting a user expanding a node.
        /// </summary>
        /// <param name="serverObject">The server object that the user expanded.</param>
        public void ExpandServerObject(ServerObject serverObject)
        {
            _currentServerObject = serverObject;

            switch (_currentServerObject.Type)
            {
                case ServerObjectType.Server:
                    SetCurrentServer();
                    break;

                case ServerObjectType.Database:
                    SetCurrentDatabase();
                    break;

                case ServerObjectType.Tables:
                    LoadDatabaseTables();
                    break;

                case ServerObjectType.Views:
                    LoadDatabaseViews();
                    break;

                case ServerObjectType.Table:
                    SetCurrentTable();
                    break;

                case ServerObjectType.View:
                    SetCurrentTable();
                    break;
            }
        }

        /// <summary>
        /// Retrieves table names from the SQL server and loads them into the UI.
        /// </summary>
        private void LoadDatabaseTables()
        {
            if (_currentServerObject.IsLoaded)
                return;

            View.LoadDatabaseTablesUI(_currentServerObject, GetCurrentDatabaseTableNames());
            _currentServerObject.IsLoaded = true;
        }

        /// <summary>
        /// Retrieves view names from the SQL server and loads them into the UI.
        /// </summary>
        private void LoadDatabaseViews()
        {
            if (_currentServerObject.IsLoaded)
                return;

            View.LoadDatabaseViewsUI(_currentServerObject, GetCurrentDatabaseViewNames());
            _currentServerObject.IsLoaded = true;
        }

        /// <summary>
        /// Gets generated entity code for the selected server object.
        /// </summary>
        /// <returns>A code string.</returns>
        public string GetCurrentServerObjectEntity(string languageOption)
        {
            var codeFactory = new CodeFactory(languageOption);
            var tableView = GetCurrentTableView();

            return codeFactory.GetEntityCode(tableView);
        }

        /// <summary>
        /// Gets generated data object code for the selected server object.
        /// </summary>
        /// <returns>A code string.</returns>
        public string GetCurrentServerObjectDataObject(string languageOption)
        {
            var codeFactory = new CodeFactory(languageOption);
            var tableView = GetCurrentTableView();

            return codeFactory.GetDataObjectCode(tableView);
        }

        #region Factory Methods

        private IEnumerable<string> GetCurrentDatabaseTableNames()
        {
            return _currentDatabase.Tables.Cast<Smo.Table>()
                .Where(t => !t.IsSystemObject).Select(t => t.Name);
        }

        private IEnumerable<string> GetCurrentDatabaseViewNames()
        {
            return _currentDatabase.Views.Cast<Smo.View>()
                .Where(v => !v.IsSystemObject).Select(v => v.Name);
        }

        private Smo.TableViewBase GetCurrentTableView()
        {
            Smo.TableViewBase retval = null;

            switch (_currentServerObject.Type)
            {
                case ServerObjectType.Table:
                    retval = _currentTable;
                    break;

                case ServerObjectType.View:
                    retval = _currentView;
                    break;
            }

            return retval;
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets the type of the current server object.
        /// </summary>
        /// <value>The type of the current server object.</value>
        public ServerObjectType CurrentServerObjectType
        {
            get { return _currentServerObject == null ? ServerObjectType.None : _currentServerObject.Type; }
        }

        #endregion

        #region fields

        private Smo.Server _currentServer;
        private Smo.Database _currentDatabase;
        private Smo.Table _currentTable;
        private Smo.View _currentView;
        private ServerObject _currentServerObject;

        #endregion
    }
}