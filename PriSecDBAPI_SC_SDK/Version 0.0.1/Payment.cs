using System;
using System.Linq;
using System.Text;
using PriSecDBAPI_SC_SDK.Model;
using Newtonsoft.Json;
using ASodium;
using System.IO;
using PriSecDBAPI_SC_SDK.Helper;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PriSecDBAPI_SC_SDK
{

    public static class Payment
    {        
        public static String[] CreatePaymentPage()
        {
            if(ApplicationPath.Path!=null && ApplicationPath.Path.CompareTo("")!=0 && ETLSPath.Path!=null && ETLSPath.Path.CompareTo("") != 0) 
            {
                String OrderID = "";
                String CheckOutPageUrl = "";
                if (GetPaymentCheckOutPage(ref OrderID, ref CheckOutPageUrl) == true)
                {
                    throw new Exception("Error:Unable to create payment page");
                }
                String[] PaymentPageArray = new String[2] { OrderID, CheckOutPageUrl };
                return PaymentPageArray;
            }
            else 
            {
                return new String[] { };
            }            
        }

        public static void VerifyMadePayment(String OrderID) 
        {
            if(ApplicationPath.Path != null && ApplicationPath.Path.CompareTo("") != 0 && ETLSPath.Path != null && ETLSPath.Path.CompareTo("") != 0) 
            {
                Boolean HasMadePayment = true;
                String ExceptionString = "";
                if (OrderID != null && OrderID.CompareTo("") != 0)
                {
                    HasMadePayment = VerifyPayment(OrderID,ref ExceptionString);
                    if (HasMadePayment == false)
                    {
                        if (ExceptionString.CompareTo("") != 0) 
                        {
                            throw new Exception(ExceptionString);
                        }
                        else 
                        {
                            throw new Exception("Error: Unable to verify made payment");
                        }
                    }
                }
                else
                {
                    throw new Exception("Error: OrderID must not be null or empty");
                }
            }
            else 
            {
                throw new ArgumentException("Error: ApplicationPath and ETLSPath must not be null/empty");
            }            
        }

        public static void VerifyMadeRenewPayment(String OrderID,Boolean RemainKeys = true) 
        {
            if (ApplicationPath.Path != null && ApplicationPath.Path.CompareTo("") != 0 && ETLSPath.Path != null && ETLSPath.Path.CompareTo("") != 0) 
            {
                Boolean HasMadePayment = true;
                String Base64RandomChallenge = "";
                String ExceptionString = "";
                if (OrderID != null && OrderID.CompareTo("") != 0)
                {
                    ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                    Byte[] RandomChallengeByte = new Byte[] { };
                    while (Base64RandomChallenge.CompareTo("") == 0)
                    {
                        Base64RandomChallenge = "";
                        ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                    }
                    RandomChallengeByte = Convert.FromBase64String(Base64RandomChallenge);
                    if (RemainKeys == true)
                    {
                        HasMadePayment = RemainKeysVerifyRenewedPayment(OrderID, RandomChallengeByte,ref ExceptionString);
                    }
                    else
                    {
                        HasMadePayment = VerifyRenewedPayment(OrderID, RandomChallengeByte,ref ExceptionString);
                    }
                    if (HasMadePayment == false)
                    {
                        if (ExceptionString.CompareTo("") != 0) 
                        {
                            throw new Exception(ExceptionString);
                        }
                        else 
                        {
                            throw new Exception("Error: Unable to verify renewed payment");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Error: OrderID must not be null/empty");
                }
            }
            else 
            {
                throw new ArgumentException("Error: ApplicationPath and ETLSPath must not be null/empty");
            }
        }

        private static Boolean GetPaymentCheckOutPage(ref String OrderID, ref String CheckOutPageUrl)
        {
            CheckOutPageHolderModel PageHolder = new CheckOutPageHolderModel();
            Boolean CheckServerBoolean = true;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync("CreateReceivePayment/");
                try
                {
                    response.Wait();
                }
                catch
                {
                    CheckServerBoolean = false;
                }
                if (CheckServerBoolean == true)
                {
                    var result = response.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var Result = readTask.Result;
                        if (Result != null && Result.CompareTo("") != 0 && Result.Contains("Error") == false)
                        {
                            PageHolder = JsonConvert.DeserializeObject<CheckOutPageHolderModel>(Result);
                            OrderID = PageHolder.PayPalOrderID;
                            CheckOutPageUrl = PageHolder.CheckOutPageUrl;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private static Boolean VerifyPayment(String OrderID, ref String ExceptionString)
        {
            Byte[] ClientED25519SK = new Byte[] { };
            Byte[] ClientX25519SK = new Byte[] { };
            Byte[] ClientX25519PK = new Byte[] { };
            Byte[] SharedSecret = new Byte[] { };
            Byte[] OrderIDByte = new Byte[] { };
            Byte[] NonceByte = new Byte[] { };
            Byte[] CipheredOrderIDByte = new Byte[] { };
            Byte[] CombinedCipheredOrderIDByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredOrderIDByte = new Byte[] { };
            Byte[] CipheredLoginED25519PK = new Byte[] { };
            Byte[] CombinedCipheredLoginED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredLoginED25519PK = new Byte[] { };
            Byte[] SignedLoginED25519PKByte = new Byte[] { };
            Byte[] CipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] SignedSealedDHPKByte = new Byte[] { };
            Byte[] CipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] CipheredSealedDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSealedDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSealedDHED25519PK = new Byte[] { };
            Byte[] SignedIKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] CipheredIKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredIKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredIKDHED25519PK = new Byte[] { };
            Byte[] SignedSPKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] CipheredSPKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSPKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSPKDHED25519PK = new Byte[] { };
            Byte[] SignedOPKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] CipheredOPKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredOPKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredOPKDHED25519PK = new Byte[] { };
            Byte[] EncryptedDatabaseNameByte = new Byte[] { };
            Byte[] DatabaseNameByte = new Byte[] { };
            String DatabaseName = "";
            Byte[] EncryptedDatabaseUserNameByte = new Byte[] { };
            Byte[] DatabaseUserNameByte = new Byte[] { };
            String DatabaseUserName = "";
            Byte[] EncryptedDatabaseUserPasswordByte = new Byte[] { };
            Byte[] DatabaseUserPasswordByte = new Byte[] { };
            String DatabaseUserPassword = "";
            Boolean CheckServerBoolean = true;
            RevampedKeyPair MyLoginKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair SignatureSealedDHKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair SealedDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair IKSignatureKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair IKDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair SPKSignatureKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair SPKDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair OPKSignatureKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair OPKDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            PaymentModel MyModel = new PaymentModel();
            String ETLSSessionID = "";
            if (ApplicationPath.IsWindows == true) 
            {
                ETLSSessionID = File.ReadAllText(ETLSPath.Path + "\\" + "SessionID.txt");
            }
            else 
            {
                ETLSSessionID = File.ReadAllText(ETLSPath.Path + "/" + "SessionID.txt");
            }
            if (OrderID != null && OrderID.CompareTo("") != 0)
            {
                if (ETLSSessionID != null && ETLSSessionID.CompareTo("") != 0)
                {
                    if (ApplicationPath.IsWindows == true) 
                    {
                        if (Directory.Exists(ApplicationPath.Path + "\\" + "SignatureStorage") == false)
                        {
                            Directory.CreateDirectory(ApplicationPath.Path + "\\" + "SignatureStorage");
                        }
                        if (Directory.Exists(ApplicationPath.Path + "\\" + "DHStorage") == false)
                        {
                            Directory.CreateDirectory(ApplicationPath.Path + "\\" + "DHStorage");
                        }
                    }
                    else 
                    {
                        if (Directory.Exists(ApplicationPath.Path + "/" + "SignatureStorage") == false)
                        {
                            Directory.CreateDirectory(ApplicationPath.Path + "/" + "SignatureStorage");
                        }
                        if (Directory.Exists(ApplicationPath.Path + "/" + "DHStorage") == false)
                        {
                            Directory.CreateDirectory(ApplicationPath.Path + "/" + "DHStorage");
                        }
                    }                    
                    if (ApplicationPath.IsWindows == true) 
                    {
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519SK.txt", MyLoginKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519PK.txt", MyLoginKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SignatureSealedDHED25519SK.txt", SignatureSealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SignatureSealedDHED25519PK.txt", SignatureSealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\IKSignatureED25519SK.txt", IKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\IKSignatureED25519PK.txt", IKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SPKSignatureED25519SK.txt", SPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SPKSignatureED25519PK.txt", SPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\OPKSignatureED25519SK.txt", OPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\OPKSignatureED25519PK.txt", OPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SealedDHX25519SK.txt", SealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SealedDHX25519PK.txt", SealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\IKX25519SK.txt", IKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\IKX25519PK.txt", IKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SPKX25519SK.txt", SPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SPKX25519PK.txt", SPKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\OPKX25519SK.txt", OPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\OPKX25519PK.txt", OPKDHKeyPair.PublicKey);
                        ClientED25519SK = File.ReadAllBytes(ETLSPath.Path + "\\" + ETLSSessionID + "\\" + "ECDSASK.txt");
                        SharedSecret = File.ReadAllBytes(ETLSPath.Path + "\\" + ETLSSessionID + "\\" + "SharedSecret.txt");
                    }
                    else 
                    {
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519SK.txt", MyLoginKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519PK.txt", MyLoginKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SignatureSealedDHED25519SK.txt", SignatureSealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SignatureSealedDHED25519PK.txt", SignatureSealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/IKSignatureED25519SK.txt", IKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/IKSignatureED25519PK.txt", IKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SPKSignatureED25519SK.txt", SPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SPKSignatureED25519PK.txt", SPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/OPKSignatureED25519SK.txt", OPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/OPKSignatureED25519PK.txt", OPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SealedDHX25519SK.txt", SealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SealedDHX25519PK.txt", SealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/IKX25519SK.txt", IKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/IKX25519PK.txt", IKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SPKX25519SK.txt", SPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SPKX25519PK.txt", SPKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/OPKX25519SK.txt", OPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/OPKX25519PK.txt", OPKDHKeyPair.PublicKey);
                        ClientED25519SK = File.ReadAllBytes(ETLSPath.Path + "/" + ETLSSessionID + "/" + "ECDSASK.txt");
                        SharedSecret = File.ReadAllBytes(ETLSPath.Path + "/" + ETLSSessionID + "/" + "SharedSecret.txt");
                    }                    
                    OrderIDByte = Encoding.UTF8.GetBytes(OrderID);
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredOrderIDByte = SodiumSecretBox.Create(OrderIDByte, NonceByte, SharedSecret);
                    CombinedCipheredOrderIDByte = NonceByte.Concat(CipheredOrderIDByte).ToArray();
                    ETLSSignedCombinedCipheredOrderIDByte = SodiumPublicKeyAuth.Sign(CombinedCipheredOrderIDByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredLoginED25519PK = SodiumSecretBox.Create(MyLoginKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredLoginED25519PK = NonceByte.Concat(CipheredLoginED25519PK).ToArray();
                    ETLSSignedCombinedCipheredLoginED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredLoginED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedLoginED25519PKByte = SodiumPublicKeyAuth.Sign(MyLoginKeyPair.PublicKey, MyLoginKeyPair.PrivateKey, true);
                    CipheredSignedLoginED25519PK = SodiumSecretBox.Create(SignedLoginED25519PKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedLoginED25519PK = NonceByte.Concat(CipheredSignedLoginED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSignedLoginED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedLoginED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedSealedDHPKByte = SodiumPublicKeyAuth.Sign(SealedDHKeyPair.PublicKey, SignatureSealedDHKeyPair.PrivateKey, true);
                    CipheredSignedSealedDHPKByte = SodiumSecretBox.Create(SignedSealedDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedSealedDHPKByte = NonceByte.Concat(CipheredSignedSealedDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedSealedDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedSealedDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredSealedDHED25519PK = SodiumSecretBox.Create(SignatureSealedDHKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredSealedDHED25519PK = NonceByte.Concat(CipheredSealedDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSealedDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSealedDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedIKDHPKByte = SodiumPublicKeyAuth.Sign(IKDHKeyPair.PublicKey, IKSignatureKeyPair.PrivateKey, true);
                    CipheredSignedIKDHPKByte = SodiumSecretBox.Create(SignedIKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedIKDHPKByte = NonceByte.Concat(CipheredSignedIKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedIKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedIKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredIKDHED25519PK = SodiumSecretBox.Create(IKSignatureKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredIKDHED25519PK = NonceByte.Concat(CipheredIKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredIKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredIKDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedSPKDHPKByte = SodiumPublicKeyAuth.Sign(SPKDHKeyPair.PublicKey, SPKSignatureKeyPair.PrivateKey, true);
                    CipheredSignedSPKDHPKByte = SodiumSecretBox.Create(SignedSPKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedSPKDHPKByte = NonceByte.Concat(CipheredSignedSPKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedSPKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedSPKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredSPKDHED25519PK = SodiumSecretBox.Create(SPKSignatureKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredSPKDHED25519PK = NonceByte.Concat(CipheredSPKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSPKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSPKDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedOPKDHPKByte = SodiumPublicKeyAuth.Sign(OPKDHKeyPair.PublicKey, OPKSignatureKeyPair.PrivateKey, true);
                    CipheredSignedOPKDHPKByte = SodiumSecretBox.Create(SignedOPKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedOPKDHPKByte = NonceByte.Concat(CipheredSignedOPKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedOPKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedOPKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredOPKDHED25519PK = SodiumSecretBox.Create(OPKSignatureKeyPair.PublicKey, NonceByte, SharedSecret, true);
                    CombinedCipheredOPKDHED25519PK = NonceByte.Concat(CipheredOPKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredOPKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredOPKDHED25519PK, ClientED25519SK, true);
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.GetAsync("CreateReceivePayment/CheckPayment?ClientPathID=" + ETLSSessionID +
                            "&CipheredSignedOrderID="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredOrderIDByte))
                            + "&CipheredSignedLoginED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredLoginED25519PK))
                            + "&EncryptedSignedSignedLoginED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedLoginED25519PK))
                            + "&CipheredSignedSignedSealedDHX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedSealedDHPKByte))
                            + "&CipheredSignedSealedDHED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSealedDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHSPKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedSPKDHPKByte))
                            + "&CipheredSignedSealedX3DHSPKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSPKDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHIKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedIKDHPKByte))
                            + "&CipheredSignedSealedX3DHIKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredIKDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHOPKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedOPKDHPKByte))
                            + "&CipheredSignedSealedX3DHOPKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredOPKDHED25519PK)));
                        try
                        {
                            response.Wait();
                        }
                        catch
                        {
                            CheckServerBoolean = false;
                        }
                        if (CheckServerBoolean == true)
                        {
                            var result = response.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                var readTask = result.Content.ReadAsStringAsync();
                                readTask.Wait();

                                var Result = readTask.Result;
                                if ((Result == null || Result.CompareTo("") == 0) || (Result.Contains("Error") == true))
                                {
                                    ExceptionString = Result;
                                    return false;
                                }
                                else
                                {
                                    MyModel = JsonConvert.DeserializeObject<PaymentModel>(Result);
                                    if (MyModel.Status != null && MyModel.Status.CompareTo("") != 0)
                                    {
                                        if (ApplicationPath.IsWindows == true) 
                                        {
                                            if (Directory.Exists(ApplicationPath.Path + "\\" + "DBCredentials") == false)
                                            {
                                                Directory.CreateDirectory(ApplicationPath.Path + "\\" + "DBCredentials");
                                            }
                                            ClientX25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + ETLSSessionID + "\\" + "ECDHSK.txt");
                                        }
                                        else 
                                        {
                                            if (Directory.Exists(ApplicationPath.Path + "/" + "DBCredentials") == false)
                                            {
                                                Directory.CreateDirectory(ApplicationPath.Path + "/" + "DBCredentials");
                                            }
                                            ClientX25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + ETLSSessionID + "/" + "ECDHSK.txt");
                                        }                                        
                                        ClientX25519PK = SodiumScalarMult.Base(ClientX25519SK);
                                        EncryptedDatabaseNameByte = Convert.FromBase64String(MyModel.CipheredDBName);
                                        EncryptedDatabaseUserNameByte = Convert.FromBase64String(MyModel.CipheredDBAccountUserName);
                                        EncryptedDatabaseUserPasswordByte = Convert.FromBase64String(MyModel.CipheredDBAccountPassword);
                                        DatabaseNameByte = SodiumSealedPublicKeyBox.Open(EncryptedDatabaseNameByte, ClientX25519PK, ClientX25519SK);
                                        DatabaseUserNameByte = SodiumSealedPublicKeyBox.Open(EncryptedDatabaseUserNameByte, ClientX25519PK, ClientX25519SK);
                                        DatabaseUserPasswordByte = SodiumSealedPublicKeyBox.Open(EncryptedDatabaseUserPasswordByte, ClientX25519PK, ClientX25519SK, true);
                                        DatabaseName = Encoding.UTF8.GetString(DatabaseNameByte);
                                        DatabaseUserName = Encoding.UTF8.GetString(DatabaseUserNameByte);
                                        DatabaseUserPassword = Encoding.UTF8.GetString(DatabaseUserPasswordByte);
                                        if (ApplicationPath.IsWindows == true) 
                                        {
                                            File.WriteAllText(ApplicationPath.Path + "\\" + "DBCredentials\\PaymentID.txt", MyModel.SystemPaymentID);
                                            File.WriteAllText(ApplicationPath.Path + "\\" + "DBCredentials\\DBName.txt", DatabaseName);
                                            File.WriteAllText(ApplicationPath.Path + "\\" + "DBCredentials\\DBUserName.txt", DatabaseUserName);
                                            File.WriteAllText(ApplicationPath.Path + "\\" + "DBCredentials\\DBUserPassword.txt", DatabaseUserPassword);
                                            File.WriteAllBytes(ApplicationPath.Path + "\\" + "DBCredentials\\DBNameBytes.txt", DatabaseNameByte);
                                            File.WriteAllBytes(ApplicationPath.Path + "\\" + "DBCredentials\\DBUserNameBytes.txt", DatabaseUserNameByte);
                                            File.WriteAllBytes(ApplicationPath.Path + "\\" + "DBCredentials\\DBUserPasswordBytes.txt", DatabaseUserPasswordByte);
                                        }
                                        else 
                                        {
                                            File.WriteAllText(ApplicationPath.Path + "/" + "DBCredentials/PaymentID.txt", MyModel.SystemPaymentID);
                                            File.WriteAllText(ApplicationPath.Path + "/" + "DBCredentials/DBName.txt", DatabaseName);
                                            File.WriteAllText(ApplicationPath.Path + "/" + "DBCredentials/DBUserName.txt", DatabaseUserName);
                                            File.WriteAllText(ApplicationPath.Path + "/" + "DBCredentials/DBUserPassword.txt", DatabaseUserPassword);
                                            File.WriteAllBytes(ApplicationPath.Path + "/" + "DBCredentials/DBNameBytes.txt", DatabaseNameByte);
                                            File.WriteAllBytes(ApplicationPath.Path + "/" + "DBCredentials/DBUserNameBytes.txt", DatabaseUserNameByte);
                                            File.WriteAllBytes(ApplicationPath.Path + "/" + "DBCredentials/DBUserPasswordBytes.txt", DatabaseUserPasswordByte);
                                        }                                        
                                        MyLoginKeyPair.Clear();
                                        SignatureSealedDHKeyPair.Clear();
                                        SealedDHKeyPair.Clear();
                                        IKSignatureKeyPair.Clear();
                                        IKDHKeyPair.Clear();
                                        SPKSignatureKeyPair.Clear();
                                        SPKDHKeyPair.Clear();
                                        OPKSignatureKeyPair.Clear();
                                        OPKDHKeyPair.Clear();
                                        return true;
                                    }
                                    else
                                    {
                                        SodiumSecureMemory.SecureClearBytes(ClientED25519SK);
                                        SodiumSecureMemory.SecureClearBytes(ClientX25519SK);
                                        MyLoginKeyPair.Clear();
                                        SignatureSealedDHKeyPair.Clear();
                                        SealedDHKeyPair.Clear();
                                        IKSignatureKeyPair.Clear();
                                        IKDHKeyPair.Clear();
                                        SPKSignatureKeyPair.Clear();
                                        SPKDHKeyPair.Clear();
                                        OPKSignatureKeyPair.Clear();
                                        OPKDHKeyPair.Clear();
                                        ExceptionString = "Error: Unable to find status in the converted JSON data";
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                ExceptionString = "Error: Unable to fetch values from server";
                                SodiumSecureMemory.SecureClearBytes(ClientED25519SK);
                                SodiumSecureMemory.SecureClearBytes(ClientX25519SK);
                                MyLoginKeyPair.Clear();
                                SignatureSealedDHKeyPair.Clear();
                                SealedDHKeyPair.Clear();
                                IKSignatureKeyPair.Clear();
                                IKDHKeyPair.Clear();
                                SPKSignatureKeyPair.Clear();
                                SPKDHKeyPair.Clear();
                                OPKSignatureKeyPair.Clear();
                                OPKDHKeyPair.Clear();
                                return false;
                            }
                        }
                        else
                        {
                            ExceptionString = "Error: Server was offline";
                            SodiumSecureMemory.SecureClearBytes(ClientED25519SK);
                            SodiumSecureMemory.SecureClearBytes(ClientX25519SK);
                            MyLoginKeyPair.Clear();
                            SignatureSealedDHKeyPair.Clear();
                            SealedDHKeyPair.Clear();
                            IKSignatureKeyPair.Clear();
                            IKDHKeyPair.Clear();
                            SPKSignatureKeyPair.Clear();
                            SPKDHKeyPair.Clear();
                            OPKSignatureKeyPair.Clear();
                            OPKDHKeyPair.Clear();
                            return false;
                        }
                    }
                }
                else
                {
                    ExceptionString = "Error: You have not yet establish an ETLS session with server";
                    SodiumSecureMemory.SecureClearBytes(ClientED25519SK);
                    SodiumSecureMemory.SecureClearBytes(ClientX25519SK);
                    MyLoginKeyPair.Clear();
                    SignatureSealedDHKeyPair.Clear();
                    SealedDHKeyPair.Clear();
                    IKSignatureKeyPair.Clear();
                    IKDHKeyPair.Clear();
                    SPKSignatureKeyPair.Clear();
                    SPKDHKeyPair.Clear();
                    OPKSignatureKeyPair.Clear();
                    OPKDHKeyPair.Clear();
                    return false;
                }
            }
            else
            {
                ExceptionString = "Error: OrderID must not be empty/null";
                SodiumSecureMemory.SecureClearBytes(ClientED25519SK);
                SodiumSecureMemory.SecureClearBytes(ClientX25519SK);
                MyLoginKeyPair.Clear();
                SignatureSealedDHKeyPair.Clear();
                SealedDHKeyPair.Clear();
                IKSignatureKeyPair.Clear();
                IKDHKeyPair.Clear();
                SPKSignatureKeyPair.Clear();
                SPKDHKeyPair.Clear();
                OPKSignatureKeyPair.Clear();
                OPKDHKeyPair.Clear();
                return false;
            }
        }

        private static Boolean VerifyRenewedPayment(String OrderID, Byte[] RandomChallenge, ref String ExceptionString)
        {
            String UniquePaymentID = "";
            Byte[] UniquePaymentIDByte = new Byte[] { };
            Byte[] CipheredUniquePaymentIDByte = new Byte[] { };
            Byte[] CombinedCipheredUniquePaymentIDByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredUniquePaymentIDByte = new Byte[] { };
            Byte[] ClientED25519SK = new Byte[] { };
            Byte[] ClientLoginED25519SK = new Byte[] { };
            Byte[] SharedSecret = new Byte[] { };
            Byte[] OrderIDByte = new Byte[] { };
            Byte[] NonceByte = new Byte[] { };
            Byte[] CipheredOrderIDByte = new Byte[] { };
            Byte[] CombinedCipheredOrderIDByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredOrderIDByte = new Byte[] { };
            Byte[] CipheredLoginED25519PK = new Byte[] { };
            Byte[] CombinedCipheredLoginED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredLoginED25519PK = new Byte[] { };
            Byte[] SignedLoginED25519PKByte = new Byte[] { };
            Byte[] CipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] SignedSealedDHPKByte = new Byte[] { };
            Byte[] CipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] CipheredSealedDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSealedDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSealedDHED25519PK = new Byte[] { };
            Byte[] SignedIKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] CipheredIKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredIKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredIKDHED25519PK = new Byte[] { };
            Byte[] SignedSPKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] CipheredSPKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSPKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSPKDHED25519PK = new Byte[] { };
            Byte[] SignedOPKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] CipheredOPKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredOPKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredOPKDHED25519PK = new Byte[] { };
            Byte[] SignedRandomChallenge = new Byte[] { };
            Byte[] ETLSSignedSignedRandomChallenge = new Byte[] { };
            Boolean CheckServerBoolean = true;
            RevampedKeyPair MyLoginKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair SignatureSealedDHKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair SealedDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair IKSignatureKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair IKDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair SPKSignatureKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair SPKDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair OPKSignatureKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            RevampedKeyPair OPKDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            String ETLSSessionID = "";
            if (ApplicationPath.IsWindows == true)
            {
                ETLSSessionID = File.ReadAllText(ETLSPath.Path + "\\" + "SessionID.txt");
            }
            else
            {
                ETLSSessionID = File.ReadAllText(ETLSPath.Path + "/" + "SessionID.txt");
            }
            if (OrderID != null && OrderID.CompareTo("") != 0)
            {
                if (ETLSSessionID != null && ETLSSessionID.CompareTo("") != 0)
                {
                    if (ApplicationPath.IsWindows == true) 
                    {
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SignatureSealedDHED25519SK.txt", SignatureSealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SignatureSealedDHED25519PK.txt", SignatureSealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\IKSignatureED25519SK.txt", IKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\IKSignatureED25519PK.txt", IKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SPKSignatureED25519SK.txt", SPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SPKSignatureED25519PK.txt", SPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\OPKSignatureED25519SK.txt", OPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\OPKSignatureED25519PK.txt", OPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SealedDHX25519SK.txt", SealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SealedDHX25519PK.txt", SealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\IKX25519SK.txt", IKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\IKX25519PK.txt", IKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SPKX25519SK.txt", SPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SPKX25519PK.txt", SPKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\OPKX25519SK.txt", OPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\OPKX25519PK.txt", OPKDHKeyPair.PublicKey);
                        SharedSecret = File.ReadAllBytes(ETLSPath.Path + "\\" + ETLSSessionID + "\\" + "SharedSecret.txt");
                        ClientED25519SK = File.ReadAllBytes(ETLSPath.Path + "\\" + ETLSSessionID + "\\" + "ECDSASK.txt");
                    }
                    else 
                    {
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SignatureSealedDHED25519SK.txt", SignatureSealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SignatureSealedDHED25519PK.txt", SignatureSealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/IKSignatureED25519SK.txt", IKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/IKSignatureED25519PK.txt", IKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SPKSignatureED25519SK.txt", SPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SPKSignatureED25519PK.txt", SPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/OPKSignatureED25519SK.txt", OPKSignatureKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/OPKSignatureED25519PK.txt", OPKSignatureKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SealedDHX25519SK.txt", SealedDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SealedDHX25519PK.txt", SealedDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/IKX25519SK.txt", IKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/IKX25519PK.txt", IKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SPKX25519SK.txt", SPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/SPKX25519PK.txt", SPKDHKeyPair.PublicKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/OPKX25519SK.txt", OPKDHKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "DHStorage/OPKX25519PK.txt", OPKDHKeyPair.PublicKey);
                        SharedSecret = File.ReadAllBytes(ETLSPath.Path + "/" + ETLSSessionID + "/" + "SharedSecret.txt");
                        ClientED25519SK = File.ReadAllBytes(ETLSPath.Path + "/" + ETLSSessionID + "/" + "ECDSASK.txt");
                    }
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    if (ApplicationPath.IsWindows == true) 
                    {
                        UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "\\" + "DBCredentials\\PaymentID.txt");
                    }
                    else 
                    {
                        UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "/" + "DBCredentials/PaymentID.txt");
                    }
                    UniquePaymentIDByte = Encoding.UTF8.GetBytes(UniquePaymentID);
                    CipheredUniquePaymentIDByte = SodiumSecretBox.Create(UniquePaymentIDByte, NonceByte, SharedSecret);
                    CombinedCipheredUniquePaymentIDByte = NonceByte.Concat(CipheredUniquePaymentIDByte).ToArray();
                    ETLSSignedCombinedCipheredUniquePaymentIDByte = SodiumPublicKeyAuth.Sign(CombinedCipheredUniquePaymentIDByte, ClientED25519SK);
                    if (ApplicationPath.IsWindows == true) 
                    {
                        ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519SK.txt");
                    }
                    else 
                    {
                        ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519SK.txt");
                    }
                    SignedRandomChallenge = SodiumPublicKeyAuth.Sign(RandomChallenge, ClientLoginED25519SK, true);
                    ETLSSignedSignedRandomChallenge = SodiumPublicKeyAuth.Sign(SignedRandomChallenge, ClientED25519SK);
                    if (ApplicationPath.IsWindows == true)
                    {
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519SK.txt", MyLoginKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519PK.txt", MyLoginKeyPair.PublicKey);
                    }
                    else
                    {
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519SK.txt", MyLoginKeyPair.PrivateKey);
                        File.WriteAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519PK.txt", MyLoginKeyPair.PublicKey);
                    }
                    OrderIDByte = Encoding.UTF8.GetBytes(OrderID);
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredOrderIDByte = SodiumSecretBox.Create(OrderIDByte, NonceByte, SharedSecret);
                    CombinedCipheredOrderIDByte = NonceByte.Concat(CipheredOrderIDByte).ToArray();
                    ETLSSignedCombinedCipheredOrderIDByte = SodiumPublicKeyAuth.Sign(CombinedCipheredOrderIDByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredLoginED25519PK = SodiumSecretBox.Create(MyLoginKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredLoginED25519PK = NonceByte.Concat(CipheredLoginED25519PK).ToArray();
                    ETLSSignedCombinedCipheredLoginED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredLoginED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedLoginED25519PKByte = SodiumPublicKeyAuth.Sign(MyLoginKeyPair.PublicKey, MyLoginKeyPair.PrivateKey);
                    CipheredSignedLoginED25519PK = SodiumSecretBox.Create(SignedLoginED25519PKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedLoginED25519PK = NonceByte.Concat(CipheredSignedLoginED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSignedLoginED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedLoginED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedSealedDHPKByte = SodiumPublicKeyAuth.Sign(SealedDHKeyPair.PublicKey, SignatureSealedDHKeyPair.PrivateKey);
                    CipheredSignedSealedDHPKByte = SodiumSecretBox.Create(SignedSealedDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedSealedDHPKByte = NonceByte.Concat(CipheredSignedSealedDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedSealedDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedSealedDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredSealedDHED25519PK = SodiumSecretBox.Create(SignatureSealedDHKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredSealedDHED25519PK = NonceByte.Concat(CipheredSealedDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSealedDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSealedDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedIKDHPKByte = SodiumPublicKeyAuth.Sign(IKDHKeyPair.PublicKey, IKSignatureKeyPair.PrivateKey, true);
                    CipheredSignedIKDHPKByte = SodiumSecretBox.Create(SignedIKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedIKDHPKByte = NonceByte.Concat(CipheredSignedIKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedIKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedIKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredIKDHED25519PK = SodiumSecretBox.Create(IKSignatureKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredIKDHED25519PK = NonceByte.Concat(CipheredIKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredIKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredIKDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedSPKDHPKByte = SodiumPublicKeyAuth.Sign(SPKDHKeyPair.PublicKey, SPKSignatureKeyPair.PrivateKey, true);
                    CipheredSignedSPKDHPKByte = SodiumSecretBox.Create(SignedSPKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedSPKDHPKByte = NonceByte.Concat(CipheredSignedSPKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedSPKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedSPKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredSPKDHED25519PK = SodiumSecretBox.Create(SPKSignatureKeyPair.PublicKey, NonceByte, SharedSecret);
                    CombinedCipheredSPKDHED25519PK = NonceByte.Concat(CipheredSPKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSPKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSPKDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedOPKDHPKByte = SodiumPublicKeyAuth.Sign(OPKDHKeyPair.PublicKey, OPKSignatureKeyPair.PrivateKey, true);
                    CipheredSignedOPKDHPKByte = SodiumSecretBox.Create(SignedOPKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedOPKDHPKByte = NonceByte.Concat(CipheredSignedOPKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedOPKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedOPKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredOPKDHED25519PK = SodiumSecretBox.Create(OPKSignatureKeyPair.PublicKey, NonceByte, SharedSecret, true);
                    CombinedCipheredOPKDHED25519PK = NonceByte.Concat(CipheredOPKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredOPKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredOPKDHED25519PK, ClientED25519SK, true);
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.GetAsync("CreateReceivePayment/RenewPayment?ClientPathID=" + ETLSSessionID +
                            "&CipheredSignedOrderID="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredOrderIDByte))
                            + "&CipheredSignedUniquePaymentID="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredUniquePaymentIDByte))
                            + "&SignedSignedRandomChallenge="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedSignedRandomChallenge))
                            + "&CipheredSignedLoginED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredLoginED25519PK))
                            + "&EncryptedSignedSignedLoginED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedLoginED25519PK))
                            + "&CipheredSignedSignedSealedDHX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedSealedDHPKByte))
                            + "&CipheredSignedSealedDHED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSealedDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHSPKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedSPKDHPKByte))
                            + "&CipheredSignedSealedX3DHSPKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSPKDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHIKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedIKDHPKByte))
                            + "&CipheredSignedSealedX3DHIKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredIKDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHOPKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedOPKDHPKByte))
                            + "&CipheredSignedSealedX3DHOPKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredOPKDHED25519PK)));
                        try
                        {
                            response.Wait();
                        }
                        catch
                        {
                            CheckServerBoolean = false;
                        }
                        if (CheckServerBoolean == true)
                        {
                            var result = response.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                var readTask = result.Content.ReadAsStringAsync();
                                readTask.Wait();

                                var Result = readTask.Result;
                                if ((Result == null || Result.CompareTo("") == 0) || (Result.Contains("Error") == true))
                                {
                                    ExceptionString = Result;
                                    return false;
                                }
                                else
                                {
                                    Result = Result.Substring(1, Result.Length - 2);
                                    MyLoginKeyPair.Clear();
                                    SignatureSealedDHKeyPair.Clear();
                                    SealedDHKeyPair.Clear();
                                    IKSignatureKeyPair.Clear();
                                    IKDHKeyPair.Clear();
                                    SPKSignatureKeyPair.Clear();
                                    SPKDHKeyPair.Clear();
                                    OPKSignatureKeyPair.Clear();
                                    OPKDHKeyPair.Clear();
                                    return true;
                                }
                            }
                            else
                            {
                                ExceptionString = "Error: Unable to fetch values from server";
                                MyLoginKeyPair.Clear();
                                SignatureSealedDHKeyPair.Clear();
                                SealedDHKeyPair.Clear();
                                IKSignatureKeyPair.Clear();
                                IKDHKeyPair.Clear();
                                SPKSignatureKeyPair.Clear();
                                SPKDHKeyPair.Clear();
                                OPKSignatureKeyPair.Clear();
                                OPKDHKeyPair.Clear();
                                return false;
                            }
                        }
                        else
                        {
                            ExceptionString = "Error: Server was offline";
                            MyLoginKeyPair.Clear();
                            SignatureSealedDHKeyPair.Clear();
                            SealedDHKeyPair.Clear();
                            IKSignatureKeyPair.Clear();
                            IKDHKeyPair.Clear();
                            SPKSignatureKeyPair.Clear();
                            SPKDHKeyPair.Clear();
                            OPKSignatureKeyPair.Clear();
                            OPKDHKeyPair.Clear();
                            return false;
                        }
                    }
                }
                else
                {
                    ExceptionString = "Error: You have not yet establish an ETLS session with server";
                    MyLoginKeyPair.Clear();
                    SignatureSealedDHKeyPair.Clear();
                    SealedDHKeyPair.Clear();
                    IKSignatureKeyPair.Clear();
                    IKDHKeyPair.Clear();
                    SPKSignatureKeyPair.Clear();
                    SPKDHKeyPair.Clear();
                    OPKSignatureKeyPair.Clear();
                    OPKDHKeyPair.Clear();
                    return false;
                }
            }
            else
            {
                ExceptionString = "Error: OrderID must not be null/empty";
                MyLoginKeyPair.Clear();
                SignatureSealedDHKeyPair.Clear();
                SealedDHKeyPair.Clear();
                IKSignatureKeyPair.Clear();
                IKDHKeyPair.Clear();
                SPKSignatureKeyPair.Clear();
                SPKDHKeyPair.Clear();
                OPKSignatureKeyPair.Clear();
                OPKDHKeyPair.Clear();
                return false;
            }
        }

        private static Boolean RemainKeysVerifyRenewedPayment(String OrderID, Byte[] RandomChallenge,ref String ExceptionString)
        {
            String UniquePaymentID = "";
            Byte[] UniquePaymentIDByte = new Byte[] { };
            Byte[] CipheredUniquePaymentIDByte = new Byte[] { };
            Byte[] CombinedCipheredUniquePaymentIDByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredUniquePaymentIDByte = new Byte[] { };
            Byte[] ClientED25519SK = new Byte[] { };
            Byte[] ClientLoginED25519SK = new Byte[] { };
            Byte[] SharedSecret = new Byte[] { };
            Byte[] OrderIDByte = new Byte[] { };
            Byte[] NonceByte = new Byte[] { };
            Byte[] CipheredOrderIDByte = new Byte[] { };
            Byte[] CombinedCipheredOrderIDByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredOrderIDByte = new Byte[] { };
            Byte[] CipheredLoginED25519PK = new Byte[] { };
            Byte[] CombinedCipheredLoginED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredLoginED25519PK = new Byte[] { };
            Byte[] SignedLoginED25519PKByte = new Byte[] { };
            Byte[] CipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedLoginED25519PK = new Byte[] { };
            Byte[] SignedSealedDHPKByte = new Byte[] { };
            Byte[] CipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedSealedDHPKByte = new Byte[] { };
            Byte[] CipheredSealedDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSealedDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSealedDHED25519PK = new Byte[] { };
            Byte[] SignedIKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedIKDHPKByte = new Byte[] { };
            Byte[] CipheredIKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredIKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredIKDHED25519PK = new Byte[] { };
            Byte[] SignedSPKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedSPKDHPKByte = new Byte[] { };
            Byte[] CipheredSPKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredSPKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSPKDHED25519PK = new Byte[] { };
            Byte[] SignedOPKDHPKByte = new Byte[] { };
            Byte[] CipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] CombinedCipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredSignedOPKDHPKByte = new Byte[] { };
            Byte[] CipheredOPKDHED25519PK = new Byte[] { };
            Byte[] CombinedCipheredOPKDHED25519PK = new Byte[] { };
            Byte[] ETLSSignedCombinedCipheredOPKDHED25519PK = new Byte[] { };
            Byte[] SignedRandomChallenge = new Byte[] { };
            Byte[] ETLSSignedSignedRandomChallenge = new Byte[] { };
            Byte[] SealedDHED25519SK = new Byte[] { };
            Byte[] SealedDHED25519PK = new Byte[] { };
            Byte[] IKDHED25519SK = new Byte[] { };
            Byte[] IKDHED25519PK = new Byte[] { };
            Byte[] SPKDHED25519SK = new Byte[] { };
            Byte[] SPKDHED25519PK = new Byte[] { };
            Byte[] OPKDHED25519SK = new Byte[] { };
            Byte[] OPKDHED25519PK = new Byte[] { };
            Byte[] SealedDHX25519SK = new Byte[] { };
            Byte[] SealedDHX25519PK = new Byte[] { };
            Byte[] IKX25519SK = new Byte[] { };
            Byte[] IKX25519PK = new Byte[] { };
            Byte[] SPKX25519SK = new Byte[] { };
            Byte[] SPKX25519PK = new Byte[] { };
            Byte[] OPKX25519SK = new Byte[] { };
            Byte[] OPKX25519PK = new Byte[] { };
            Byte[] ClientLoginED25519PK = new Byte[] { };
            Boolean CheckServerBoolean = true;
            String ETLSSessionID = "";
            if (ApplicationPath.IsWindows == true)
            {
                ETLSSessionID = File.ReadAllText(ETLSPath.Path + "\\" + "SessionID.txt");
            }
            else
            {
                ETLSSessionID = File.ReadAllText(ETLSPath.Path + "/" + "SessionID.txt");
            }
            if (OrderID != null && OrderID.CompareTo("") != 0)
            {
                if (ETLSSessionID != null && ETLSSessionID.CompareTo("") != 0)
                {
                    if (ApplicationPath.IsWindows==true) 
                    {
                        SealedDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SignatureSealedDHED25519SK.txt");
                        SealedDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SignatureSealedDHED25519PK.txt");
                        IKDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\IKSignatureED25519SK.txt");
                        IKDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\IKSignatureED25519PK.txt");
                        SPKDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SPKSignatureED25519SK.txt");
                        SPKDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\SPKSignatureED25519PK.txt");
                        OPKDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\OPKSignatureED25519SK.txt");
                        OPKDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\OPKSignatureED25519PK.txt");
                        SealedDHX25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SealedDHX25519PK.txt");
                        IKX25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\IKX25519PK.txt");
                        SPKX25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\SPKX25519PK.txt");
                        OPKX25519PK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "DHStorage\\OPKX25519PK.txt");
                        SharedSecret = File.ReadAllBytes(ETLSPath.Path + "\\" + ETLSSessionID + "\\" + "SharedSecret.txt");
                        ClientED25519SK = File.ReadAllBytes(ETLSPath.Path + "\\" + ETLSSessionID + "\\" + "ECDSASK.txt");
                    }
                    else 
                    {
                        SealedDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SignatureSealedDHED25519SK.txt");
                        SealedDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SignatureSealedDHED25519PK.txt");
                        IKDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/IKSignatureED25519SK.txt");
                        IKDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/IKSignatureED25519PK.txt");
                        SPKDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SPKSignatureED25519SK.txt");
                        SPKDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/SPKSignatureED25519PK.txt");
                        OPKDHED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/OPKSignatureED25519SK.txt");
                        OPKDHED25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/OPKSignatureED25519PK.txt");
                        SealedDHX25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "DHStorage/SealedDHX25519PK.txt");
                        IKX25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "DHStorage/IKX25519PK.txt");
                        SPKX25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "DHStorage/SPKX25519PK.txt");
                        OPKX25519PK = File.ReadAllBytes(ApplicationPath.Path + "/" + "DHStorage/OPKX25519PK.txt");
                        SharedSecret = File.ReadAllBytes(ETLSPath.Path + "/" + ETLSSessionID + "/" + "SharedSecret.txt");
                        ClientED25519SK = File.ReadAllBytes(ETLSPath.Path + "/" + ETLSSessionID + "/" + "ECDSASK.txt");
                    }                    
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    if (ApplicationPath.IsWindows == true) 
                    {
                        UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "\\" + "DBCredentials\\PaymentID.txt");
                    }
                    else 
                    {
                        UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "/" + "DBCredentials/PaymentID.txt");
                    }
                    UniquePaymentIDByte = Encoding.UTF8.GetBytes(UniquePaymentID);
                    CipheredUniquePaymentIDByte = SodiumSecretBox.Create(UniquePaymentIDByte, NonceByte, SharedSecret);
                    CombinedCipheredUniquePaymentIDByte = NonceByte.Concat(CipheredUniquePaymentIDByte).ToArray();
                    ETLSSignedCombinedCipheredUniquePaymentIDByte = SodiumPublicKeyAuth.Sign(CombinedCipheredUniquePaymentIDByte, ClientED25519SK);
                    if (ApplicationPath.IsWindows == true) 
                    {
                        ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519SK.txt");
                    }
                    else 
                    {
                        ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519SK.txt");
                    }
                    ClientLoginED25519PK = SodiumPublicKeyAuth.GeneratePublicKey(ClientLoginED25519SK);
                    SignedRandomChallenge = SodiumPublicKeyAuth.Sign(RandomChallenge, ClientLoginED25519SK);
                    ETLSSignedSignedRandomChallenge = SodiumPublicKeyAuth.Sign(SignedRandomChallenge, ClientED25519SK);
                    OrderIDByte = Encoding.UTF8.GetBytes(OrderID);
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredOrderIDByte = SodiumSecretBox.Create(OrderIDByte, NonceByte, SharedSecret);
                    CombinedCipheredOrderIDByte = NonceByte.Concat(CipheredOrderIDByte).ToArray();
                    ETLSSignedCombinedCipheredOrderIDByte = SodiumPublicKeyAuth.Sign(CombinedCipheredOrderIDByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredLoginED25519PK = SodiumSecretBox.Create(ClientLoginED25519PK, NonceByte, SharedSecret);
                    CombinedCipheredLoginED25519PK = NonceByte.Concat(CipheredLoginED25519PK).ToArray();
                    ETLSSignedCombinedCipheredLoginED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredLoginED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedLoginED25519PKByte = SodiumPublicKeyAuth.Sign(ClientLoginED25519PK, ClientLoginED25519SK, true);
                    CipheredSignedLoginED25519PK = SodiumSecretBox.Create(SignedLoginED25519PKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedLoginED25519PK = NonceByte.Concat(CipheredSignedLoginED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSignedLoginED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedLoginED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedSealedDHPKByte = SodiumPublicKeyAuth.Sign(SealedDHX25519PK, SealedDHED25519SK, true);
                    CipheredSignedSealedDHPKByte = SodiumSecretBox.Create(SignedSealedDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedSealedDHPKByte = NonceByte.Concat(CipheredSignedSealedDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedSealedDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedSealedDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredSealedDHED25519PK = SodiumSecretBox.Create(SealedDHED25519PK, NonceByte, SharedSecret);
                    CombinedCipheredSealedDHED25519PK = NonceByte.Concat(CipheredSealedDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSealedDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSealedDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedIKDHPKByte = SodiumPublicKeyAuth.Sign(IKX25519PK, IKDHED25519SK, true);
                    CipheredSignedIKDHPKByte = SodiumSecretBox.Create(SignedIKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedIKDHPKByte = NonceByte.Concat(CipheredSignedIKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedIKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedIKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredIKDHED25519PK = SodiumSecretBox.Create(IKDHED25519PK, NonceByte, SharedSecret);
                    CombinedCipheredIKDHED25519PK = NonceByte.Concat(CipheredIKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredIKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredIKDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedSPKDHPKByte = SodiumPublicKeyAuth.Sign(SPKX25519PK, SPKDHED25519SK, true);
                    CipheredSignedSPKDHPKByte = SodiumSecretBox.Create(SignedSPKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedSPKDHPKByte = NonceByte.Concat(CipheredSignedSPKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedSPKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedSPKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredSPKDHED25519PK = SodiumSecretBox.Create(SPKDHED25519PK, NonceByte, SharedSecret);
                    CombinedCipheredSPKDHED25519PK = NonceByte.Concat(CipheredSPKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredSPKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredSPKDHED25519PK, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    SignedOPKDHPKByte = SodiumPublicKeyAuth.Sign(OPKX25519PK, OPKDHED25519SK, true);
                    CipheredSignedOPKDHPKByte = SodiumSecretBox.Create(SignedOPKDHPKByte, NonceByte, SharedSecret);
                    CombinedCipheredSignedOPKDHPKByte = NonceByte.Concat(CipheredSignedOPKDHPKByte).ToArray();
                    ETLSSignedCombinedCipheredSignedOPKDHPKByte = SodiumPublicKeyAuth.Sign(CombinedCipheredSignedOPKDHPKByte, ClientED25519SK);
                    NonceByte = new Byte[] { };
                    NonceByte = SodiumSecretBox.GenerateNonce();
                    CipheredOPKDHED25519PK = SodiumSecretBox.Create(OPKDHED25519PK, NonceByte, SharedSecret, true);
                    CombinedCipheredOPKDHED25519PK = NonceByte.Concat(CipheredOPKDHED25519PK).ToArray();
                    ETLSSignedCombinedCipheredOPKDHED25519PK = SodiumPublicKeyAuth.Sign(CombinedCipheredOPKDHED25519PK, ClientED25519SK, true);
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.GetAsync("CreateReceivePayment/RenewPayment?ClientPathID=" + ETLSSessionID +
                            "&CipheredSignedOrderID="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredOrderIDByte))
                            + "&CipheredSignedUniquePaymentID="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredUniquePaymentIDByte))
                            + "&SignedSignedRandomChallenge="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedSignedRandomChallenge))
                            + "&CipheredSignedLoginED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredLoginED25519PK))
                            + "&EncryptedSignedSignedLoginED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedLoginED25519PK))
                            + "&CipheredSignedSignedSealedDHX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedSealedDHPKByte))
                            + "&CipheredSignedSealedDHED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSealedDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHSPKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedSPKDHPKByte))
                            + "&CipheredSignedSealedX3DHSPKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSPKDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHIKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedIKDHPKByte))
                            + "&CipheredSignedSealedX3DHIKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredIKDHED25519PK))
                            + "&CipheredSignedSignedSealedX3DHOPKX25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredSignedOPKDHPKByte))
                            + "&CipheredSignedSealedX3DHOPKED25519PK="
                            + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ETLSSignedCombinedCipheredOPKDHED25519PK)));
                        try
                        {
                            response.Wait();
                        }
                        catch
                        {
                            CheckServerBoolean = false;
                        }
                        if (CheckServerBoolean == true)
                        {
                            var result = response.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                var readTask = result.Content.ReadAsStringAsync();
                                readTask.Wait();

                                var Result = readTask.Result;
                                if ((Result == null || Result.CompareTo("") == 0) || (Result.Contains("Error") == true))
                                {
                                    ExceptionString = Result;
                                    return false;
                                }
                                else
                                {
                                    Result = Result.Substring(1, Result.Length - 2);
                                    return true;
                                }
                            }
                            else
                            {
                                ExceptionString = "Error: Unable to fetch values from server";
                                return false;
                            }
                        }
                        else
                        {
                            ExceptionString = "Error: Server was offline";
                            return false;
                        }
                    }
                }
                else
                {
                    ExceptionString = "Error: You have not yet establish an ETLS session with server";
                    return false;
                }
            }
            else
            {
                ExceptionString = "Error: OrderID must not be null/empty";
                return false;
            }
        }
    }
}
