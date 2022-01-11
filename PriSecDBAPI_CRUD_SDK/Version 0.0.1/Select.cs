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
    public static class Select
    {
        public static void SelectDBRecordChecker(String QueryString, String ParameterName, String ParameterValue, ref String[] RetrievedValue) 
        {
            int SealedCredentialsPathCount = 0;
            Boolean IsPaymentIDExist = true;
            String[] SubRetrievedValue = new String[] { };
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
                    SelectDBRecord(QueryString, ParameterName, ParameterValue, ref SubRetrievedValue);
                    RetrievedValue = SubRetrievedValue;
                }
                else 
                {
                    throw new ArgumentException("Error: You haven't make a payment and create a sealed credential");
                }
            }
            else 
            {
                throw new ArgumentException("Error: You have not set the application path");
            }
        }

        public static void SpecialSelectDBRecordChecker(String QueryString, String[] ParameterName, String[] ParameterValue, ref String[] RetrievedValue)
        {
            int SealedCredentialsPathCount = 0;
            Boolean IsPaymentIDExist = true;
            String[] SubRetrievedValue = new String[] { };
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
                    SpecialSelectDBRecord(QueryString, ParameterName, ParameterValue, ref SubRetrievedValue);
                    RetrievedValue = SubRetrievedValue;
                }
                else
                {
                    throw new ArgumentException("Error: You haven't make a payment and create a sealed credential");
                }
            }
            else
            {
                throw new ArgumentException("Error: You have not set the application path");
            }
        }

        private static void SelectDBRecord(String QueryString, String ParameterName, String ParameterValue, ref String[] RetrievedValue)
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
            Byte[] ParameterValueByte = new Byte[] { };
            String Base64ParameterValue = "";
            String JSONBodyString = "";
            Boolean ServerOnlineChecker = true;
            NormalDBModel MySelectModel = new NormalDBModel();
            DBRecordsModel MyRecordsModel = new DBRecordsModel();
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
            ParameterNameByte = Encoding.UTF8.GetBytes(ParameterName);
            ParameterValueByte = Encoding.UTF8.GetBytes(ParameterValue);
            Base64QueryString = Convert.ToBase64String(QueryStringByte);
            Base64ParameterName = Convert.ToBase64String(ParameterNameByte);
            Base64ParameterValue = Convert.ToBase64String(ParameterValueByte);
            MySelectModel.MyDBCredentialModel = new SealedDBCredentialModel();
            MySelectModel.MyDBCredentialModel.SealedDBName = SealedDBName;
            MySelectModel.MyDBCredentialModel.SealedDBUserName = SealedDBUserName;
            MySelectModel.MyDBCredentialModel.SealedDBUserPassword = SealedDBUserPassword;
            MySelectModel.MyDBCredentialModel.SealedSessionID = SealedSessionID;
            MySelectModel.UniquePaymentID = UniquePaymentID;
            MySelectModel.Base64QueryString = Base64QueryString;
            MySelectModel.Base64ParameterName = Base64ParameterName;
            MySelectModel.Base64ParameterValue = Base64ParameterValue;
            JSONBodyString = JsonConvert.SerializeObject(MySelectModel);
            StringContent PostRequestData = new StringContent(JSONBodyString, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsync("SelectDBRecord/", PostRequestData);
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
                        MyRecordsModel = JsonConvert.DeserializeObject<DBRecordsModel>(Result);
                        if (MyRecordsModel.Status.Contains("Error"))
                        {
                            throw new Exception(MyRecordsModel.Status);
                        }
                        else
                        {
                            RetrievedValue = MyRecordsModel.ParameterValues;
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

        private static void SpecialSelectDBRecord(String QueryString, String[] ParameterName, String[] ParameterValue, ref String[] RetrievedValue)
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
            SpecialSelectDBModel MySelectModel = new SpecialSelectDBModel();
            DBRecordsModel MyRecordsModel = new DBRecordsModel();
            String[] SubDirectories = new String[] { };
            int LoopCount = 0;
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
            Base64QueryString = Convert.ToBase64String(QueryStringByte);
            MySelectModel.MyDBCredentialModel = new SealedDBCredentialModel();
            MySelectModel.MyDBCredentialModel.SealedDBName = SealedDBName;
            MySelectModel.MyDBCredentialModel.SealedDBUserName = SealedDBUserName;
            MySelectModel.MyDBCredentialModel.SealedDBUserPassword = SealedDBUserPassword;
            MySelectModel.MyDBCredentialModel.SealedSessionID = SealedSessionID;
            MySelectModel.UniquePaymentID = UniquePaymentID;
            MySelectModel.Base64QueryString = Base64QueryString;
            MySelectModel.Base64ParameterName = Base64ParameterNameArray;
            MySelectModel.Base64ParameterValue = Base64ParameterValueArray;
            JSONBodyString = JsonConvert.SerializeObject(MySelectModel);
            StringContent PostRequestData = new StringContent(JSONBodyString, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://mrchewitsoftware.com.my:5002/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsync("SpecialSelectDBRecord/", PostRequestData);
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
                        MyRecordsModel = JsonConvert.DeserializeObject<DBRecordsModel>(Result);
                        if (MyRecordsModel.Status.Contains("Error"))
                        {
                            throw new Exception(MyRecordsModel.Status);
                        }
                        else
                        {
                            RetrievedValue = MyRecordsModel.ParameterValues;
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
}
