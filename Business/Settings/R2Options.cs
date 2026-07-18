namespace Business.Settings
{
    public class R2Options
    {
        public const string SectionName = "R2";

        public string AccountId { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string BucketName { get; set; } = string.Empty;
    }
}
