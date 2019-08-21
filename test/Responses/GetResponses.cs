using System.Net;
using System.Net.Http;
using System.Text;

namespace Ovh.Test.Responses
{
    public static class Get
    {
        public static string me_content = "{\"sex\":null,\"phoneCountry\":\"FR\",\"currency\":{\"code\":\"EUR\",\"symbol\":\"€\"},\"nichandle\":\"none-ovh\",\"name\":\"Noname\",\"legalform\":\"individual\",\"city\":\"sin\",\"firstname\":\"nofname\",\"language\":\"eo_EO\",\"customerCode\":\"0000-0000-00\",\"nationalIdentificationNumber\":null,\"phone\":\"+33.000000000\",\"ovhSubsidiary\":\"EO\",\"email\":\"none@dev.null\",\"organisation\":\"\",\"area\":\"\",\"zip\":\"00000\",\"fax\":\"\",\"spareEmail\":null,\"address\":\"somewhere\",\"vat\":\"\",\"state\":\"complete\",\"birthCity\":\"\",\"country\":\"FR\",\"ovhCompany\":\"ovh\",\"corporationType\":\"\",\"italianSDI\":null,\"birthDay\":\"\",\"companyNationalIdentificationNumber\":null}";
        public static HttpResponseMessage me_message = new HttpResponseMessage{
            Content = new StringContent(me_content, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK
        };

        public static string time_content = "1566485767";
        public static HttpResponseMessage time_message = new HttpResponseMessage{
            Content = new StringContent(time_content, Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK
        };

        public static HttpResponseMessage empty_message = new HttpResponseMessage{
            Content = new StringContent("", Encoding.UTF8, "application/json"),
            StatusCode = HttpStatusCode.OK
        };
    }
}