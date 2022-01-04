using System;

namespace PriSecDBAPI_SC_SDK.Model
{
    class LockDBAccountModel
    {
        public String SealedSessionID { get; set; }

        public String SealedDBUserName { get; set; }

        public String UniquePaymentID { get; set; }

        public String SignedRandomChallenge { get; set; }
    }
}
