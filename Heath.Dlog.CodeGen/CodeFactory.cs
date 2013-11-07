using System.Configuration;
using System.Linq;
using Heath.Dlog.CodeGen.Configuration;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace Heath.Dlog.CodeGen
{
    /// <summary>
    /// A factory class for code generation.
    /// </summary>
    public class CodeFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFactory"/> class.
        /// </summary>
        /// <param name="languageOption">The language option.</param>
        public CodeFactory(string languageOption)
        {
            LanguageOption = languageOption;
        }

        /// <summary>
        /// Returns an entity code string representation of the provided <see cref="Smo.TableViewBase"/>.
        /// </summary>
        /// <returns>An entity code string.</returns>
        public string GetEntityCode(Smo.TableViewBase tableView)
        {
            var languageProvider = GetLanguageProvider();
            var entityTemplate = GetEntityTemplate();

            return CodeService.GenerateEntityCode(tableView, languageProvider, entityTemplate);
        }

        /// <summary>
        /// Returns an data object code string representation of the provided <see cref="Smo.TableViewBase"/>.
        /// </summary>
        /// <returns>A data object code string.</returns>
        public string GetDataObjectCode(Smo.TableViewBase tableView)
        {
            var languageProvider = GetLanguageProvider();
            var dataObjectTemplate = GetDataObjectTemplate();

            return CodeService.GetDataObjectCode(tableView, languageProvider, dataObjectTemplate);
        }

        #region Factory Methods

        /// <summary>
        /// Gets a provider specific to the language option chosen by the user.
        /// </summary>
        private ILanguageProvider GetLanguageProvider()
        {
            ILanguageProvider retval = null;

            switch (LanguageOption)
            {
                case CodeGen.LanguageOption.CSharp:
                    retval = new CSharpLanguageProvider();
                    break;

                case CodeGen.LanguageOption.VisualBasic:
                    retval = new VisualBasicLanguageProvider();
                    break;
            }

            return retval;
        }

        /// <summary>
        /// Gets the entity template string from the app.config.
        /// </summary>
        private string GetEntityTemplate()
        {
            var codeTemplatesConfigurationSection = (CodeTemplatesConfigurationSection)ConfigurationManager.GetSection("codeTemplates");

            if (codeTemplatesConfigurationSection == null)
                return string.Empty;

            return codeTemplatesConfigurationSection.EntityTemplates.Cast<EntityTemplateElement>().Where(
                ete => ete.Language == LanguageOption).Select(
                ete => ete.Template).FirstOrDefault();
        }

        /// <summary>
        /// Gets the data object template string from the app.config.
        /// </summary>
        private string GetDataObjectTemplate()
        {
            var codeTemplatesConfigurationSection = (CodeTemplatesConfigurationSection)ConfigurationManager.GetSection("codeTemplates");

            if (codeTemplatesConfigurationSection == null)
                return string.Empty;

            return codeTemplatesConfigurationSection.DataObjectTemplates.Cast<DataObjectTemplateElement>().Where(
                dote => dote.Language == LanguageOption).Select(
                dote => dote.Template).FirstOrDefault();
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the language option.
        /// </summary>
        /// <value>The language option.</value>
        public string LanguageOption { get; private set; }

        #endregion

    }
}