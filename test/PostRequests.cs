using System.Net.Http;
using NUnit.Framework;
using Ovh.Api;
using Ovh.Api.Testing;
using System;
using FakeItEasy;
using System.Linq;
using Ovh.Test.Models;
using Newtonsoft.Json;

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
        public void POST_with_no_data_and_result_as_string()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/geolocation"))))
                .Returns(Responses.Post.me_geolocation_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = c.PostAsync("/me/geolocation", null).Result;
            Assert.AreEqual(Responses.Post.me_geolocation_content, result);

            var geolocCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/geolocation")).First();

            var requestMessage = geolocCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Constants.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Constants.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Constants.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$3473ad8790d09e6d28f8a9d6f09a05c1f5f0bbfc", headers.GetValues(Constants.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public void POST_with_no_data_and_result_as_T()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/geolocation"))))
                .Returns(Responses.Post.me_geolocation_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = c.PostAsync<Geolocation>("/me/geolocation", null).Result;
            Assert.AreEqual("eo", result.countryCode);
            Assert.AreEqual("256.0.0.1", result.ip);
            Assert.AreEqual("Atlantis", result.continent);
        }

        [Test]
        public void POST_with_string_data_and_result_as_string()
        {
            var testHandler = A.Fake<FakeHttpMessageHandler>(a => a.CallsBaseMethods());
            MockAuthTimeCallWithFakeItEasy(testHandler);
            A.CallTo(() =>
                testHandler.Send(A<HttpRequestMessage>.That.Matches(
                    r => r.RequestUri.ToString().Contains("/me/contact"))))
                .Returns(Responses.Post.me_contact_message);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = c.PostAsync("/me/contact", "Fake content").Result;
            Assert.AreEqual(Responses.Post.me_contact_content, result);

            var contactCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/contact")).First();

            var requestMessage = contactCall.GetArgument<HttpRequestMessage>("request");
            var headers = requestMessage.Headers;
            Assert.Multiple(() => {
                Assert.AreEqual(Constants.APPLICATION_KEY, headers.GetValues(Constants.OVH_APP_HEADER).First());
                Assert.AreEqual(Constants.CONSUMER_KEY, headers.GetValues(Constants.OVH_CONSUMER_HEADER).First());
                Assert.AreEqual(currentServerTimestamp.ToString(), headers.GetValues(Constants.OVH_TIME_HEADER).First());
                Assert.AreEqual("$1$19a8f2db1a3b2b89b231c7872332b6ba117d8bd7", headers.GetValues(Constants.OVH_SIGNATURE_HEADER).First());
            });
        }

        [Test]
        public void POST_with_T_data_and_result_as_string()
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

            var lol = JsonConvert.SerializeObject(dummyContact);

            var c = ClientFactory.GetClient(testHandler).AsTestable(timeProvider);
            var result = c.PostAsync<Contact, Contact>("/me/contact", dummyContact).Result;

            //Ensure that the call went through correctly
            Assert.AreEqual(123456, result.id);

            var contactCall = Fake.GetCalls(testHandler).Where(call =>
                call.Method.Name == "Send" &&
                call.GetArgument<HttpRequestMessage>("request").RequestUri.ToString().Contains("/me/contact")).First();

            var requestMessage = contactCall.GetArgument<HttpRequestMessage>("request");

            // Ensure that we sent a serialized version of the dummy contact
            var sendtObject = JsonConvert.DeserializeObject<Contact>(requestMessage.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(dummyContact.address.country, sendtObject.address.country);
            Assert.AreEqual(dummyContact.address.zip, sendtObject.address.zip);
            Assert.AreEqual(dummyContact.email, sendtObject.email);
        }
    }
}

