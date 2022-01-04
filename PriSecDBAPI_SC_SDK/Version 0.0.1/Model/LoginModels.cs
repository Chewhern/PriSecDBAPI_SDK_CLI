using System;

namespace PriSecDBAPI_SC_SDK.Model
{
    class LoginModels
    {
        public String RequestStatus { get; set; }

        public String SignedRandomChallengeBase64String { get; set; }

        public String ServerECDSAPKBase64String { get; set; }
    }
}
