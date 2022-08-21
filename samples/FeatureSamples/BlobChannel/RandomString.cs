namespace BlobChannelSample
{
    public class RandomString
    {
        public static string GetRandomString(int length)
        {
            byte[] buffer = new byte[length];
            Random ran = new();
            ran.NextBytes(buffer);
            return Convert.ToBase64String(buffer);
        }
    }
}
