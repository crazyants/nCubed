﻿using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using RaraAvis.nCubed.Core.Exceptions;
using RaraAvis.nCubed.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaraAvis.nCubed.EventSourcing.Core.Exceptions
{
    /// <summary>
    /// Class that processes EventSourcing exceptions.
    /// </summary>
    public static class EventSourcingExceptionProcessor
    {
        private enum POLICIES { ES_POLICY}
        private static readonly EventSourcingErrorEventSource Log = FrameworkLoggingFactory<EventSourcingErrorEventSource>.Instance;
        private static ExceptionManager exceptionManager = BuildExceptionManagerConfig();
        /// <summary>
        /// EntLib ExceptionManager.
        /// </summary>
        public static ExceptionManager ExceptionManager
        {
            get
            {
                return exceptionManager;
            }
        }
        /// <summary>
        /// Handles an exception given, and returns a processed exception, generally a <see cref="T:RaraAvis.nCubed.Core.Exceptions.SemanticException"/>
        /// </summary>
        /// <param name="exceptionToHandle">Exception that is going to be processed.</param>
        /// <param name="exceptionToThrow">Returns a <see cref="T:System.Exception"/>, it depends on configuration.</param>
        /// <returns>True if exception managed, false otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "It is used by EntLib Infrastructure")]
        public static bool HandleException(Exception exceptionToHandle, out Exception exceptionToThrow)
        {
            return ExceptionManager.HandleException(exceptionToHandle, POLICIES.ES_POLICY.ToString(), out exceptionToThrow);
        }
        /// <summary>
        /// Executes an <see cref="T:System.Action"/> and processes any exception raised.
        /// </summary>
        /// <param name="action">A <see cref="T:System.Action"/> to be processed.</param>
        public static void Process(Action action)
        {
            ExceptionManager.Process(action, POLICIES.ES_POLICY.ToString());
        }
        /// <summary>
        /// Executes a <see cref="T:System.Func`1"/> and processes any exception raised.
        /// </summary>
        /// <typeparam name="TResult">Result type to be returned.</typeparam>
        /// <param name="action">A <see cref="T:System.Func`1"/> to be processed.</param>
        /// <returns>The value returned by method.</returns>
        public static TResult Process<TResult>(Func<TResult> action)
        {
            return ExceptionManager.Process<TResult>(action, POLICIES.ES_POLICY.ToString());
        }
        private static ExceptionManager BuildExceptionManagerConfig()
        {
            var policies = new List<ExceptionPolicyDefinition>();

            var esPolicy = new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(typeof (Exception),
                    PostHandlingAction.ThrowNewException,
                    new IExceptionHandler[]
                     {
                        new SemanticLoggingErrorHandler(Log.LogException, typeof(XmlExceptionFormatter)),
                        new WrapHandler("{handlingInstanceID}",
                         typeof(SemanticException))
                     })
            };

            policies.Add(new ExceptionPolicyDefinition(POLICIES.ES_POLICY.ToString(), esPolicy));

            return new ExceptionManager(policies);
        }
    }

}
