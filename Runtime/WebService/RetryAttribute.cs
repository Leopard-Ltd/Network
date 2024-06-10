namespace GameFoundation.Scripts.Network.WebService
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class RetryAttribute : Attribute
    {
        public int RetryCount { get; set; }

        public RetryAttribute(int retryCount) { this.RetryCount = retryCount; }
    }
}