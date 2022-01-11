using System;
using System.Linq;
using System.Text;
using System.IO;
using ASodium;
using PriSecDBAPI_SC_SDK.Helper;

namespace PriSecDBAPI_CRUD_SDK
{
    public static class DecryptRetrievedValues
    {
        public static void SDHDecrypt(String[] RetrievedData, ref String[] DecryptedData) 
        {
            if (ApplicationPath.Path != null && ApplicationPath.Path.CompareTo("") != 0) 
            {
                int Count = Directory.GetFileSystemEntries(ApplicationPath.Path + "\\DHStorage\\").Length;
                if (Count != 0) 
                {
                    if (DecryptedData.Length != RetrievedData.Length)
                    {
                        throw new ArgumentException("Error: Decrypted Data and Retrieved Data String array length must be the same");
                    }
                    else 
                    {
                        Byte[] RecipientECDHSKByte = new Byte[] { };
                        Byte[] RecipientECDHPKByte = new Byte[] { };
                        Byte[] ConcatedPublicKeyByte = new Byte[] { };
                        Byte[] NonceByte = new Byte[] { };
                        Byte[] SharedSecret = new Byte[] { };
                        Byte[] SealedValueByte = new Byte[] { };
                        Byte[] SanitizedSealedValueByte = new Byte[] { };
                        Byte[] ServerPublicKeyByte = new Byte[] { };
                        Byte[] DecryptedValueByte = new Byte[] { };
                        String DecryptedValue = "";
                        Boolean IsSealedBox = true;
                        int LoopCount = 0;
                        if (ApplicationPath.IsWindows == true)
                        {
                            RecipientECDHPKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\SealedDHX25519PK.txt");
                        }
                        else
                        {
                            RecipientECDHPKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/SealedDHX25519PK.txt");
                        }
                        while (LoopCount < RetrievedData.Length)
                        {
                            SealedValueByte = Convert.FromBase64String(RetrievedData[LoopCount]);
                            if (ApplicationPath.IsWindows == true)
                            {
                                RecipientECDHSKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\SealedDHX25519SK.txt");
                            }
                            else
                            {
                                RecipientECDHSKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/SealedDHX25519SK.txt");
                            }
                            try
                            {
                                DecryptedValueByte = SodiumSealedPublicKeyBox.Open(SealedValueByte, RecipientECDHPKByte, RecipientECDHSKByte, true);
                            }
                            catch
                            {
                                IsSealedBox = false;
                            }
                            if (IsSealedBox == false)
                            {
                                ServerPublicKeyByte = new Byte[32];
                                Array.Copy(SealedValueByte, 0, ServerPublicKeyByte, 0, 32);
                                SanitizedSealedValueByte = new Byte[SealedValueByte.Length - 32];
                                Array.Copy(SealedValueByte, 32, SanitizedSealedValueByte, 0, SanitizedSealedValueByte.Length);
                                if (ApplicationPath.IsWindows == true)
                                {
                                    RecipientECDHSKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\SealedDHX25519SK.txt");
                                }
                                else
                                {
                                    RecipientECDHSKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/SealedDHX25519SK.txt");
                                }
                                SharedSecret = SodiumScalarMult.Mult(RecipientECDHSKByte, ServerPublicKeyByte, true);
                                ConcatedPublicKeyByte = ServerPublicKeyByte.Concat(RecipientECDHPKByte).ToArray();
                                NonceByte = SodiumKDF.KDFFunction((uint)SodiumSecretBoxXChaCha20Poly1305.GenerateNonce().Length, 1, "GetNonce", ConcatedPublicKeyByte);
                                DecryptedValueByte = SodiumSecretBoxXChaCha20Poly1305.Open(SanitizedSealedValueByte, NonceByte, SharedSecret, true);
                            }
                            DecryptedValue = Encoding.UTF8.GetString(DecryptedValueByte);
                            DecryptedData[LoopCount] = DecryptedValue;
                            LoopCount += 1;
                        }
                    }
                }
                else
                {
                    throw new Exception("Error: You have not yet make a payment or somebody deleted the secret cryptography ");
                }
            }
            else 
            {
                throw new ArgumentException("Error: You have not set application path");
            }            
        }

