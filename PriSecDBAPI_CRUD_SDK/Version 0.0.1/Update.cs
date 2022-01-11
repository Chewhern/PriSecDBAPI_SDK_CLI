using System;
using System.Text;
using PriSecDBAPI_CRUD_SDK.Model;
using Newtonsoft.Json;
using System.IO;
using PriSecDBAPI_SC_SDK.Helper;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PriSecDBAPI_CRUD_SDK
{
    public static class Update
    {
        public static void UpdateDBRecord(String QueryString, String[] ParameterNameArray, String[] ParameterValueArray, String[] IDValue, String[] NewIDValue, Boolean IsSealedDiffieHellman, Boolean IsXSalsa20Poly1305)
        {
            int SealedCredentialsPathCount = 0;
            Boolean IsPaymentIDExist = true;
            if (ApplicationPath.Path!=null && ApplicationPath.Path.CompareTo("") != 0) 
            {
                SealedCredentialsPathCount = Directory.GetFileSystemEntries(ApplicationPath.Path + "\\SealedCredentials\\").Length;
                if (ApplicationPath.IsWindows == true)
                {
                    IsPaymentIDExist = File.Exists(ApplicationPath.Path + "\\DBCredentials\\PaymentID.txt");
                }
                else
                {
                    IsPaymentIDExist = File.Exists(ApplicationPath.Path + "/DBCredentials/PaymentID.txt");
                }
                if (IsPaymentIDExist == true && SealedCredentialsPathCount != 0)
                {
                    if (IDValue.Length != 0)
                    {
                        int ParameterNameArrayCount = 0;
                        int ParameterValueArrayCount = 0;
                        int LoopCount = 0;
                        String SealedSessionID = "";
                        String SealedDBName = "";
                        String SealedDBUserName = "";
                        String SealedDBUserPassword = "";
                        String UniquePaymentID = "";
                        Byte[] QueryStringByte = new Byte[] { };
                        String Base64QueryString = "";
                        Byte[] ParameterNameByte = new Byte[] { };
                        String Base64ParameterName = "";
                        String[] Base64ParameterNameArray = new String[] { };
                        Byte[] ParameterValueByte = new Byte[] { };
                        String Base64ParameterValue = "";
                        String[] Base64ParameterValueArray = new String[] { };
                        String JSONBodyString = "";
                        Boolean ServerOnlineChecker = true;
                        SpecialDBModel MySpecialDBModel = new SpecialDBModel();
                        StringContent PostRequestData = new StringContent("");
                        String[] SubDirectories = new String[] { };
                        if (ApplicationPath.IsWindows == true)
                        {
                            SubDirectories = Directory.GetDirectories(ApplicationPath.Path + "\\SealedCredentials\\");
                            SealedSessionID = SubDirectories[0].Remove(0, (ApplicationPath.Path + "\\SealedCredentials\\").Length);
                            SealedDBName = File.ReadAllText(ApplicationPath.Path + "\\SealedCredentials\\" + SealedSessionID + "\\SealedDBNameB64.txt");
                            SealedDBUserName = File.ReadAllText(ApplicationPath.Path + "\\SealedCredentials\\" + SealedSessionID + "\\SealedDBUserNameB64.txt");
                            SealedDBUserPassword = File.ReadAllText(ApplicationPath.Path + "\\SealedCredentials\\" + SealedSessionID + "\\SealedDBUserPasswordB64.txt");
                            UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "\\DBCredentials\\PaymentID.txt");
                        }
                        else
                        {
                            SubDirectories = Directory.GetDirectories(ApplicationPath.Path + "/SealedCredentials/");
                            SealedSessionID = SubDirectories[0].Remove(0, (ApplicationPath.Path + "/SealedCredentials/").Length);
                            SealedDBName = File.ReadAllText(ApplicationPath.Path + "/SealedCredentials/" + SealedSessionID + "/SealedDBNameB64.txt");
                            SealedDBUserName = File.ReadAllText(ApplicationPath.Path + "/SealedCredentials/" + SealedSessionID + "/SealedDBUserNameB64.txt");
                            SealedDBUserPassword = File.ReadAllText(ApplicationPath.Path + "/SealedCredentials/" + SealedSessionID + "/SealedDBUserPasswordB64.txt");
                            UniquePaymentID = File.ReadAllText(ApplicationPath.Path + "/DBCredentials/PaymentID.txt");
                        }
                        QueryStringByte = Encoding.UTF8.GetBytes(QueryString);
                        Base64QueryString = Convert.ToBase64String(QueryStringByte);
                        ParameterNameArrayCount = ParameterNameArray.Length;
                        ParameterValueArrayCount = ParameterValueArray.Length;
                        if ((ParameterNameArrayCount == ParameterValueArrayCount) == true)
                        {
                            if (ParameterNameArrayCount == 0 && ParameterValueArrayCount == 0)
                            {
                                if (NewIDValue.Length == 0)
                                {
                                    throw new ArgumentException("Error: New ID Value array length must not be 0");
                                }
                            }
                            Base64ParameterNameArray = new String[ParameterNameArrayCount];
                            Base64ParameterValueArray = new String[ParameterValueArrayCount];
                            while (LoopCount < ParameterNameArrayCount)
                            {
                                ParameterNameByte = Encoding.UTF8.GetBytes(ParameterNameArray[LoopCount]);
                                Base64ParameterName = Convert.ToBase64String(ParameterNameByte);
                                ParameterValueByte = Encoding.UTF8.GetBytes(ParameterValueArray[LoopCount]);
                                Base64ParameterValue = Convert.ToBase64String(ParameterValueByte);
                                Base64ParameterNameArray[LoopCount] = Base64ParameterName;
                                Base64ParameterValueArray[LoopCount] = Base64ParameterValue;
                                LoopCount += 1;
                            }
                            MySpecialDBModel.MyDBCredentialModel = new SealedDBCredentialModel();
                            MySpecialDBModel.MyDBCredentialModel.SealedDBName = SealedDBName;
                            MySpecialDBModel.MyDBCredentialModel.SealedDBUserName = SealedDBUserName;
                            MySpecialDBModel.MyDBCredentialModel.SealedDBUserPassword = SealedDBUserPassword;
                            MySpecialDBModel.MyDBCredentialModel.SealedSessionID = SealedSessionID;
                            MySpecialDBModel.UniquePaymentID = UniquePaymentID;
                            MySpecialDBModel.Base64QueryString = Base64QueryString;
                            MySpecialDBModel.Base64ParameterName = Base64ParameterNameArray;
                            MySpecialDBModel.Base64ParameterValue = Base64ParameterValueArray;
                            MySpecialDBModel.IDValue = IDValue;
                            MySpecialDBModel.NewIDValue = NewIDValue;
                            MySpecialDBModel.IsXSalsa20Poly1305 = IsXSalsa20Poly1305;
                            JSONBodyString = JsonConvert.SerializeObject(MySpecialDBModel);
                            PostRequestData = new StringContent(JSONBodyString, Encoding.UTF8, "application/json");
                            if (IsSealedDiffieHellman == true)
                            {
                                using (var client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(
                                        new MediaTypeWithQualityHeaderValue("application/json"));
                                    var response = client.PostAsync("SpecialSealedDHDBUpdate/", PostRequestData);
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
                            else
                            {
                                using (var client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(
                                        new MediaTypeWithQualityHeaderValue("application/json"));
                                    var response = client.PostAsync("SpecialSealedX3DHDBUpdate/", PostRequestData);
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
                            throw new Exception("Error: Parameter name array length and parameter value array length must be exactly the same and can't be 0");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Error: ID Value array length must not be 0");
                    }
                }
                else 
                {
                    throw new ArgumentException("Error: You haven't make a payment and create a sealed credential");
                }
            }
            else 
            {
                throw new ArgumentException("Error: You have not yet set application path");
            }                       
        }
    }
}
