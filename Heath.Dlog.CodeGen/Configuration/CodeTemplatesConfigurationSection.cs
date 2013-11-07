using System.Configuration;

namespace Heath.Dlog.CodeGen.Configuration
{
    internal class CodeTemplatesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("entityTemplates")]
        public EntityTemplateElementCollection EntityTemplates
        {
            get { return (EntityTemplateElementCollection)this["entityTemplates"]; }
            set { this["entityTemplates"] = value; }
        }

        [ConfigurationProperty("dataObjectTemplates")]
        public DataObjectTemplateElementCollection DataObjectTemplates
        {
            get { return (DataObjectTemplateElementCollection)this["dataObjectTemplates"]; }
            set { this["dataObjectTemplates"] = value; }
        }
    }
}