using System.Configuration;

namespace Heath.Dlog.CodeGen.Configuration
{
    internal class DataObjectTemplateElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataObjectTemplateElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataObjectTemplateElement)element).Name;
        }
    }
}