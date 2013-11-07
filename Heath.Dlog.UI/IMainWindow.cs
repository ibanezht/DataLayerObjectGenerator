using System.Collections.Generic;

namespace Heath.Dlog.UI
{
    /// <summary>
    /// Represents a contract a that form will fill for displaying SQL server objects.
    /// </summary>
    internal interface IMainWindow
    {
        /// <summary>
        /// The main SQL server loading method.
        /// </summary>
        /// <param name="serverName">The name of the SQL server being loaded.</param>
        /// <param name="databaseName">The name of the database being loaded.</param>
        void LoadServerDatabaseUI(string serverName, string databaseName);

        /// <summary>
        /// Loads the tables for the corresponding server object.
        /// </summary>
        /// <param name="serverObject">A server object representing a database.</param>
        /// <param name="tableNames">A collection of all the user tables names contained in the database.</param>
        void LoadDatabaseTablesUI(ServerObject serverObject, IEnumerable<string> tableNames);

        /// <summary>
        /// Loads the views for the corresponding server object.
        /// </summary>
        /// <param name="serverObject">A server object representing a database.</param>
        /// <param name="viewNames">A collection of all the user view names contained in the database.</param>
        void LoadDatabaseViewsUI(ServerObject serverObject, IEnumerable<string> viewNames);
    }
}