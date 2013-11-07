using System.CodeDom.Compiler;
using Microsoft.VisualBasic;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace Heath.Dlog.CodeGen
{
    /// <summary>
    /// Fills the <see cref="ILanguageProvider"/> contract for the Visual Basic language.
    /// </summary>
    internal class VisualBasicLanguageProvider : ILanguageProvider
    {
        #region ILanguageProvider Members

        /// <summary>
        /// Gets a CodeDom provider for a specific language option.
        /// </summary>
        /// <returns>A <see cref="CodeDomProvider"/>.</returns>
        public CodeDomProvider GetCodeDomProvider()
        {
            return new VBCodeProvider();
        }

        /// <summary>
        /// Gets a string representation of the provided data type specific to the language option.
        /// </summary>
        /// <param name="dataType">The provided data type.</param>
        /// <returns>
        /// A string representation of the provided data type.
        /// </returns>
        public string GetTypeString(Smo.DataType dataType)
        {
            switch (dataType.SqlDataType)
            {
                case Smo.SqlDataType.Bit:
                    return "System.Boolean";

                case Smo.SqlDataType.SmallDateTime:
                    return "System.DateTime";

                case Smo.SqlDataType.Int:
                    return "System.Int32";

                case Smo.SqlDataType.NVarChar:
                    return "System.String";

                case Smo.SqlDataType.VarChar:
                    return "System.String";

                default:
                    return "System.String";
            }
        }

        /// <summary>
        /// Gets a formated version of the field specific to the language option.
        /// </summary>
        /// <param name="original">The original field name.</param>
        /// <returns>A formatted field string.</returns>
        public string GetFormattedInternalFieldName(string original)
        {
            return string.Format("_{0}{1}", original.Substring(0, 1).ToLower(), original.Substring(1, original.Length - 1));
        }

        /// <summary>
        /// Gets a formated version of the parameter specific to the language option.
        /// </summary>
        /// <param name="original">The original parameter name.</param>
        /// <returns>A formatted parameter string.</returns>
        public string GetFormattedParamenterName(string original)
        {
            return string.Concat(original.Substring(0, 1).ToLower(), original.Substring(1, original.Length - 1));
        }

        /// <summary>
        /// Gets a formated version of the constructor specific to the language option.
        /// </summary>
        /// <param name="original">The original constructor.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>A formatted constructor string.</returns>
        public string GetFormattedConstructor(string original, string typeName)
        {
            return original;
        }

        #endregion
    }
}