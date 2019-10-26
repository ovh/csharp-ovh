using System.Net.Http;
using NUnit.Framework;
using Ovh.Api;
using Ovh.Api.Testing;
using System;
using FakeItEasy;
using System.Linq;
using Ovh.Test.Models;
using System.Threading.Tasks;

namespace Ovh.Test
{
    [TestFixture]
    public class GetRequests
    {
        static long currentClientTimestamp = 1566485765;
        static long currentServerTimestamp = 1566485767;
        static DateTimeOffset currentDateTime = DateTimeOffset.FromUnixTimeSeconds(currentClientTimestamp);
        static ITimeProvider timeProvider = A.Fake<ITimeProvider>();

        public GetRequests()
        {
            A.CallTo(() => timeProvider.UtcNow).Returns(currentDateTime);
        }

        public static void MockAuthTimeCallWithFakeItEasy(FakeHttpMessageHandler fake)
        {
            A.CallTo(() =>
                fake.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/auth/time"))))
                .Returns(Responses.Get.time_message);
        }

        [Test]
        public void GET_auth_time()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);

            var httpClient = new HttpClient(testHandler);
            var c = new Client("ovh-eu", httpClient: httpClient).AsTestable(timeProvider);

            Assert.AreEqual(2, c.TimeDelta);
        }

        [Test]
        public async Task GET_me_as_string()
        {
            var fake = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(fake);

            A.CallTo(() =>
                fake.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me"))))
                .Returns(Responses.Get.me_message);


            var c = ClientFactory.GetClient(fake);
            var result = await c.GetAsync("/me");
            Assert.AreEqual(Responses.Get.me_content, result);

            var meCall = Fake.GetCalls(fake).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me")).First();

            var requestMessage = meCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Client.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Client.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Client.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$dfe0b86bf2ab0d9eb3f785dc1ab00de58984d80c", headers.GetValues(Client.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public async Task GET_me_as_T()
        {
            var fake = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(fake);

            A.CallTo(() =>
                fake.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me"))))
                .Returns(Responses.Get.me_message);


            var c = ClientFactory.GetClient(fake);
            var result = await c.GetAsync<Me>("/me");

            Assert.AreEqual("Noname", result.name);
            Assert.AreEqual("none-ovh", result.nichandle);
            Assert.AreEqual("EUR", result.currency.code);
            Assert.AreEqual("€", result.currency.symbol);
        }

        [Test]
        public async Task GET_with_filter_generates_correct_signature()
        {
            var fake = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(fake);

            A.CallTo(() =>
                fake.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/route"))))
                .Returns(Responses.Get.empty_message);


            var c = ClientFactory.GetClient(fake);
            var queryParams = new QueryStringParams();
            queryParams.Add("filter", "value:&é'-");
            queryParams.Add("anotherfilter", "=test");
            _ = await c.GetAsync("/route", queryParams);


            var meCall = Fake.GetCalls(fake).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/route")).First();

            var requestMessage = meCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.AreEqual("$1$098b93d342b6db4848ec448063be2b6884e94723", headers.GetValues(Client.OVH_SIGNATURE_HEADER).First());
        }
    }
}

