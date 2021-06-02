namespace MiniLaunch.Common.Responses
{
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.turbine.com/SE/GLS", IsNullable = false)]
    public partial class LoginAccountResponse
    {
        private LoginAccountResponseLoginAccountResult loginAccountResultField;

        public LoginAccountResponseLoginAccountResult LoginAccountResult
        {
            get => loginAccountResultField;
            set => loginAccountResultField = value;
        }
    }

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    public partial class LoginAccountResponseLoginAccountResult
    {
        private LoginAccountResponseLoginAccountResultGameSubscription[] subscriptionsField;

        private string ticketField;

        [System.Xml.Serialization.XmlArrayItem("GameSubscription", IsNullable = false)]
        public LoginAccountResponseLoginAccountResultGameSubscription[] Subscriptions
        {
            get => subscriptionsField;
            set => subscriptionsField = value;
        }

        public string Ticket
        {
            get => ticketField;
            set => ticketField = value;
        }
    }

    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.turbine.com/SE/GLS")]
    public partial class LoginAccountResponseLoginAccountResultGameSubscription
    {
        private string gameField;

        private string nameField;

        private string descriptionField;

        private string[] productTokensField;

        private object customerServiceTokensField;

        private string statusField;

        private string billingSystemTimeField;

        private object additionalInfoField;

        public string Game
        {
            get => gameField;
            set => gameField = value;
        }

        public string Name
        {
            get => nameField;
            set => nameField = value;
        }

        public string Description
        {
            get => descriptionField;
            set => descriptionField = value;
        }

        [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
        public string[] ProductTokens
        {
            get => productTokensField;
            set => productTokensField = value;
        }

        public object CustomerServiceTokens
        {
            get => customerServiceTokensField;
            set => customerServiceTokensField = value;
        }

        public string Status
        {
            get => statusField;
            set => statusField = value;
        }

        public string BillingSystemTime
        {
            get => billingSystemTimeField;
            set => billingSystemTimeField = value;
        }

        public object AdditionalInfo
        {
            get => additionalInfoField;
            set => additionalInfoField = value;
        }
    }
}