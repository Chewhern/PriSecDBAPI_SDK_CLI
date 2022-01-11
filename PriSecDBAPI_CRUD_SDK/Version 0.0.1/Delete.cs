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
    public static class Delete
    {
        public static void SpecialDeleteDBRecord(String QueryString, String[] ParameterName, String[] ParameterValue)
        {
            int SealedCredentialsPathCount = 0;
            Boolean IsPaymentIDExist = true;
            if (ApplicationPath.Path != null && ApplicationPath.Path.CompareTo("") != 0) 
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
                    String SealedSessionID = "";
                    String SealedDBName = "";
                    String SealedDBUserName = "";
                    String SealedDBUserPassword = "";
                    String UniquePaymentID = "";
                    Byte[] QueryStringByte = new Byte[] { };
                    String Base64QueryString = "";
                    Byte[] ParameterNameByte = new Byte[] { };
                    String Base64ParameterName = "";
                    String[] Base64ParameterNameArray = new String[ParameterName.Length];
                    Byte[] ParameterValueByte = new Byte[] { };
                    String Base64ParameterValue = "";
                    String[] Base64ParameterValueArray = new String[ParameterValue.Length];
                    String JSONBodyString = "";
                    Boolean ServerOnlineChecker = true;
                    SpecialSelectDBModel MyDeleteModel = new SpecialSelectDBModel();
                    int LoopCount = 0;
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
                    while (LoopCount < ParameterName.Length)
                    {
                        ParameterNameByte = Encoding.UTF8.GetBytes(ParameterName[LoopCount]);
                        ParameterValueByte = Encoding.UTF8.GetBytes(ParameterValue[LoopCount]);
                        Base64ParameterName = Convert.ToBase64String(ParameterNameByte);
                        Base64ParameterValue = Convert.ToBase64String(ParameterValueByte);
                        Base64ParameterNameArray[LoopCount] = Base64ParameterName;
                        Base64ParameterValueArray[LoopCount] = Base64ParameterValue;
                        LoopCount += 1;
                    }
                    MyDeleteModel.MyDBCredentialModel = new SealedDBCredentialModel();
                    MyDeleteModel.MyDBCredentialModel.SealedDBName = SealedDBName;
                    MyDeleteModel.MyDBCredentialModel.SealedDBUserName = SealedDBUserName;
                    MyDeleteModel.MyDBCredentialModel.SealedDBUserPassword = SealedDBUserPassword;
                    MyDeleteModel.MyDBCredentialModel.SealedSessionID = SealedSessionID;
                    MyDeleteModel.UniquePaymentID = UniquePaymentID;
                    MyDeleteModel.Base64QueryString = Base64QueryString;
                    MyDeleteModel.Base64ParameterName = Base64ParameterNameArray;
                    MyDeleteModel.Base64ParameterValue = Base64ParameterValueArray;
                    JSONBodyString = JsonConvert.SerializeObject(MyDeleteModel);
                    StringContent PostRequestData = new StringContent(JSONBodyString, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.PostAsync("SpecialDeleteDBRecord/", PostRequestData);
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
