namespace AzureBlob.Api.Models
{
    public class TaggedImage
    {
        public string Caption { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string Thumbnail { get; internal set; }
        public string Url { get; internal set; }
    }
}