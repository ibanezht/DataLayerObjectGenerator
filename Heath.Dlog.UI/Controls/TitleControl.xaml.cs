
namespace Heath.Dlog.UI.Controls
{
    /// <summary>
    /// Represents a control title.
    /// </summary>
    public partial class TitleControl
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return titleControlTextBlock.Text; }
            set { titleControlTextBlock.Text = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleControl"/> class.
        /// </summary>
        public TitleControl()
        {
            InitializeComponent();
        }
    }
}