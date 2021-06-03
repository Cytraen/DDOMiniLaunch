namespace MiniLaunch.Common.Responses
{
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.turbine.com/SE/GLS", IsNullable = false)]
    public partial class GetDatacentersResponse
    {
        private GetDatacentersResponseGetDatacentersResult getDatacentersResultField;

        public GetDatacentersResponseGetDatacentersResult GetDatacentersResult
        {
            get => getDatacentersResultField;
            set => getDatacentersResultField = value;
        }
    }

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    public partial class GetDatacentersResponseGetDatacentersResult
    {
        private GetDatacentersResponseGetDatacentersResultDatacenter datacenterField;

        public GetDatacentersResponseGetDatacentersResultDatacenter Datacenter
        {
            get => datacenterField;
            set => datacenterField = value;
        }
    }

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    public partial class GetDatacentersResponseGetDatacentersResultDatacenter
    {
        private string nameField;

        private GetDatacentersResponseGetDatacentersResultDatacenterWorld[] worldsField;

        private string authServerField;

        private string patchServerField;

        private string launcherConfigurationServerField;

        public string Name
        {
            get => nameField;
            set => nameField = value;
        }

        [System.Xml.Serialization.XmlArrayItem("World", IsNullable = false)]
        public GetDatacentersResponseGetDatacentersResultDatacenterWorld[] Worlds
        {
            get => worldsField;
            set => worldsField = value;
        }

        public string AuthServer
        {
            get => authServerField;
            set => authServerField = value;
        }

        public string PatchServer
        {
            get => patchServerField;
            set => patchServerField = value;
        }

        public string LauncherConfigurationServer
        {
            get => launcherConfigurationServerField;
            set => launcherConfigurationServerField = value;
        }
    }

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    public partial class GetDatacentersResponseGetDatacentersResultDatacenterWorld
    {
        private string nameField;

        private string loginServerUrlField;

        private string chatServerUrlField;

        private string statusServerUrlField;

        private byte orderField;

        private string languageField;

        public string Name
        {
            get => nameField;
            set => nameField = value;
        }

        public string LoginServerUrl
        {
            get => loginServerUrlField;
            set => loginServerUrlField = value;
        }

        public string ChatServerUrl
        {
            get => chatServerUrlField;
            set => chatServerUrlField = value;
        }

        public string StatusServerUrl
        {
            get => statusServerUrlField;
            set => statusServerUrlField = value;
        }

        public byte Order
        {
            get => orderField;
            set => orderField = value;
        }

        public string Language
        {
            get => languageField;
            set => languageField = value;
        }
    }
}