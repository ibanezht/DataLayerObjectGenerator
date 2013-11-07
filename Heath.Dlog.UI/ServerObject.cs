
namespace Heath.Dlog.UI
{
    /// <summary>
    /// Represents a SQL server object.
    /// </summary>
    public class ServerObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerObject"/> class.
        /// </summary>
        /// <param name="name">The name of the server object.</param>
        /// <param name="type">The type of the server object.</param>
        /// <param name="control">The control of the server object.</param>
        public ServerObject(string name, ServerObjectType type, object control)
        {
            Name = name;
            Type = type;
            Control = control;
            IsLoaded = false;
        }

        #region properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public ServerObjectType Type { get; private set; }

        /// <summary>
        /// Gets or sets the associated control.
        /// </summary>
        /// <value>The associated control.</value>
        public object Control { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
        public bool IsLoaded { get; set; }

        #endregion
    }
}