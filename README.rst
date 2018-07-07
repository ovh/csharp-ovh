.. image:: https://travis-ci.org/ovh/csharp-ovh.svg?branch=master
    :target: https://travis-ci.org/ovh/csharp-ovh

Lightweight wrapper around OVH's APIs. Handles all the hard work including
credential creation and requests signing.

.. code:: csharp

    using Ovh.Api;
    using System;

    namespace api_tester
    {
        class Program
        {
            static void Main(string[] args)
            {
                // Instantiate. Visit https://api.ovh.com/createToken/index.cgi?GET=/me
                // to get your credentials
                Client client = new Client("ovh-eu", "<application_key>", "<application_secret>", "<consumer_key>");
                PartialMe me = client.Get<PartialMe>("/me");

                // Print nice welcome message
                Console.WriteLine(String.Format("Hello {0}!", me.firstname));
                Console.ReadLine();
            }
        }

        class PartialMe
        {
            public string firstname { get; set; }
            public string name { get; set; }
        }
    }

Installation
============

The easiest way to get the latest stable release is to grab it from `NuGet
<https://www.nuget.org>`_.

.. code:: bash

    nuget install ovh

Example Usage
=============

Use the API on behalf of a user
-------------------------------

1. Create an application
************************

To interact with the APIs, the SDK needs to identify itself using an
``application_key`` and an ``application_secret``. To get them, you need
to register your application. Depending the API you plan to use, visit:

- `OVH Europe <https://eu.api.ovh.com/createApp/>`_
- `OVH US <https://api.ovhcloud.com/createApp/>`_
- `OVH North-America <https://ca.api.ovh.com/createApp/>`_
- `So you Start Europe <https://eu.api.soyoustart.com/createApp/>`_
- `So you Start North America <https://ca.api.soyoustart.com/createApp/>`_
- `Kimsufi Europe <https://eu.api.kimsufi.com/createApp/>`_
- `Kimsufi North America <https://ca.api.kimsufi.com/createApp/>`_
- `RunAbove <https://api.runabove.com/createApp/>`_

Once created, you will obtain an **application key (AK)** and an **application
secret (AS)**.

2. Configure your application
*****************************

The easiest and safest way to use your application's credentials is to create a
``.ovh.conf`` configuration file in application's working directory. Here is how
it looks like:

.. code:: ini

    [default]
    ; general configuration: default endpoint
    endpoint=ovh-eu

    [ovh-eu]
    ; configuration specific to 'ovh-eu' endpoint
    application_key=my_app_key
    application_secret=my_application_secret
    ; uncomment following line when writing a script application
    ; with a single consumer key.
    ;consumer_key=my_consumer_key

Depending on the API you want to use, you may set the ``endpoint`` to:

* ``ovh-eu`` for OVH Europe API
* ``ovh-us`` for OVH US API
* ``ovh-ca`` for OVH North-America API
* ``soyoustart-eu`` for So you Start Europe API
* ``soyoustart-ca`` for So you Start North America API
* ``kimsufi-eu`` for Kimsufi Europe API
* ``kimsufi-ca`` for Kimsufi North America API
* ``runabove-ca`` for RunAbove API

See Configuration_ for more information on available configuration mechanisms.

.. note:: When using a versioning system, make sure to add ``.ovh.conf`` to ignored
          files. It contains confidential/security-sensitive informations!

3. Authorize your application to access a customer account
**********************************************************

To allow your application to access a customer account using the API on your
behalf, you need a **consumer key (CK)**.

Here is a sample code you can use to allow your application to access a
customer's informations:

.. code:: csharp

    using Ovh.Api;
    using System;
    using System.Collections.Generic;

    namespace api_tester
    {
        class Program
        {
            static void Main(string[] args)
            {
                Client client = new Client();
                CredentialRequest requestPayload = new CredentialRequest(
                    new List<AccessRight>(){
                        new AccessRight("GET", "/me"),
                    }
                );

                CredentialRequestResult credentialRequestResult = 
                    client.RequestConsumerKey(requestPayload);
                Console.Write(
                    String.Format("Please visit {0} to authenticate ", 
                        credentialRequestResult.ValidationUrl));
                Console.WriteLine("and press enter to continue");
                Console.ReadLine();

                PartialMe me = client.Get<PartialMe>("/me");

                Console.WriteLine(
                    String.Format("Welcome, {0}", me.firstname));
                Console.WriteLine(
                    String.Format("Btw, your 'consumerKey' is {0}",
                        credentialRequestResult.ConsumerKey));
                Console.ReadLine();
            }
        }

        class PartialMe
        {
            public string firstname { get; set; }
            public string name { get; set; }
        }
    }



