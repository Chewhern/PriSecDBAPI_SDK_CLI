using System;
using PriSecDBAPI_SC_SDK.Helper;
using PriSecDBAPI_SC_SDK.Model;
using ASodium;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;

namespace PriSecDBAPI_SC_SDK
{
    public static class ETLS
    {
        private static SecureIDGenerator MySecureIDGenerator = new SecureIDGenerator();

        public static void InitializeETLSPath(String Path, Boolean IsWindows=false) 
        {
            if(Path!=null && Path.CompareTo("") != 0) 
            {
                ETLSPath.Path = Path;
            }
            else 
            {
                throw new ArgumentException("Error: Path for ETLS must not be null/empty");
            }
        }

        public static String ShowETLSPath() 
        {
            return ETLSPath.Path;
        }

        public static void CreateResetSession() 
        {
            String Base64Result = "";
            if(ETLSPath.Path!=null && ETLSPath.Path.CompareTo("") != 0) 
            {
                if (Directory.Exists(ETLSPath.Path) == true) 
                {
                    if (Directory.GetFileSystemEntries(ETLSPath.Path).Length == 0 || Directory.GetFileSystemEntries(ETLSPath.Path).Length == 1)
                    {
                        CreateNewSession();
                    }
                    else
                    {
                        InitiateETLSDeletion(ref Base64Result);
                        if (Base64Result != null && Base64Result.CompareTo("") != 0)
                        {
                            DeleteETLSSession(Base64Result);
                        }
                        else
                        {
                            throw new Exception("Error: Failed to get random challenge from server");
                        }
                    }
                }
                else 
                {
                    throw new ArgumentException("Error: The ETLS Path must exist/created");
                }
            }
            else 
            {
                throw new ArgumentException("Error: ETLS Path has not been initialized");
            }
        }

