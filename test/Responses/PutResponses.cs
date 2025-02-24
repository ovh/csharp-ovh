using System.Net;
using System.Net.Http;

namespace Ovh.Test.Responses
{
    public static class Put
    {
        public static readonly string me_contact_content = "{\"email\":\"deleteme@deleteme.deleteme\",\"lastName\":\"deleteme\",\"birthCountry\":null,\"language\":\"fr_FR\",\"nationalIdentificationNumber\":null,\"legalForm\":\"individual\",\"gender\":null,\"organisationName\":null,\"birthCity\":null,\"birthZip\":null,\"birthDay\":null,\"companyNationalIdentificationNumber\":null,\"phone\":\"0000000000\",\"firstName\":\"deleteme\",\"address\":{\"city\":\"deleteme\",\"otherDetails\":null,\"province\":null,\"line3\":null,\"line1\":\"deleteme\",\"line2\":null,\"zip\":\"00000\",\"country\":\"FR\"},\"spareEmail\":null,\"organisationType\":null,\"fax\":null,\"nationality\":null,\"vat\":null,\"id\":123456,\"cellPhone\":null}";
        public static readonly HttpResponseMessage me_contact_message =
            HttpResponseMessageFactory.Create(me_contact_content, HttpStatusCode.OK);
    }
    public static class PutWith204Response
    {
        public static readonly string success_204_response = "Success ; Response Status: No Content:204";
        public static readonly HttpResponseMessage response_204_message =
            HttpResponseMessageFactory.Create(success_204_response, HttpStatusCode.NoContent);
    }
}