using System.Net;
using System.Net.Http;

namespace Ovh.Test.Responses
{
    public static class Get
    {
        public static readonly string me_content = "{\"sex\":null,\"phoneCountry\":\"FR\",\"currency\":{\"code\":\"EUR\",\"symbol\":\"€\"},\"nichandle\":\"none-ovh\",\"name\":\"Noname\",\"legalform\":\"individual\",\"city\":\"sin\",\"firstname\":\"nofname\",\"language\":\"eo_EO\",\"customerCode\":\"0000-0000-00\",\"nationalIdentificationNumber\":null,\"phone\":\"+33.000000000\",\"ovhSubsidiary\":\"EO\",\"email\":\"none@dev.null\",\"organisation\":\"\",\"area\":\"\",\"zip\":\"00000\",\"fax\":\"\",\"spareEmail\":null,\"address\":\"somewhere\",\"vat\":\"\",\"state\":\"complete\",\"birthCity\":\"\",\"country\":\"FR\",\"ovhCompany\":\"ovh\",\"corporationType\":\"\",\"italianSDI\":null,\"birthDay\":\"\",\"companyNationalIdentificationNumber\":null}";
        public static readonly HttpResponseMessage me_message =
            HttpResponseMessageFactory.Create(me_content, HttpStatusCode.OK);

        public static readonly string time_content = "1566485767";
        public static readonly HttpResponseMessage time_message =
            HttpResponseMessageFactory.Create(time_content, HttpStatusCode.OK);

        public static readonly HttpResponseMessage empty_message =
            HttpResponseMessageFactory.Create("", HttpStatusCode.OK);
    }
}