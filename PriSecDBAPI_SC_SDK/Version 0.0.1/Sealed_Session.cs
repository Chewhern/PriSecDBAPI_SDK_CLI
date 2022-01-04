using System;
using PriSecDBAPI_SC_SDK.Model;
using Newtonsoft.Json;
using ASodium;
using System.IO;
using PriSecDBAPI_SC_SDK.Helper;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PriSecDBAPI_SC_SDK
{

    public static class Sealed_Session
    {
        private static SecureIDGenerator MySecureIDGenerator = new SecureIDGenerator();

        public static void DeleteSealedCredentials() 
        {
            if (ApplicationPath.Path != null && ApplicationPath.Path.CompareTo("") != 0) 
            {
                if (ApplicationPath.IsWindows == true) 
                {
                    if (Directory.Exists(ApplicationPath.Path + "\\SealedCredentials") == true && Directory.Exists(ApplicationPath.Path + "\\" + "DBCredentials") == true)
                    {
                        String Base64RandomChallenge = "";
                        ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                        DeleteCredentials(Base64RandomChallenge);
                    }
                }
                else 
                {
                    if (Directory.Exists(ApplicationPath.Path + "/SealedCredentials") == true && Directory.Exists(ApplicationPath.Path + "\\" + "DBCredentials") == true)
                    {
                        String Base64RandomChallenge = "";
                        ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                        DeleteCredentials(Base64RandomChallenge);
                    }
                }
            }
            else
            {
                throw new Exception("Error: You have not yet initialized an application path");
            }
        }

        public static void CreateSealedCredentials() 
        {
            if(ApplicationPath.Path!=null && ApplicationPath.Path.CompareTo("") != 0) 
            {
                if (ApplicationPath.IsWindows == true) 
                {
                    if (Directory.Exists(ApplicationPath.Path + "\\DBCredentials") == true)
                    {
                        if (Directory.Exists(ApplicationPath.Path + "\\SealedCredentials") == true)
                        {
                            CreateSealedSession();
                        }
                        else
                        {
                            CreateSealedSession();
                        }
                    }
                    else
                    {
                        throw new Exception("Error: You have not yet purchase a database from merchant");
                    }
                }
                else 
                {
                    if (Directory.Exists(ApplicationPath.Path + "/DBCredentials") == true)
                    {
                        if (Directory.Exists(ApplicationPath.Path + "/SealedCredentials") == true)
                        {
                            CreateSealedSession();
                        }
                        else
                        {
                            CreateSealedSession();
                        }
                    }
                    else
                    {
                        throw new Exception("Error: You have not yet purchase a database from merchant");
                    }
                }
            }
            else 
            {
                throw new Exception("Error: You have not yet initialize application path");
            }            
        }

        private static void CreateSealedSession()
        {
            RevampedKeyPair SessionECDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair SessionECDSAKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            String MySession_ID = MySecureIDGenerator.GenerateUniqueString();
            ECDH_ECDSA_Models MyECDH_ECDSA_Models = new ECDH_ECDSA_Models();
            Boolean CheckServerOnline = true;
            Byte[] DBNameByte = new Byte[] { };
            Byte[] SealedDBNameByte = new Byte[] { };
            Byte[] DBUserNameByte = new Byte[] { };
            Byte[] SealedDBUserNameByte = new Byte[] { };
            Byte[] DBUserPasswordByte = new Byte[] { };
            Byte[] SealedDBUserPasswordByte = new Byte[] { };
            Byte[] ServerECDSAPKByte = new Byte[] { };
            Byte[] ServerECDHSPKByte = new Byte[] { };
            Byte[] ServerECDHPKByte = new Byte[] { };
            Byte[] SignedClientSessionECDHPKByte = new Byte[] { };
            Byte[] ClientSessionECDSAPKByte = new Byte[] { };
            Boolean VerifyBoolean = true;
            String SessionStatus = "";
            String ExceptionString = "";
            using (var InitializeHandShakeHttpclient = new HttpClient())
            {
                InitializeHandShakeHttpclient.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                InitializeHandShakeHttpclient.DefaultRequestHeaders.Accept.Clear();
                InitializeHandShakeHttpclient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = InitializeHandShakeHttpclient.GetAsync("EstablishSealedBoxDBCredentials/byID?ClientPathID=" + MySession_ID);
                try
                {
                    response.Wait();
                }
                catch
                {
                    CheckServerOnline = false;
                }
                if (CheckServerOnline == true)
                {
                    var result = response.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var ECDH_ECDSA_Models_Result = readTask.Result;
                        MyECDH_ECDSA_Models = JsonConvert.DeserializeObject<ECDH_ECDSA_Models>(ECDH_ECDSA_Models_Result);
                        if (MyECDH_ECDSA_Models.ID_Checker_Message.Contains("Error") == false)
                        {
                            ServerECDSAPKByte = Convert.FromBase64String(MyECDH_ECDSA_Models.ECDSA_PK_Base64String);
                            ServerECDHSPKByte = Convert.FromBase64String(MyECDH_ECDSA_Models.ECDH_SPK_Base64String);
                            try
                            {
                                ServerECDHPKByte = SodiumPublicKeyAuth.Verify(ServerECDHSPKByte, ServerECDSAPKByte);
                            }
                            catch (Exception exception)
                            {
                                VerifyBoolean = false;
                                ExceptionString = exception.ToString();
                                SessionStatus += ExceptionString + Environment.NewLine;
                            }
                            if (VerifyBoolean == true)
                            {
                                if (ApplicationPath.IsWindows == true) 
                                {
                                    if (Directory.Exists(ApplicationPath.Path + "\\SealedCredentials") == false)
                                    {
                                        Directory.CreateDirectory(ApplicationPath.Path + "\\SealedCredentials");
                                    }
                                    if (Directory.Exists(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID) == false)
                                    {
                                        Directory.CreateDirectory(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID);
                                    }
                                    DBNameByte = File.ReadAllBytes(ApplicationPath.Path + "\\DBCredentials\\DBNameBytes.txt");
                                    DBUserNameByte = File.ReadAllBytes(ApplicationPath.Path + "\\DBCredentials\\DBUserNameBytes.txt");
                                    DBUserPasswordByte = File.ReadAllBytes(ApplicationPath.Path + "\\DBCredentials\\DBUserPasswordBytes.txt");
                                }
                                else 
                                {
                                    if (Directory.Exists(ApplicationPath.Path + "/SealedCredentials") == false)
                                    {
                                        Directory.CreateDirectory(ApplicationPath.Path + "/SealedCredentials");
                                    }
                                    if (Directory.Exists(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID) == false)
                                    {
                                        Directory.CreateDirectory(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID);
                                    }
                                    DBNameByte = File.ReadAllBytes(ApplicationPath.Path + "/DBCredentials/DBNameBytes.txt");
                                    DBUserNameByte = File.ReadAllBytes(ApplicationPath.Path + "/DBCredentials/DBUserNameBytes.txt");
                                    DBUserPasswordByte = File.ReadAllBytes(ApplicationPath.Path + "/DBCredentials/DBUserPasswordBytes.txt");
                                }                        
                                SealedDBNameByte = SodiumSealedPublicKeyBox.Create(DBNameByte, ServerECDHPKByte);
                                SealedDBUserNameByte = SodiumSealedPublicKeyBox.Create(DBUserNameByte, ServerECDHPKByte);
                                SealedDBUserPasswordByte = SodiumSealedPublicKeyBox.Create(DBUserPasswordByte, ServerECDHPKByte);
                                if (ApplicationPath.IsWindows == true) 
                                {
                                    File.WriteAllBytes(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID + "\\SealedDBName.txt", SealedDBNameByte);
                                    File.WriteAllBytes(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID + "\\SealedDBUserName.txt", SealedDBUserNameByte);
                                    File.WriteAllBytes(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID + "\\SealedDBUserPassword.txt", SealedDBUserPasswordByte);
                                    File.WriteAllText(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID + "\\SealedDBNameB64.txt", Convert.ToBase64String(SealedDBNameByte));
                                    File.WriteAllText(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID + "\\SealedDBUserNameB64.txt", Convert.ToBase64String(SealedDBUserNameByte));
                                    File.WriteAllText(ApplicationPath.Path + "\\SealedCredentials\\" + MySession_ID + "\\SealedDBUserPasswordB64.txt", Convert.ToBase64String(SealedDBUserPasswordByte));
                                }
                                else 
                                {
                                    File.WriteAllBytes(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID + "/SealedDBName.txt", SealedDBNameByte);
                                    File.WriteAllBytes(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID + "/SealedDBUserName.txt", SealedDBUserNameByte);
                                    File.WriteAllBytes(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID + "/SealedDBUserPassword.txt", SealedDBUserPasswordByte);
                                    File.WriteAllText(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID + "/SealedDBNameB64.txt", Convert.ToBase64String(SealedDBNameByte));
                                    File.WriteAllText(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID + "/SealedDBUserNameB64.txt", Convert.ToBase64String(SealedDBUserNameByte));
                                    File.WriteAllText(ApplicationPath.Path + "/SealedCredentials/" + MySession_ID + "/SealedDBUserPasswordB64.txt", Convert.ToBase64String(SealedDBUserPasswordByte));
                                }                                
                            }
                            else
                            {
                                throw new Exception("Error: Unable to verify sealed session parameters received from server");
                            }
                            SodiumSecureMemory.SecureClearBytes(ServerECDHSPKByte);
                            SodiumSecureMemory.SecureClearBytes(ServerECDSAPKByte);
                            SodiumSecureMemory.SecureClearBytes(ServerECDHPKByte);
                        }
                        else
                        {
                            throw new Exception(MyECDH_ECDSA_Models.ID_Checker_Message);
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Unable to fetch values from server");
                    }
                }
                else
                {
                    throw new Exception("Error: The server was offline");
                }
            }
            SessionECDHKeyPair.Clear();
            SessionECDSAKeyPair.Clear();
        }

        private static void DeleteCredentials(String Base64RandomChallenge)
        {
            Byte[] ClientLoginED25519SK = new Byte[] { };
            Byte[] RandomChallenge = Convert.FromBase64String(Base64RandomChallenge);
            Byte[] SignedRandomChallenge = new Byte[] { };
            String UniquePaymentID = "";
            Boolean ServerOnlineChecker = true;
            String[] SubDirectories = new String[] { };
            if (ApplicationPath.IsWindows == true) 
            {
                SubDirectories = Directory.GetDirectories(ApplicationPath.Path + "\\SealedCredentials\\");
            }
            else 
            {
                SubDirectories = Directory.GetDirectories(ApplicationPath.Path + "/SealedCredentials/");
            }
            String SealedSessionID = "";
            if (ApplicationPath.IsWindows == true) 
            {
                SealedSessionID = SubDirectories[0].Remove(0, (ApplicationPath.Path + "\\SealedCredentials\\").Length);
            }
            else 
            {
                SealedSessionID = SubDirectories[0].Remove(0, (ApplicationPath.Path + "/SealedCredentials/").Length);
            }
            if (SealedSessionID != null && SealedSessionID.CompareTo("") != 0)
            {
                if (ApplicationPath.IsWindows == true) 
                {
                    ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\SignatureStorage\\LoginED25519SK.txt");
                }
                else 
                {
                    ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/SignatureStorage/LoginED25519SK.txt");
                }
                SignedRandomChallenge = SodiumPublicKeyAuth.Sign(RandomChallenge, ClientLoginED25519SK, true);
                if (ApplicationPath.IsWindows == true) 
                {
                    UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "\\DBCredentials\\PaymentID.txt");
                }
                else 
                {
                    UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "/DBCredentials/PaymentID.txt");
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.GetAsync("EstablishSealedBoxDBCredentials/DeleteSealedSession?ClientPathID="
                        + SealedSessionID
                        + "&UniquePaymentID="
                        + UniquePaymentID
                        + "&SignedRandomChallenge="
                        + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(SignedRandomChallenge)));
                    try
                    {
                        response.Wait();
                    }
                    catch
                    {
                        ServerOnlineChecker = false;
                    }
                    if (ServerOnlineChecker == true)
                    {
                        var result = response.Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var readTask = result.Content.ReadAsStringAsync();
                            readTask.Wait();

                            var Result = readTask.Result;
                            Result = Result.Substring(1, Result.Length - 2);
                            if (Result.Contains("Error"))
                            {
                                throw new Exception(Result);
                            }
                            else
                            {
                                if (ApplicationPath.IsWindows == true) 
                                {
                                    Directory.Delete(ApplicationPath.Path + "\\SealedCredentials\\" + SealedSessionID, true);
                                }
                                else 
                                {
                                    Directory.Delete(ApplicationPath.Path + "/SealedCredentials/" + SealedSessionID, true);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Error: Unable to fetch values from server");
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Server was now offline");
                    }
                }
            }
            else
            {
                throw new Exception("Error: You have not yet establish a sealed session with the server");
            }
        }
    }
}
