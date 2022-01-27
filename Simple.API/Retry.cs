using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Simple.API
{
    /// <summary>
    /// Class to retry ClientInfo Calls
    /// </summary>
    public class Retry
    {
        Dictionary<HttpStatusCode, Func<bool>> dicHandlers;
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public Retry()
        {
            dicHandlers = new Dictionary<HttpStatusCode, Func<bool>>();
        }
        /// <summary>
        /// Register a ResponseCode handler
        /// </summary>
        /// <param name="responseCode">ResponseCode to trigger for</param>
        /// <param name="action">Action to be executed. True to retry, False to pass</param>
        public Retry Register(HttpStatusCode responseCode, Func<bool> action)
        {
            dicHandlers[responseCode] = action;
            return this;
        }
        /// <summary>
        /// Unregister a handler
        /// </summary>
        public Retry Unregister(HttpStatusCode responseCode)
        {
            if (dicHandlers.ContainsKey(responseCode)) dicHandlers.Remove(responseCode);
            return this;
        }
        /// <summary>
        /// Get all registered handlers
        /// </summary>
        public HttpStatusCode[] GetRegisteredHandlers()
        {
            return dicHandlers.Keys.ToArray();
        }
        /// <summary>
        /// Exeutes a retriable call.
        /// The call will retry once
        /// </summary>
        public async Task<T> DoAsync<T>(Func<Task<T>> func)
            where T : Response 
        {
            // First execution
            var r = await func();
            // there is any handler?
            if (dicHandlers.ContainsKey(r.StatusCode))
            {
                // executes handler
                var result = dicHandlers[r.StatusCode]();
                // shoul try again?
                if (result)
                {
                    // not recursive
                    return await func();
                }
            }

            return r;
        }
    }
}
