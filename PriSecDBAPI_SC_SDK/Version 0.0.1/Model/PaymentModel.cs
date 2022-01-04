using System;

namespace PriSecDBAPI_SC_SDK.Model
{
    class PaymentModel
    {
        public String Status { get; set; }

        public String CipheredDBName { get; set; }

        public String CipheredDBAccountUserName { get; set; }

        public String CipheredDBAccountPassword { get; set; }

        public String SystemPaymentID { get; set; }

        public String GMT8PaymentMadeDateTime { get; set; }
    }
}
