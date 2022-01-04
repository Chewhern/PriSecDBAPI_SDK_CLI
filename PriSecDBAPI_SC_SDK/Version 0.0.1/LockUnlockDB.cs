using System;
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

    public static class LockUnlockDB
    {
        public static void LockDBAccount(Boolean LockAccount=true)
        {
            if(ApplicationPath.Path!=null && ApplicationPath.Path.CompareTo("") != 0) 
            {
                if (LockAccount == true) 
                {
                    if (ApplicationPath.IsWindows == true)
                    {
                        if (Directory.Exists(ApplicationPath.Path + "\\SealedCredentials\\") == true && Directory.Exists(ApplicationPath.Path + "\\DBCredentials") == true)
                        {
                            String Base64RandomChallenge = "";
                            ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                            LockDBAccount(Base64RandomChallenge);
                        }
                        else 
                        {
                            throw new Exception("Error: SealedCredentials and DBCredentials sub directory aren't exist");
                        }
                    }
                    else
                    {
                        if (Directory.Exists(ApplicationPath.Path + "/SealedCredentials/") == true && Directory.Exists(ApplicationPath.Path + "/DBCredentials") == true)
                        {
                            String Base64RandomChallenge = "";
                            ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                            LockDBAccount(Base64RandomChallenge);
                        }
                        else
                        {
                            throw new Exception("Error: SealedCredentials and DBCredentials sub directory aren't exist");
                        }
                    }
                }
                else 
                {
                    if (ApplicationPath.IsWindows == true)
                    {
                        if (Directory.Exists(ApplicationPath.Path + "\\SealedCredentials\\") == true && Directory.Exists(ApplicationPath.Path + "\\DBCredentials") == true)
                        {
                            String Base64RandomChallenge = "";
                            ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                            LockDBAccount(Base64RandomChallenge,false);
                        }
                        else
                        {
                            throw new Exception("Error: SealedCredentials and DBCredentials sub directory aren't exist");
                        }
                    }
                    else
                    {
                        if (Directory.Exists(ApplicationPath.Path + "/SealedCredentials/") == true && Directory.Exists(ApplicationPath.Path + "/DBCredentials") == true)
                        {
                            String Base64RandomChallenge = "";
                            ChallengeRequestor.RequestChallenge(ref Base64RandomChallenge);
                            LockDBAccount(Base64RandomChallenge,false);
                        }
                        else
                        {
                            throw new Exception("Error: SealedCredentials and DBCredentials sub directory aren't exist");
                        }
                    }
                }
            }
            else 
            {
                throw new Exception("Error: You have not yet initialized an application path");
            }            
        }

        private static void LockDBAccount(String Base64RandomChallenge, Boolean LockAccount = true)
        {
            Byte[] ClientLoginED25519SK = new Byte[] { };
            Byte[] RandomChallenge = Convert.FromBase64String(Base64RandomChallenge);
            Byte[] SignedRandomChallenge = new Byte[] { };
            String SealedDBUserName = "";
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
            LockDBAccountModel MyLockModel = new LockDBAccountModel();
            String JSONBodyString = "";
            if (SealedSessionID != null && SealedSessionID.CompareTo("") != 0)
            {
                if (ApplicationPath.IsWindows == true) 
                {
                    ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "\\" + "SignatureStorage\\LoginED25519SK.txt");
                }
                else 
                {
                    ClientLoginED25519SK = File.ReadAllBytes(ApplicationPath.Path + "/" + "SignatureStorage/LoginED25519SK.txt");
                }
                SignedRandomChallenge = SodiumPublicKeyAuth.Sign(RandomChallenge, ClientLoginED25519SK, true);
                if (ApplicationPath.IsWindows == true) 
                {
                    UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "\\DBCredentials\\PaymentID.txt");
                    SealedDBUserName = File.ReadAllText(ApplicationPath.Path + "\\SealedCredentials\\" + SealedSessionID + "\\SealedDBUserNameB64.txt");
                }
                else 
                {
                    UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "/DBCredentials/PaymentID.txt");
                    SealedDBUserName = File.ReadAllText(ApplicationPath.Path + "/SealedCredentials/" + SealedSessionID + "/SealedDBUserNameB64.txt");
                }
                MyLockModel.SealedDBUserName = SealedDBUserName;
                MyLockModel.SealedSessionID = SealedSessionID;
                MyLockModel.SignedRandomChallenge = Convert.ToBase64String(SignedRandomChallenge);
                MyLockModel.UniquePaymentID = UniquePaymentID;
                JSONBodyString = JsonConvert.SerializeObject(MyLockModel);
                StringContent PostRequestData = new StringContent(JSONBodyString, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    if (LockAccount == true)
                    {
                        client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsync("LockDBAccount/", PostRequestData);
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
                    else
                    {
                        client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsync("UnlockDBAccount/", PostRequestData);
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
            }
            else
            {
                throw new Exception("Error: You have not yet establish an ETLS with server");
            }
        }
    }
}
