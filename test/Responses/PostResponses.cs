using System.Net;
using System.Net.Http;
using System.Text;

namespace Ovh.Test.Responses
{
    public static class Post
    {
        public static string me_geolocation_content = "{\"countryCode\":\"eo\",\"ip\":\"256.0.0.1\",\"continent\":\"Atlantis\"}";
        public static HttpResponseMessage me_geolocation_message = new HttpResponseMessage{
            Content = new StringContent(me_geolocation_content, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK
        };

        public static string me_contact_content = "{\"email\":\"deleteme@deleteme.deleteme\",\"lastName\":\"deleteme\",\"birthCountry\":null,\"language\":\"fr_FR\",\"nationalIdentificationNumber\":null,\"legalForm\":\"individual\",\"gender\":null,\"organisationName\":null,\"birthCity\":null,\"birthZip\":null,\"birthDay\":null,\"companyNationalIdentificationNumber\":null,\"phone\":\"0000000000\",\"firstName\":\"deleteme\",\"address\":{\"city\":\"deleteme\",\"otherDetails\":null,\"province\":null,\"line3\":null,\"line1\":\"deleteme\",\"line2\":null,\"zip\":\"00000\",\"country\":\"FR\"},\"spareEmail\":null,\"organisationType\":null,\"fax\":null,\"nationality\":null,\"vat\":null,\"id\":123456,\"cellPhone\":null}";
        public static HttpResponseMessage me_contact_message = new HttpResponseMessage{
            Content = new StringContent(me_contact_content, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK
        };
    }
}