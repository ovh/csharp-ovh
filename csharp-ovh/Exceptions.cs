using System;

namespace Ovh.Api.Exceptions
{
    /// <summary>
    /// Base OVH API exception, all specific exceptions inherits from it.
    /// </summary>
    [Serializable]
    public class ApiException : Exception
    {
        public ApiException() { }
        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception inner) : base(message, inner) { }
        protected ApiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when the request fails at a low level (DNS, network, ...)
    /// </summary>
    [Serializable]
    public class HttpException : ApiException
    {
        public HttpException() { }
        public HttpException(string message) : base(message) { }
        public HttpException(string message, Exception inner) : base(message, inner) { }
        protected HttpException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when trying to sign request with invalid key
    /// </summary>
    [Serializable]
    public class InvalidKeyException : ApiException
    {
        public InvalidKeyException() { }
        public InvalidKeyException(string message) : base(message) { }
        public InvalidKeyException(string message, Exception inner) : base(message, inner) { }
        protected InvalidKeyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when trying to sign request with invalid consumer key
    /// </summary>
    [Serializable]
    public class InvalidCredentialException : ApiException
    {
        public InvalidCredentialException() { }
        public InvalidCredentialException(string message) : base(message) { }
        public InvalidCredentialException(string message, Exception inner) : base(message, inner) { }
        protected InvalidCredentialException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class InvalidResponseException : ApiException
    {
        public InvalidResponseException() { }
        public InvalidResponseException(string message) : base(message) { }
        public InvalidResponseException(string message, Exception inner) : base(message, inner) { }
        protected InvalidResponseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when region is not in "REGIONS".
    /// </summary>
    [Serializable]
    public class InvalidRegionException : ApiException
    {
        public InvalidRegionException() { }
        public InvalidRegionException(string message) : base(message) { }
        public InvalidRegionException(string message, Exception inner) : base(message, inner) { }
        protected InvalidRegionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when attempting to modify readonly data.
    /// </summary>
    [Serializable]
    public class ReadOnlyException : ApiException
    {
        public ReadOnlyException() { }
        public ReadOnlyException(string message) : base(message) { }
        public ReadOnlyException(string message, Exception inner) : base(message, inner) { }
        protected ReadOnlyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when requested resource does not exist.
    /// </summary>
    [Serializable]
    public class ResourceNotFoundException : ApiException
    {
        public ResourceNotFoundException() { }
        public ResourceNotFoundException(string message) : base(message) { }
        public ResourceNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ResourceNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when request contains bad parameters.
    /// </summary>
    [Serializable]
    public class BadParametersException : ApiException
    {
        public BadParametersException() { }
        public BadParametersException(string message) : base(message) { }
        public BadParametersException(string message, Exception inner) : base(message, inner) { }
        protected BadParametersException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when request is stale.
    /// </summary>
    [Serializable]
    public class StaleRequestException : ApiException
    {
        public StaleRequestException() { }
        public StaleRequestException(string message) : base(message) { }
        public StaleRequestException(string message, Exception inner) : base(message, inner) { }
        protected StaleRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when trying to create an already existing resource.
    /// </summary>
    [Serializable]
    public class ResourceConflictException : ApiException
    {
        public ResourceConflictException() { }
        public ResourceConflictException(string message) : base(message) { }
        public ResourceConflictException(string message, Exception inner) : base(message, inner) { }
        protected ResourceConflictException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    /// <summary>
    /// Raised when there is an error from network layer.
    /// </summary>
    [Serializable]
    public class NetworkException : ApiException
    {
        public NetworkException() { }
        public NetworkException(string message) : base(message) { }
        public NetworkException(string message, Exception inner) : base(message, inner) { }
        protected NetworkException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class NotGrantedCallException : ApiException
    {
        public NotGrantedCallException() { }
        public NotGrantedCallException(string message) : base(message) { }
        public NotGrantedCallException(string message, Exception inner) : base(message, inner) { }
        protected NotGrantedCallException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class NotCredentialException : ApiException
    {
        public NotCredentialException() { }
        public NotCredentialException(string message) : base(message) { }
        public NotCredentialException(string message, Exception inner) : base(message, inner) { }
        protected NotCredentialException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class ForbiddenException : ApiException
    {
        public ForbiddenException() { }
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, Exception inner) : base(message, inner) { }
        protected ForbiddenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class ConfigurationKeyMissingException : ApiException
    {
        public ConfigurationKeyMissingException() { }
        public ConfigurationKeyMissingException(string message) : base(message) { }
        public ConfigurationKeyMissingException(string message, Exception inner) : base(message, inner) { }
        protected ConfigurationKeyMissingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
