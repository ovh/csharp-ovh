using System.Net.Http;
using NUnit.Framework;
using Ovh.Api;
using Ovh.Api.Testing;
using System;
using FakeItEasy;
using System.Linq;
using Ovh.Test.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Ovh.Test
{
    [TestFixture]
    public class PostRequests
    {
        static long currentClientTimestamp = 1566485765;
        static long currentServerTimestamp = 1566485767;
        static DateTimeOffset currentDateTime = DateTimeOffset.FromUnixTimeSeconds(currentClientTimestamp);
        static ITimeProvider timeProvider = A.Fake<ITimeProvider>();

        public PostRequests()
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
        public async Task POST_with_no_data_and_result_as_string()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/geolocation"))))
                .Returns(Responses.Post.me_geolocation_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = await c.PostAsync("/me/geolocation", null);
            Assert.AreEqual(Responses.Post.me_geolocation_content, result);

            var geolocCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/geolocation")).First();

            var requestMessage = geolocCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Client.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Client.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Client.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$39be1fc4d5de5943625757c66502b7795eea5d33", headers.GetValues(Client.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public async Task POST_with_no_data_and_result_as_T()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/geolocation"))))
                .Returns(Responses.Post.me_geolocation_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = await c.PostAsync<Geolocation>("/me/geolocation");
            Assert.AreEqual("eo", result.countryCode);
            Assert.AreEqual("256.0.0.1", result.ip);
            Assert.AreEqual("Atlantis", result.continent);
        }

        [Test]
        public async Task POST_with_raw_string_data_and_string_result()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/contact"))))
                .Returns(Responses.Post.me_contact_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = await c.PostStringAsync("/me/contact", "Fake content");
            Assert.AreEqual(Responses.Post.me_contact_content, result);

            var contactCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/contact")).First();

            var requestMessage = contactCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Client.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Client.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Client.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$a653bd5fc3140295fb5667af029a538fc011b1e6", headers.GetValues(Client.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public async Task POST_with_string_to_be_serialized_data_and_T_result()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/contact"))))
                .Returns(Responses.Post.me_contact_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = await c.PostAsync<Contact>("/me/contact", "Fake content");
            Assert.AreEqual("deleteme@deleteme.deleteme", result.email);

            var contactCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/contact")).First();

            var requestMessage = contactCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Client.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Client.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Client.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$9246fa2a1f62c9629982b8c22d8397c2aa2fa510", headers.GetValues(Client.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public async Task POST_with_string_to_be_serialized_data_and_string_result()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/contact"))))
                .Returns(Responses.Post.me_contact_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = await c.PostAsync("/me/contact", "Fake content");
            Assert.AreEqual(Responses.Post.me_contact_content, result);

            var contactCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/contact")).First();

            var requestMessage = contactCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Client.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Client.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Client.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$9246fa2a1f62c9629982b8c22d8397c2aa2fa510", headers.GetValues(Client.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public async Task POST_with_T_data_and_string_result()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            var dummyContact = new Contact{
                address = new Address{
                    city = "deleteme",
                    country = "FR",
                    line1 = "deleteme",
                    zip = "00000"
                },
                email = "deleteme@deleteme.deleteme",
                firstName = "deleteme",
                language = "fr_FR",
                lastName = "deleteme",
                legalForm = "individual",
                phone = "0000000000"
            };
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/contact"))))
                .Returns(Responses.Post.me_contact_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = await c.PostAsync<Contact>("/me/contact", dummyContact);

            //Ensure that the call went through correctly
            Assert.AreEqual(123456, result.id);

            var contactCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/contact")).First();

            var requestMessage = contactCall.GetArgument<HttpRequestMessage>("request");

            // Ensure that we sent a serialized version of the dummy contact
            var sendtObject = JsonConvert.DeserializeObject<Contact>(await requestMessage.Content.ReadAsStringAsync());
            Assert.AreEqual(dummyContact.address.country, sendtObject.address.country);
            Assert.AreEqual(dummyContact.address.zip, sendtObject.address.zip);
            Assert.AreEqual(dummyContact.email, sendtObject.email);
        }
    }
}