        private static void CreateNewSession()
        {
            RevampedKeyPair SessionECDHKeyPair = SodiumPublicKeyBox.GenerateRevampedKeyPair();
            RevampedKeyPair SessionECDSAKeyPair = SodiumPublicKeyAuth.GenerateRevampedKeyPair();
            String MySession_ID = MySecureIDGenerator.GenerateUniqueString();
            ECDH_ECDSA_Models MyECDH_ECDSA_Models = new ECDH_ECDSA_Models();
            Boolean CheckServerOnline = true;
            Boolean CreateShareSecretStatus = true;
            Boolean CheckSharedSecretStatus = true;
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
                var response = InitializeHandShakeHttpclient.GetAsync("ECDH_ECDSA_TempSession/byID?ClientPathID=" + MySession_ID);
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
                                ClientSessionECDSAPKByte = SessionECDSAKeyPair.PublicKey;
                                SignedClientSessionECDHPKByte = SodiumPublicKeyAuth.Sign(SessionECDHKeyPair.PublicKey, SessionECDSAKeyPair.PrivateKey);
                                CreateSharedSecret(ref CreateShareSecretStatus, MySession_ID, SignedClientSessionECDHPKByte, ClientSessionECDSAPKByte);
                                if (CreateShareSecretStatus == true)
                                {
                                    CheckSharedSecret(ref CheckSharedSecretStatus, MySession_ID, ServerECDHPKByte, SessionECDHKeyPair.PrivateKey, SessionECDHKeyPair.PublicKey, SessionECDSAKeyPair.PrivateKey);
                                    if (CheckSharedSecretStatus == false)
                                    {
                                        throw new Exception("Error: The shared secret created locally does not match with server");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Error: Unable to create shared secret on server's side");
                                }
                            }
                            else
                            {
                                throw new Exception("Error in verifying the server ECDH public key");
                            }
                            SodiumSecureMemory.SecureClearBytes(ServerECDSAPKByte);
                            SodiumSecureMemory.SecureClearBytes(ServerECDHSPKByte);
                        }
                        else
                        {
                            throw new Exception(MyECDH_ECDSA_Models.ID_Checker_Message);
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Unable to get handshake parameters from server");
                    }
                }
                else
                {
                    throw new Exception("Error: Server was offline");
                }
            }
            SessionECDHKeyPair.Clear();
            SessionECDSAKeyPair.Clear();
        }

        private static void CreateSharedSecret(ref Boolean CheckBoolean, String MySession_ID, Byte[] SignedClientSessionECDHPKByte, Byte[] ClientSessionECDSAPKByte)
        {
            CheckBoolean = false;
            String SessionStatus = "";
            var CreateSharedSecretHttpClient = new HttpClient();
            CreateSharedSecretHttpClient.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
            CreateSharedSecretHttpClient.DefaultRequestHeaders.Accept.Clear();
            CreateSharedSecretHttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var newresponse = CreateSharedSecretHttpClient.GetAsync("ECDH_ECDSA_TempSession/ByHandshake?ClientPathID=" + MySession_ID + "&SECDHPK=" + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(SignedClientSessionECDHPKByte)) + "&ECDSAPK=" + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(ClientSessionECDSAPKByte)));
            try
            {
                newresponse.Wait();
                var newresult = newresponse.Result;

                if (newresult.IsSuccessStatusCode)
                {
                    var newreadTask = newresult.Content.ReadAsStringAsync();
                    newreadTask.Wait();

                    SessionStatus += newreadTask.Result;

                    if (SessionStatus.Contains("Error"))
                    {
                        throw new Exception(SessionStatus);
                    }
                    else
                    {
                        CheckBoolean = true;
                    }
                }
                else
                {
                    throw new Exception("Error: Unable to fetch values from server");
                }
            }
            catch
            {
                throw new Exception("Error: Server was offline");
            }
        }

        private static void CheckSharedSecret(ref Boolean CheckBoolean, String MySession_ID, Byte[] ServerECDHPKByte, Byte[] SessionECDHPrivateKey, Byte[] SessionECDHPublicKey, Byte[] SessionECDSAPrivateKey)
        {
            if(ETLSPath.Path!=null && ETLSPath.Path.CompareTo("") != 0) 
            {
                if (ApplicationPath.IsWindows == true) 
                {
                    if (Directory.Exists(ETLSPath.Path + "\\" + MySession_ID) == false)
                    {
                        Directory.CreateDirectory(ETLSPath.Path + "\\" + MySession_ID);
                        File.WriteAllBytes(ETLSPath.Path + "\\" + MySession_ID + "\\" + "ECDHSK.txt", SessionECDHPrivateKey);
                    }
                    else
                    {
                        File.WriteAllBytes(ETLSPath.Path + "\\" + MySession_ID + "\\" + "ECDHSK.txt", SessionECDHPrivateKey);
                    }
                }
                else 
                {
                    if (Directory.Exists(ETLSPath.Path + "/" + MySession_ID) == false)
                    {
                        Directory.CreateDirectory(ETLSPath.Path + "/" + MySession_ID);
                        File.WriteAllBytes(ETLSPath.Path + "/" + MySession_ID + "/" + "ECDHSK.txt", SessionECDHPrivateKey);
                    }
                    else
                    {
                        File.WriteAllBytes(ETLSPath.Path + "/" + MySession_ID + "/" + "ECDHSK.txt", SessionECDHPrivateKey);
                    }
                }                
                CheckBoolean = false;
                Boolean CheckServerOnline = true;
                String CheckSharedSecretStatus = "";
                Byte[] TestData = new Byte[] { 255, 255, 255 };
                Byte[] SharedSecretByte = SodiumScalarMult.Mult(SessionECDHPrivateKey, ServerECDHPKByte);
                Byte[] NonceByte = SodiumSecretBox.GenerateNonce();
                Byte[] TestEncryptedData = SodiumSecretBox.Create(TestData, NonceByte, SharedSecretByte);
                var CheckSharedSecretHttpClient = new HttpClient();
                CheckSharedSecretHttpClient.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                CheckSharedSecretHttpClient.DefaultRequestHeaders.Accept.Clear();
                CheckSharedSecretHttpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var CheckSharedSecretHttpClientResponse = CheckSharedSecretHttpClient.GetAsync("ECDH_ECDSA_TempSession/BySharedSecret?ClientPathID=" + MySession_ID + "&CipheredData=" + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(TestEncryptedData)) + "&Nonce=" + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(NonceByte)));
                try
                {
                    CheckSharedSecretHttpClientResponse.Wait();
                }
                catch
                {
                    CheckServerOnline = false;
                }
                if (CheckServerOnline == true)
                {
                    var CheckSharedSecretHttpClientResponseResult = CheckSharedSecretHttpClientResponse.Result;

                    if (CheckSharedSecretHttpClientResponseResult.IsSuccessStatusCode)
                    {
                        var CheckSharedSecretHttpClientResponseResultReadTask = CheckSharedSecretHttpClientResponseResult.Content.ReadAsStringAsync();
                        CheckSharedSecretHttpClientResponseResultReadTask.Wait();

                        CheckSharedSecretStatus = CheckSharedSecretHttpClientResponseResultReadTask.Result;
                        if (CheckSharedSecretStatus.Contains("Error"))
                        {
                            throw new Exception(CheckSharedSecretStatus);
                        }
                        else
                        {
                            if (ApplicationPath.IsWindows == true) 
                            {
                                File.WriteAllBytes(ETLSPath.Path + "\\" + MySession_ID + "\\" + "SharedSecret.txt", SharedSecretByte);
                                File.WriteAllBytes(ETLSPath.Path + "\\" + MySession_ID + "\\" + "ECDSASK.txt", SessionECDSAPrivateKey);
                                File.WriteAllText(ETLSPath.Path + "\\" + "SessionID.txt", MySession_ID);
                            }
                            else 
                            {
                                File.WriteAllBytes(ETLSPath.Path + "/" + MySession_ID + "/" + "SharedSecret.txt", SharedSecretByte);
                                File.WriteAllBytes(ETLSPath.Path + "/" + MySession_ID + "/" + "ECDSASK.txt", SessionECDSAPrivateKey);
                                File.WriteAllText(ETLSPath.Path + "/" + "SessionID.txt", MySession_ID);
                            }
                            CheckBoolean = true;
                            ETLSSessionIDStorage.ETLSID = MySession_ID;
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Unable to fetch values from server");
                    }
                    SodiumSecureMemory.SecureClearBytes(SharedSecretByte);
                    SodiumSecureMemory.SecureClearBytes(SessionECDSAPrivateKey);
                }
                else
                {
                    throw new Exception("Error: Server was offline");
                }
            }
            else 
            {
                throw new ArgumentException("Error: You haven't initialized a path for ETLS");
            }            
        }

        private static void InitiateETLSDeletion(ref String Base64Result)
        {
            StreamReader MyStreamReader;
            if (ApplicationPath.IsWindows == true) 
            {
                MyStreamReader = new StreamReader(ETLSPath.Path + "\\" + "SessionID.txt");
            }
            else 
            {
                MyStreamReader = new StreamReader(ETLSPath.Path + "/" + "SessionID.txt");
            }
            String Temp_Session_ID = MyStreamReader.ReadLine();
            MyStreamReader.Close();
            Boolean CheckServerOnline = true;
            using (var InitializeHandShakeHttpclient = new HttpClient())
            {
                InitializeHandShakeHttpclient.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                InitializeHandShakeHttpclient.DefaultRequestHeaders.Accept.Clear();
                InitializeHandShakeHttpclient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = InitializeHandShakeHttpclient.GetAsync("ECDH_ECDSA_TempSession/InitiateDeletionOfETLS?ClientPathID=" + Temp_Session_ID);
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

                        var Result = readTask.Result;

                        if (Result.Contains("Error") == true)
                        {
                            throw new Exception(Result);
                        }
                        else
                        {
                            Base64Result = Result.Substring(1, Result.Length - 2);
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Unable to fetch values from server");
                    }
                }
                else
                {
                    throw new Exception("Error: Server was offline");
                }
            }
        }

        private static void DeleteETLSSession(String Base64Result)
        {
            Boolean ServerOnlineChecker = true;
            StreamReader MyStreamReader;
            if (ApplicationPath.IsWindows == true)
            {
                MyStreamReader = new StreamReader(ETLSPath.Path + "\\" + "SessionID.txt");
            }
            else
            {
                MyStreamReader = new StreamReader(ETLSPath.Path + "/" + "SessionID.txt");
            }
            String Temp_Session_ID = MyStreamReader.ReadLine();
            MyStreamReader.Close();
            Byte[] ClientECDSASKByte = new Byte[] { };
            Byte[] RandomData = Convert.FromBase64String(Base64Result);
            Byte[] SignedRandomData = new Byte[] { };
            String Status = "";
            if (Temp_Session_ID != null && Temp_Session_ID.CompareTo("") != 0)
            {
                using (var client = new HttpClient())
                {
                    if (ApplicationPath.IsWindows == true) 
                    {
                        ClientECDSASKByte = File.ReadAllBytes(ETLSPath.Path + "\\" + Temp_Session_ID + "\\ECDSASK.txt");
                    }
                    else 
                    {
                        ClientECDSASKByte = File.ReadAllBytes(ETLSPath.Path + "/" + Temp_Session_ID + "/ECDSASK.txt");
                    }
                    SignedRandomData = SodiumPublicKeyAuth.Sign(RandomData, ClientECDSASKByte, true);
                    client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.GetAsync("ECDH_ECDSA_TempSession/DeleteByClientCryptographicID?ClientPathID=" + Temp_Session_ID + "&ValidationData=" + System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(SignedRandomData)));
                    SodiumSecureMemory.SecureClearBytes(RandomData);
                    SodiumSecureMemory.SecureClearBytes(SignedRandomData);
                    try
                    {
                        response.Wait();
                    }
                    catch
                    {
                        ServerOnlineChecker = false;
                        throw new Exception("Error: Server was offline");
                    }
                    if (ServerOnlineChecker == true)
                    {
                        var result = response.Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var readTask = result.Content.ReadAsStringAsync();
                            readTask.Wait();

                            Status = readTask.Result;

                            if (Status.Contains("Error"))
                            {
                                throw new Exception(Status);
                            }
                            else
                            {
                                if (ApplicationPath.IsWindows == true) 
                                {
                                    Directory.Delete(ETLSPath.Path + "\\" + Temp_Session_ID, true);
                                    File.WriteAllText(ETLSPath.Path + "\\" + "SessionID.txt", "");
                                }
                                else 
                                {
                                    Directory.Delete(ETLSPath.Path + "/" + Temp_Session_ID, true);
                                    File.WriteAllText(ETLSPath.Path + "/" + "SessionID.txt", "");
                                }                                
                                CreateNewSession();
                            }
                        }
                        else
                        {
                            throw new Exception("Error: Unable to fetch values from server");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Error: You haven't establish an ETLS session with server");
            }
        }
    }
}