        public static void x3SDHDecrypt(String[] RetrievedData, ref String[] DecryptedData) 
        {
            if(ApplicationPath.Path!=null && ApplicationPath.Path.CompareTo("") != 0) 
            {
                int Count = Directory.GetFileSystemEntries(ApplicationPath.Path + "\\DHStorage\\").Length;
                if (Count != 0) 
                {
                    if (DecryptedData.Length != RetrievedData.Length) 
                    {
                        throw new ArgumentException("Error: Decrypted Data and Retrieved Data String array length must be the same");
                    }
                    else 
                    {
                        Byte[] SharedSecret1 = new Byte[] { };
                        Byte[] SharedSecret2 = new Byte[] { };
                        Byte[] SharedSecret3 = new Byte[] { };
                        Byte[] SharedSecret4 = new Byte[] { };
                        Byte[] ConcatedSharedSecret = new Byte[] { };
                        Byte[] MasterSharedSecret = new Byte[] { };
                        Byte[] BobIKX25519SKByte = new Byte[] { };
                        Byte[] BobIKX25519PKByte = new Byte[] { };
                        Byte[] BobSPKX25519SKByte = new Byte[] { };
                        Byte[] BobSPKX25519PKByte = new Byte[] { };
                        Byte[] BobOPKX25519SKByte = new Byte[] { };
                        Byte[] BobOPKX25519PKByte = new Byte[] { };
                        Byte[] AliceIKX25519PKByte = new Byte[] { };
                        Byte[] AliceEKX25519PKByte = new Byte[] { };
                        Byte[] AliceConcatedX25519PKByte = new Byte[] { };
                        Byte[] BobConcatedX25519PKByte = new Byte[] { };
                        Byte[] AliceCheckSum = new Byte[] { };
                        Byte[] BobCheckSum = new Byte[] { };
                        Byte[] ConcatedCheckSum = new Byte[] { };
                        Byte[] NonceByte = new Byte[] { };
                        Byte[] SealedValueByte = new Byte[] { };
                        Byte[] SanitizedSealedValueByte = new Byte[] { };
                        Byte[] ServerPublicKeyByte = new Byte[] { };
                        Byte[] DecryptedValueByte = new Byte[] { };
                        String DecryptedValue = "";
                        Boolean IsXSalsa20Poly1305 = true;
                        int LoopCount = 0;
                        if (ApplicationPath.IsWindows == true)
                        {
                            BobIKX25519PKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\IKX25519PK.txt");
                            BobSPKX25519PKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\SPKX25519PK.txt");
                            BobOPKX25519PKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\OPKX25519PK.txt");
                        }
                        else
                        {
                            BobIKX25519PKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/IKX25519PK.txt");
                            BobSPKX25519PKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/SPKX25519PK.txt");
                            BobOPKX25519PKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/OPKX25519PK.txt");
                        }
                        while (LoopCount < RetrievedData.Length)
                        {
                            SealedValueByte = Convert.FromBase64String(RetrievedData[LoopCount]);
                            AliceIKX25519PKByte = new Byte[32];
                            AliceEKX25519PKByte = new Byte[32];
                            SanitizedSealedValueByte = new Byte[SealedValueByte.Length - 64];
                            Array.Copy(SealedValueByte, 0, AliceIKX25519PKByte, 0, 32);
                            Array.Copy(SealedValueByte, 32, AliceEKX25519PKByte, 0, 32);
                            Array.Copy(SealedValueByte, 64, SanitizedSealedValueByte, 0, SanitizedSealedValueByte.Length);
                            if (ApplicationPath.IsWindows == true)
                            {
                                BobIKX25519SKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\IKX25519SK.txt");
                                BobSPKX25519SKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\SPKX25519SK.txt");
                                BobOPKX25519SKByte = File.ReadAllBytes(ApplicationPath.Path + "\\DHStorage\\OPKX25519SK.txt");
                            }
                            else
                            {
                                BobIKX25519SKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/IKX25519SK.txt");
                                BobSPKX25519SKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/SPKX25519SK.txt");
                                BobOPKX25519SKByte = File.ReadAllBytes(ApplicationPath.Path + "/DHStorage/OPKX25519SK.txt");
                            }
                            SharedSecret1 = SodiumScalarMult.Mult(BobSPKX25519SKByte, AliceIKX25519PKByte);
                            SharedSecret2 = SodiumScalarMult.Mult(BobIKX25519SKByte, AliceEKX25519PKByte, true);
                            SharedSecret3 = SodiumScalarMult.Mult(BobSPKX25519SKByte, AliceEKX25519PKByte, true);
                            SharedSecret4 = SodiumScalarMult.Mult(BobOPKX25519SKByte, AliceEKX25519PKByte, true);
                            ConcatedSharedSecret = SharedSecret1.Concat(SharedSecret2).Concat(SharedSecret3).Concat(SharedSecret4).ToArray();
                            MasterSharedSecret = SodiumKDF.KDFFunction(32, 1, "X3DHSKey", ConcatedSharedSecret, true);
                            AliceConcatedX25519PKByte = AliceIKX25519PKByte.Concat(AliceEKX25519PKByte).ToArray();
                            BobConcatedX25519PKByte = BobSPKX25519PKByte.Concat(BobIKX25519PKByte).Concat(BobOPKX25519PKByte).ToArray();
                            AliceCheckSum = SodiumGenericHash.ComputeHash(64, AliceConcatedX25519PKByte);
                            BobCheckSum = SodiumGenericHash.ComputeHash(64, BobConcatedX25519PKByte);
                            ConcatedCheckSum = AliceCheckSum.Concat(BobCheckSum).ToArray();
                            try
                            {
                                NonceByte = SodiumGenericHash.ComputeHash((Byte)SodiumSecretBox.GenerateNonce().Length, ConcatedCheckSum);
                                DecryptedValueByte = SodiumSecretBox.Open(SanitizedSealedValueByte, NonceByte, MasterSharedSecret, true);
                            }
                            catch
                            {
                                IsXSalsa20Poly1305 = false;
                            }
                            if (IsXSalsa20Poly1305 == false)
                            {
                                NonceByte = SodiumGenericHash.ComputeHash((Byte)SodiumSecretBoxXChaCha20Poly1305.GenerateNonce().Length, ConcatedCheckSum);
                                DecryptedValueByte = SodiumSecretBoxXChaCha20Poly1305.Open(SanitizedSealedValueByte, NonceByte, MasterSharedSecret, true);
                            }
                            DecryptedValue = Encoding.UTF8.GetString(DecryptedValueByte);
                            SodiumSecureMemory.SecureClearBytes(SharedSecret1);
                            SodiumSecureMemory.SecureClearBytes(SharedSecret2);
                            SodiumSecureMemory.SecureClearBytes(SharedSecret3);
                            SodiumSecureMemory.SecureClearBytes(SharedSecret4);
                            SodiumSecureMemory.SecureClearBytes(ConcatedSharedSecret);
                            SodiumSecureMemory.SecureClearBytes(MasterSharedSecret);
                            DecryptedData[LoopCount] = DecryptedValue;
                            LoopCount += 1;
                        }
                    }
                }
                else 
                {
                    throw new Exception("Error: You have not yet make a payment or somebody deleted the secret cryptography ");
                }
            }
            else 
            {
                throw new ArgumentException("Error: You have not set application path");
            }            
        }
    }
}
