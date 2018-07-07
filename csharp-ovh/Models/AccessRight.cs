//Copyright(c) 2013-2016, OVH SAS.
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//  * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.

// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.

// * Neither the name of OVH SAS nor the
//   names of its contributors may be used to endorse or promote products
//   derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY OVH SAS AND CONTRIBUTORS ``AS IS'' AND ANY
//EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED.IN NO EVENT SHALL OVH SAS AND CONTRIBUTORS BE LIABLE FOR ANY
//DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ovh.Api.Models
{

    /// <summary>
    /// Class representing API Access rights composed of paths and methods
    /// </summary>
    public class AccessRight
    {
        private string _method;

        /// <summary>
        /// HTTP Method to authorize
        /// </summary>
        [JsonProperty(PropertyName = "method")]
        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value.ToUpper();
            }
        }

        /// <summary>
        /// API resource to authorize access to
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        /// <summary>
        /// Initializes an AccessRight based on <paramref name="path"/> and <paramref name="method"/>
        /// </summary>
        /// <param name="method">HTTP Method to authorize</param>
        /// <param name="path">API resource to authorize access to</param>
        public AccessRight(string method, string path)
        {
            Method = method;
            Path = path;
        }

        /// <summary>
        /// Initializes an AccessRight based on a tuple composed of a path and a method
        /// </summary>
        public AccessRight(Tuple<string, string> rule)
            : this(rule.Item1, rule.Item2)
        {
        }
    }
}