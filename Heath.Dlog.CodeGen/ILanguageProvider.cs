using System.CodeDom.Compiler;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace Heath.Dlog.CodeGen
{
    /// <summary>
    /// Represents a contract that language provider objects must fill.
    /// </summary>
    internal interface ILanguageProvider
    {
        /// <summary>
        /// Gets a CodeDom provider for a specific language option.
        /// </summary>
        /// <returns>A <see cref="CodeDomProvider"/>.</returns>
        CodeDomProvider GetCodeDomProvider();

        /// <summary>
        /// Gets a string representation of the provided data type specific to the language option.
        /// </summary>
        /// <param name="dataType">The provided data type.</param>
        /// <returns>A string representation of the provided data type.</returns>
        string GetTypeString(Smo.DataType dataType);

        /// <summary>
        /// Gets a formated version of the field specific to the language option.
        /// </summary>
        /// <param name="original">The original field name.</param>
        /// <returns>A formatted field string.</returns>
        string GetFormattedInternalFieldName(string original);

        /// <summary>
        /// Gets a formated version of the parameter specific to the language option.
        /// </summary>
        /// <param name="original">The original parameter name.</param>
        /// <returns>A formatted parameter string.</returns>
        string GetFormattedParamenterName(string original);

        /// <summary>
        /// Gets a formated version of the constructor specific to the language option.
        /// </summary>
        /// <param name="original">The original constructor.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>A formatted constructor string.</returns>
        string GetFormattedConstructor(string original, string typeName);
    }
}