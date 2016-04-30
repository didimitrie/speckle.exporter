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

        //this is the data that actually gets exported
        public dynamic data;

        public System.Guid uuid;
        public string type;
        public string parentGuid;
        public string myHash;
        public int hashIndex;
        public object myGeometry;

        public SPK_Object()
        {
            uuid = System.Guid.NewGuid();
            data = new System.Dynamic.ExpandoObject();
        }

        public void computeHash()
        {
            myHash = this.getHash();
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

        /// <summary>
        /// Writes itself to a file
        /// </summary>
        public void serialize(String path)
        {

            path = Path.Combine(path, (myHash) );
            
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);
           
            file.WriteLine(JsonConvert.SerializeObject(data, Formatting.None));
            file.Close();
        }

        public void serialize(String path, int index)
        {
            path = Path.Combine(path, index.ToString());

            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            file.WriteLine(JsonConvert.SerializeObject(data, Formatting.None));
            file.Close();
        }

        // http://stackoverflow.com/questions/6979718/c-sharp-object-to-string-and-back
        public string getHash()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try {
                    new BinaryFormatter().Serialize(ms, this.myGeometry);
                    return sha256_hash( Convert.ToBase64String(ms.ToArray()) );
                } catch
                {
                    Console.WriteLine("failed to serialize: " + this.type);
                    return "error"; //will not serialize, exporting a nothingsess
                }
            }
        }
    }
}