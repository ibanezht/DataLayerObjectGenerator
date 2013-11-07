
namespace Heath.Dlog.UI
{
    /// <summary> 
    /// Provides data and controls the behavior of its associated view. 
    /// </summary> 
    internal abstract class Presenter<TView>
    {
        /// <summary> 
        /// Called by its associated view when initialization is complete.
        /// </summary> 
        public abstract void ViewReady();

        #region properties

        /// <summary> 
        /// The view controlled by this class. 
        /// </summary> 
        public TView View { get; set; }

        #endregion
    }
}