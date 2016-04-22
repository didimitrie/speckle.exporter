using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace BetaSpeckle
{
    public class SPK_Object
    {
        IFormatter formatter = new BinaryFormatter();

        public System.Guid uuid;
        public string type;
        public dynamic data;
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

    }
}