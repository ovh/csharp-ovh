using System.Net.Http;
using NUnit.Framework;
using Ovh.Api;

namespace Ovh.Test
{
    [TestFixture]
    public class Signatures
    {
        static long currentServerTimestamp = 1566485767;

        [Test]
        [TestCase("/", "$1$c87db4be631d68b610079e088248dfe8881c40a1")]
        [TestCase("/me", "$1$b395d13fdb1e1aae1db2be96c6d120677c4d2000")]
        [TestCase("/auth/time", "$1$3e26ff19629bd612b1e688f1ef61b6446b5c686e")]
        [TestCase("/mails/accounts?id=5", "$1$0abd51a142538aec522fd4882e178e695b5458c4")]
        [TestCase("/mails?to=45&from=12", "$1$26b79c495a18b45bb1f197bfe3f262ecb5f21016")]
        public void generate_signature_for_a_get_call(string target, string expectedSignature)
        {
            var sig = Client.GenerateSignature(
                Constants.APPLICATION_SECRET,
                Constants.CONSUMER_KEY,
                currentServerTimestamp,
                "GET",
                target);

            Assert.AreEqual(expectedSignature, sig);
        }

        [Test]
        [TestCase("/", "$1$71d90b786d8200b46ff2736400ee416d5fe41079")]
        [TestCase("/me", "$1$0901964347daac6ab470f35286114a0add0ba399")]
        [TestCase("/auth/time", "$1$a4b70a7521cc6b397d84c9de2c0066f82a2ac277")]
        [TestCase("/mails/accounts?id=5", "$1$6d2c0ba0ab22629c6739a5f450cc47aeb785ab78")]
        [TestCase("/mails?to=45&from=12", "$1$7bb0e50be918aa6739ef1a183c7c76e73a79494f")]
        public void generate_signature_for_a_delete_call(string target, string expectedSignature)
        {
            var sig = Client.GenerateSignature(
                Constants.APPLICATION_SECRET,
                Constants.CONSUMER_KEY,
                currentServerTimestamp,
                "DELETE",
                target);

            Assert.AreEqual(expectedSignature, sig);
        }

        [Test]
        [TestCase("/", "$1$4a09d6cf15fc021ffb73da3c24797b2641cf2424")]
        [TestCase("/me", "$1$36c9811602d49b74f4374aafc0f1f26ff6724293")]
        [TestCase("/auth/time", "$1$a0ce8be3ebc95a4168237abaca254bed535601b0")]
        [TestCase("/mails/accounts?id=5", "$1$ded453bab00328fe1753768db2926cf931a1e0e6")]
        [TestCase("/mails?to=45&from=12", "$1$62904b4a7be1298f263af4b5a38ea384132906f8")]
        public void generate_signature_for_a_post_call_without_data(string target, string expectedSignature)
        {
            var sig = Client.GenerateSignature(
                Constants.APPLICATION_SECRET,
                Constants.CONSUMER_KEY,
                currentServerTimestamp,
                "POST",
                target);

            Assert.AreEqual(expectedSignature, sig);
        }

        [Test]
        [TestCase("/", "{\"data\":\"value\"}", "$1$369d08d098480a4769db04bf3f0ce8e7a266ca7e")]
        [TestCase("/me", "some_data", "$1$6a0e930b55e9c6d8b695084782f567031d5230ed")]
        [TestCase("/auth/time", "[{\"data\":\"value\"},{\"data-2\":\"value-2\"}]", "$1$7d0a0141e32070c6046890a59afa4f034c08a3f0")]
        [TestCase("/mails/accounts?id=5", "{\"data\":\"value\"}", "$1$42b3029887864b9191302ccddc7229846b11c353")]
        [TestCase("/mails?to=45&from=12", "{\"data\":\"value\"}", "$1$e23dcacc5411034a832e05c7fe9f7b2ae5c9578f")]
        public void generate_signature_for_a_post_call_with_data(string target, string data, string expectedSignature)
        {
            var sig = Client.GenerateSignature(
                Constants.APPLICATION_SECRET,
                Constants.CONSUMER_KEY,
                currentServerTimestamp,
                "POST",
                target,
                data);

            Assert.AreEqual(expectedSignature, sig);
        }

        [Test]
        [TestCase("/", "$1$c1fac5e97db0830c9bd33bad6181e317c33ae6cd")]
        [TestCase("/me", "$1$725acf08ac67cf402679b91791cda17eba656b26")]
        [TestCase("/auth/time", "$1$1857d9e775b2ab963244623ae3f97362a529c5a5")]
        [TestCase("/mails/accounts?id=5", "$1$72d379b8cb16ff4258e26e9f8c9aacb1d6958c3e")]
        [TestCase("/mails?to=45&from=12", "$1$1fbaa78566ac80f6c73169f4798fc29cc454dcb1")]
        public void generate_signature_for_a_put_call_without_data(string target, string expectedSignature)
        {
            var sig = Client.GenerateSignature(
                Constants.APPLICATION_SECRET,
                Constants.CONSUMER_KEY,
                currentServerTimestamp,
                "PUT",
                target);

            Assert.AreEqual(expectedSignature, sig);
        }

        [Test]
        [TestCase("/", "{\"data\":\"value\"}", "$1$ee4b4ac586796b17062397a7a390349306898e51")]
        [TestCase("/me", "some_data", "$1$386dc87d403f526a1e2a6c75b915a4ecedf0d224")]
        [TestCase("/auth/time", "[{\"data\":\"value\"},{\"data-2\":\"value-2\"}]", "$1$a991d5bad1a5c8ddfff315668a7c4842b24958ef")]
        [TestCase("/mails/accounts?id=5", "{\"data\":\"value\"}", "$1$590f78e1c78eec4e163afe36bf7c157d9244b682")]
        [TestCase("/mails?to=45&from=12", "{\"data\":\"value\"}", "$1$dacdb2f3407325e3fd0e6c8c3b797dca08d4491f")]
        public void generate_signature_for_a_put_call_with_data(string target, string data, string expectedSignature)
        {
            var sig = Client.GenerateSignature(
                Constants.APPLICATION_SECRET,
                Constants.CONSUMER_KEY,
                currentServerTimestamp,
                "PUT",
                target,
                data);

            Assert.AreEqual(expectedSignature, sig);
        }

    }
}