Returned ``consumerKey`` should then be kept to avoid re-authenticating your
end-user on each use.

.. note:: To request full and unlimited access to the API, you may use wildcards:

.. code:: csharp

    new List<AccessRight>(){
        new AccessRight("GET", "/*"),
        new AccessRight("PUT", "/*"),
        new AccessRight("POST", "/*"),
        new AccessRight("DELETE", "/*"),
    }

Install a new mail redirection
------------------------------

e-mail redirections may be freely configured on domains and DNS zones hosted by
OVH to an arbitrary destination e-mail using API call
``POST /email/domain/{domain}/redirection``.

.. code:: csharp

    using Ovh.Api;
    using System;
    using System.Collections.Generic;

    namespace api_tester
    {
        class Program
        {
            static void Main(string[] args)
            {
                Client client = new Client();

                string domain = "<someDomain>";
                string source = "<sourceEmail>";
                string destination = "<destinationEmail>";

                Dictionary<string, object> payload = new Dictionary<string, object>();
                payload.Add("from", source);
                payload.Add("to", destination);
                payload.Add("localCopy", false);

                client.Post(String.Format("/email/domain/{0}/redirection", domain), payload);

                Console.WriteLine(
                    String.Format("Installed new mail redirection from {0} to {1}", 
                        source, destination));
                Console.ReadLine();
            }
        }
    }



Grab bill list
--------------

Let's say you want to integrate OVH bills into your own billing system, you
could just script around the ``/me/bills`` endpoints and even get the details
of each bill lines using ``/me/bill/{billId}/details/{billDetailId}``.

This example assumes an existing Configuration_ with valid ``application_key``,
``application_secret`` and ``consumer_key``.

.. code:: csharp

    using Ovh.Api;
    using System;
    using System.Collections.Generic;

    namespace api_tester
    {
        class Program
        {
            static void Main(string[] args)
            {
                Client client = new Client();
                var billIds = client.Get<List<string>>("/me/bill");
                foreach (var billId in billIds)
                {
                    PartialOvhBill details = client.Get<PartialOvhBill>("/me/bill/" + billId);
                    Console.WriteLine(
                        String.Format("{0} ({1}): {2} --> {3}",
                            billId, details.date, details.priceWithTax.text, details.pdfUrl));
                }
                Console.ReadLine();
            }
        }

        class PartialOvhBill
        {
            public string date { get; set; }
            public string pdfUrl { get; set; }

            public OvhPrice priceWithTax { get; set; }
        }

        class OvhPrice
        {
            public string currencyCode { get; set; }
            public double value { get; set; }
            public string text { get; set; }
        }
    }


Enable network burst in SBG1
----------------------------

'Network burst' is a free service but is opt-in. What if you have, say, 10
servers in ``SBG-1`` datacenter? You certainely don't want to activate it
manually for each servers. You could take advantage of a code like this.

This example assumes an existing Configuration_ with valid ``application_key``,
``application_secret`` and ``consumer_key``.

.. code:: csharp

    using Ovh.Api;
    using System;
    using System.Collections.Generic;

    namespace api_tester
    {
        class Program
        {
            static void Main(string[] args)
            {
                Client client = new Client();

                var serverIds = client.Get<List<string>>("/dedicated/server/");
                foreach (var serverId in serverIds)
                {
                    string serverUrl = "/dedicated/server/" + serverId;
                    var details = client.Get<PartialDedicatedServer>(serverUrl);
                    if (details.datacenter == "sbg1")
                    {
                        client.Put(serverUrl + "/burst", "{\"status\":\"active\"}");
                        Console.WriteLine("Burst enabled on server " + serverId);
                    }
                }
                Console.ReadLine();
            }
        }

        class PartialDedicatedServer
        {
            public string datacenter { get; set; }
        }
    }


List application authorized to access your account
--------------------------------------------------

Thanks to the application key / consumer key mechanism, it is possible to
finely track applications having access to your data and revoke this access.
This examples lists validated applications. It could easily be adapted to
manage revocation too.

This example assumes an existing Configuration_ with valid ``application_key``,
``application_secret`` and ``consumer_key``.

