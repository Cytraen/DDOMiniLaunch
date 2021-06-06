namespace MiniLaunch.Common.Responses
{
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class Result
    {
        private string commandField;

        private string hResultField;

        private string queueNameField;

        private string queueNumberField;

        private string nowServingNumberField;

        private byte loginTierField;

        private string contextNumberField;

        public string Command
        {
            get => commandField;
            set => commandField = value;
        }

        public string HResult
        {
            get => hResultField;
            set => hResultField = value;
        }

        public string QueueName
        {
            get => queueNameField;
            set => queueNameField = value;
        }

        public string QueueNumber
        {
            get => queueNumberField;
            set => queueNumberField = value;
        }

        public int QueueNumberAsInt
        {
            get => int.Parse(queueNumberField[2..], System.Globalization.NumberStyles.HexNumber);
        }

        public string NowServingNumber
        {
            get => nowServingNumberField;
            set => nowServingNumberField = value;
        }

        public int NowServingNumberAsInt
        {
            get => int.Parse(nowServingNumberField[2..], System.Globalization.NumberStyles.HexNumber);
        }

        public byte LoginTier
        {
            get => loginTierField;
            set => loginTierField = value;
        }

        public string ContextNumber
        {
            get => contextNumberField;
            set => contextNumberField = value;
        }
    }
}