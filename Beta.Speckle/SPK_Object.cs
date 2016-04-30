/*
 * Beta.Speckle GH Exporter Component
 * Copyright (C) 2016 Dimitrie A. Stefanescu (@idid) / The Bartlett School of Architecture, UCL
 * 
 */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;
using Grasshopper.Kernel;

namespace BetaSpeckle
{
    [Serializable]
    public class SPK_Object
    {
        IFormatter formatter = new BinaryFormatter();

        //this is the data that actually gets exported - false
        public dynamic data;

        public System.Guid uuid;
        public string type;
        public string parentGuid;
        public string hashText;
        public string myHash;
        public int hashIndex;

        public SPK_Object()
        {
            uuid = System.Guid.NewGuid();
            data = new System.Dynamic.ExpandoObject();
        }

        public void computeHash()
        {
            myHash = sha256_hash(hashText);
        }

        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA1 hash = SHA1.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public void serialize(String path, int index)
        {
            path = Path.Combine(path, index.ToString());

            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            file.WriteLine(JsonConvert.SerializeObject(data, Formatting.None));
            file.Close();
        }

    }
}