using System;

namespace PriSecDBAPI_CRUD_SDK.Model
{
    public class NormalDBModel
    {
        public SealedDBCredentialModel MyDBCredentialModel { get; set; }

        public String UniquePaymentID { get; set; }

        public String Base64QueryString { get; set; }

        public String Base64ParameterName { get; set; }

        public String Base64ParameterValue { get; set; }
    }
}
