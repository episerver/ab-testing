using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EPiServer.Marketing.Messaging
{
    /// <summary>
    /// The MessageHandlerInvoker is responsible for executing an action,
    /// described by a particular message handler, to process a message. 
    /// </summary>
    public class MessageHandlerInvoker
    {
        private readonly Dictionary<object, Action<object>> invocationCache;

        /// <summary>
        /// Constructor
        /// </summary>
        public MessageHandlerInvoker()
        {
            this.invocationCache = new Dictionary<object, Action<object>>();
        }

        /// <summary>
        /// Invokes the specified message handler to process a message.
        /// </summary>
        /// <param name="handler">Message handler to be invoked</param>
        /// <param name="message">Message to be processed</param>
        public void Invoke(object handler, object message)
        {
            this.GetHandlerInvocation(handler, message.GetType())
                .Invoke(message);
        }

        /// <summary>
        /// Gets the action described by the specified message handler from the
        /// cache. If the action has not been cached, it will be compiled
        /// cached prior to retrieving it.
        /// </summary>
        /// <param name="handler">Message handler to be invoked</param>
        /// <param name="message">Type of message to be processed</param>
        /// <returns>Action described by the specified message handler</returns>
        private Action<object> GetHandlerInvocation(object handler, Type messageType)
        {
            string invocationCacheKey = this.GetInvocationCacheKey(handler, messageType);

            if (!this.IsHandlerInvocationCached(invocationCacheKey))
            {
                this.CacheHandlerInvocation(handler, messageType);
            }

            return this.GetCachedHandlerInvocation(invocationCacheKey);
        }

        /// <summary>
        /// Returns a key identifying the cached action to invoke to handle the
        /// specified message.
        /// </summary>
        /// <param name="handler">Handler exposing the action</param>
        /// <param name="messageType">Type of message to handle</param>
        /// <returns>Key identifying the action</returns>
        private string GetInvocationCacheKey(object handler, Type messageType)
        {
            return handler.GetHashCode() + "_" + messageType.Name;
        }

        /// <summary>
        /// Compiles the action described by the specified handler and adds
        /// it to the cache.
        /// </summary>
        /// <param name="handler">Handler describing the action to cache</param>
        /// <param name="compiledHandler">Type of message handled by the action</param>
        private void CacheHandlerInvocation(object handler, Type messageType)
        {
            this.invocationCache.Add(
                this.GetInvocationCacheKey(handler, messageType), 
                this.CompileHandlerInvocation(handler, messageType)
            );
        }

        /// <summary>
        /// Dynamically compiles an action to invoke the specified message 
        /// handler's "Handle" method.
        /// </summary>
        /// <remarks>
        /// This method compiles a lambda expression to an Action delegate which 
        /// invokes the appropriate "Handle" method on the specified handler. The 
        /// compiled action can subsequently be cached and reused. This approach 
        /// performs better than more traditional reflection alternatives, such 
        /// as MethodInfo.Invoke().
        /// </remarks>
        /// <param name="handler">Handler describing the action to compile</param>
        /// <param name="compiledHandler">Type of message handled by the action</param>
        /// <returns>Compiled action to invoke the specified handler</returns>
        private Action<object> CompileHandlerInvocation(object handler, Type messageType)
        {
            // Get access to a MethodInfo instance describing the message
            // handler's "Handle" method. Some handlers may handle multiple
            // types of messages. So, the message type is specified to isolate 
            // the appropriate implementation of "Handle".

            var handleMethod = handler.GetType()
                                      .GetMethod("Handle", new Type[] { messageType });

            // Construct an expression describing the input parameter to 
            // the "Handle" method.

            var messageParameter = Expression.Parameter(typeof(object), "message");

            // Dynamically construct a lambda expression to invoke the "Handle"
            // method of the target message handler. In psuedo-lambda syntax,
            // the expression looks like:
            //
            //      (message) => 
            //          handler.Handle(([Message-Type])message)
            //
            // where [Message-Type] is the type of message model that was submitted.

            var handleExpression = Expression.Lambda<Action<object>>(
                Expression.Call(
                    Expression.Constant(handler),
                    handleMethod,
                    Expression.Convert(messageParameter, messageType)
                ),
                new ParameterExpression[] { messageParameter }
            );

            // Compile the expression to a delegate of type Action<object> 
            // and return it.

            return handleExpression.Compile();
        }

        /// <summary>
        /// Returns true if the action described by the specified message
        /// handler has been cached, false otherwise.
        /// </summary>
        /// <param name="handler">Handler describing the action</param>
        /// <returns>True if the action described by the specified message handler 
        /// has been cached, false otherwise</returns>
        private bool IsHandlerInvocationCached(string invocationCacheKey)
        {
            return this.invocationCache.ContainsKey(invocationCacheKey);
        }

        /// <summary>
        /// Retrieves a compiled handler action from the cache.
        /// </summary>
        /// <param name="handler">Handler describing the action to be retrieved from the cache</param>
        /// <returns>Compiled handler action</returns>
        private Action<object> GetCachedHandlerInvocation(string invocationCacheKey)
        {
            return this.invocationCache[invocationCacheKey];
        }
    }
}
