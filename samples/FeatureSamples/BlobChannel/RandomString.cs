namespace BlobChannelSample
{
    public static class RandomString
    {
        public static string GetRandomString(int length)
        {
            byte[] buffer = new byte[length];
#pragma warning disable CA5394
            Random ran = new();
            ran.NextBytes(buffer);
#pragma warning restore CA5394
            return Convert.ToBase64String(buffer);
        }
    }
}
