using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

namespace BetaSpeckle
{
    public class SPK_Object
    {
        IFormatter formatter = new BinaryFormatter();

        //this is the data that actually gets exported
        public dynamic data;

        public System.Guid uuid;
        public string type;
        public string parentGuid;
        public string myHash;

        public SPK_Object()
        {
            uuid = System.Guid.NewGuid();
            data = new System.Dynamic.ExpandoObject();
        }

        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        /// <summary>
        /// Writes itself to a file
        /// </summary>
        public void serialize(String path)
        {

            path = Path.Combine(path, (myHash) );
            
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);
           
            file.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
            file.Close();
        }

    }
}