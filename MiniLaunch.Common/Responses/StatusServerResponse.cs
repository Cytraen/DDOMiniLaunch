namespace MiniLaunch.Common.Responses
{
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class Status
    {
        private string logintierlastnumbersField;

        private string logintiersField;

        private string queuenamesField;

        private string allow_billing_roleField;

        private string queueurlsField;

        private string lastassignedqueuenumberField;

        private string nameField;

        private byte farmidField;

        private object deny_admin_roleField;

        private bool world_fullField;

        private decimal wait_hintField;

        private string we_perma_deathField;

        private string allow_admin_roleField;

        private string nowservingqueuenumberField;

        private object deny_billing_roleField;

        private string logintiermultipliersField;

        private string loginserversField;

        private byte world_pvppermissionField;

        public string logintierlastnumbers
        {
            get => logintierlastnumbersField;
            set => logintierlastnumbersField = value;
        }

        public string logintiers
        {
            get => logintiersField;
            set => logintiersField = value;
        }

        public string queuenames
        {
            get => queuenamesField;
            set => queuenamesField = value;
        }

        public string allow_billing_role
        {
            get => allow_billing_roleField;
            set => allow_billing_roleField = value;
        }

        public string queueurls
        {
            get => queueurlsField;
            set => queueurlsField = value;
        }

        public string lastassignedqueuenumber
        {
            get => lastassignedqueuenumberField;
            set => lastassignedqueuenumberField = value;
        }

        public string name
        {
            get => nameField;
            set => nameField = value;
        }

        public byte farmid
        {
            get => farmidField;
            set => farmidField = value;
        }

        public object deny_admin_role
        {
            get => deny_admin_roleField;
            set => deny_admin_roleField = value;
        }

        public bool world_full
        {
            get => world_fullField;
            set => world_fullField = value;
        }

        public decimal wait_hint
        {
            get => wait_hintField;
            set => wait_hintField = value;
        }

        public string we_perma_death
        {
            get => we_perma_deathField;
            set => we_perma_deathField = value;
        }

        public string allow_admin_role
        {
            get => allow_admin_roleField;
            set => allow_admin_roleField = value;
        }

        public string nowservingqueuenumber
        {
            get => nowservingqueuenumberField;
            set => nowservingqueuenumberField = value;
        }

        public object deny_billing_role
        {
            get => deny_billing_roleField;
            set => deny_billing_roleField = value;
        }

        public string logintiermultipliers
        {
            get => logintiermultipliersField;
            set => logintiermultipliersField = value;
        }

        public string loginservers
        {
            get => loginserversField;
            set => loginserversField = value;
        }

        public byte world_pvppermission
        {
            get => world_pvppermissionField;
            set => world_pvppermissionField = value;
        }
    }
}