.. code:: csharp

    using Ovh.Api;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text;

    namespace api_tester
    {
        class Program
        {
            static void Main(string[] args)
            {
                Client client = new Client();

                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("status", "validated");

                var credentialIds = client.Get<List<string>>("/me/api/credential", nvc);
                foreach (var credentialId in credentialIds)
                {
                    string credentialUrl = "/me/api/credential/" + credentialId;
                    var credential = client.Get<PartialCredential>(credentialUrl);
                    var application = client.Get<PartialApplication>(credentialUrl + "/application");

                    StringBuilder sb = new StringBuilder();
                    sb.Append(credentialId).Append(" ").Append(application.status)
                        .Append(" ").Append(application.name).Append(" ")
                        .Append(application.description).Append(" ")
                        .Append(credential.creation).Append(" ")
                        .Append(credential.expiration).Append(" ")
                        .Append(credential.lastUse);
                    Console.WriteLine(sb.ToString());
                }
                Console.ReadLine();
            }
        }

        class PartialCredential
        {
            public string creation{ get; set; }
            public string expiration { get; set; }
            public string lastUse { get; set; }
        }

        class PartialApplication
        {
            public string name { get; set; }
            public string description { get; set; }
            public string status { get; set; }
        }
    }

Configuration
=============

The straightforward way to use OVH's API keys is to embed them directly in the
application code. While this is very convenient, it lacks of elegance and
flexibility.

Alternatively it is suggested to use configuration files or environment
variables so that the same code may run seamlessly in multiple environments.
Production and development for instance.

This wrapper will first look for direct instanciation parameters then
``OVH_ENDPOINT``, ``OVH_APPLICATION_KEY``, ``OVH_APPLICATION_SECRET`` and
``OVH_CONSUMER_KEY`` environment variables. If either of these parameter is not
provided, it will look for a configuration file of the form:

.. code:: ini

    [default]
    ; general configuration: default endpoint
    endpoint=ovh-eu

    [ovh-eu]
    ; configuration specific to 'ovh-eu' endpoint
    application_key=my_app_key
    application_secret=my_application_secret
    consumer_key=my_consumer_key

The client will successively attempt to locate this configuration file in

1. Current working directory: ``./.ovh.conf``
2. Current user's home directory ``%USERPROFILE%/.ovh.conf``

This lookup mechanism makes it easy to overload credentials for a specific
project or user.

Supported APIs
==============

OVH Europe
----------

- **Documentation**: https://eu.api.ovh.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://eu.api.ovh.com/console
- **Create application credentials**: https://eu.api.ovh.com/createApp/
- **Create script credentials** (all keys at once): https://eu.api.ovh.com/createToken/

OVH US
----------

- **Documentation**: https://api.ovhcloud.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://api.ovhcloud.com/console/
- **Create application credentials**: https://api.ovhcloud.com/createApp/
- **Create script credentials** (all keys at once): https://api.ovhcloud.com/createToken/

OVH North America
-----------------

- **Documentation**: https://ca.api.ovh.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://ca.api.ovh.com/console
- **Create application credentials**: https://ca.api.ovh.com/createApp/
- **Create script credentials** (all keys at once): https://ca.api.ovh.com/createToken/

So you Start Europe
-------------------

- **Documentation**: https://eu.api.soyoustart.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://eu.api.soyoustart.com/console/
- **Create application credentials**: https://eu.api.soyoustart.com/createApp/
- **Create script credentials** (all keys at once): https://eu.api.soyoustart.com/createToken/

So you Start North America
--------------------------

- **Documentation**: https://ca.api.soyoustart.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://ca.api.soyoustart.com/console/
- **Create application credentials**: https://ca.api.soyoustart.com/createApp/
- **Create script credentials** (all keys at once): https://ca.api.soyoustart.com/createToken/

Kimsufi Europe
--------------

- **Documentation**: https://eu.api.kimsufi.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://eu.api.kimsufi.com/console/
- **Create application credentials**: https://eu.api.kimsufi.com/createApp/
- **Create script credentials** (all keys at once): https://eu.api.kimsufi.com/createToken/

Kimsufi North America
---------------------

- **Documentation**: https://ca.api.kimsufi.com/
- **Community support**: api-subscribe@ml.ovh.net
- **Console**: https://ca.api.kimsufi.com/console/
- **Create application credentials**: https://ca.api.kimsufi.com/createApp/
- **Create script credentials** (all keys at once): https://ca.api.kimsufi.com/createToken/
