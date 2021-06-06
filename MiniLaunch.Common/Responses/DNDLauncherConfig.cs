namespace MiniLaunch.Common.Responses
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class configuration
    {
        private configurationAdd[] appSettingsField;

        [System.Xml.Serialization.XmlArrayItem("add", IsNullable = false)]
        public configurationAdd[] appSettings
        {
            get => appSettingsField;
            set => appSettingsField = value;
        }
    }

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class configurationAdd
    {
        private string keyField;

        private string valueField;

        [System.Xml.Serialization.XmlAttribute()]
        public string key
        {
            get => keyField;
            set => keyField = value;
        }

        [System.Xml.Serialization.XmlAttribute()]
        public string value
        {
            get => valueField;
            set => valueField = value;
        }
    }
}