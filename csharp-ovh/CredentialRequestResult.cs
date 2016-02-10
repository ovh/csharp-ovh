using Newtonsoft.Json;

namespace Ovh.Api
{
    public class CredentialRequestResult
    {
        [JsonProperty(PropertyName = "validationUrl")]
        public string ValidationUrl { get; set; }
        [JsonProperty(PropertyName = "consumerKey")]
        public string ConsumerKey { get; set; }
    }
}