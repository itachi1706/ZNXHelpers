using System.Text;

namespace ZNXHelpers
{
    public class Base64Helper
    {
        public string EncodeString(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes);
        }

        public string DecodeString(string input)
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(inputBytes);
        }
    }
}
