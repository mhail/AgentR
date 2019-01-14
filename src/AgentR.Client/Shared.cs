using System;
using System.Diagnostics;

namespace AgentR
{

    internal static class Constants
    {
        public const string ClientRegisterHandlersMethod = "registerHandlers";
        public const string HubRegisterHandlerMethod = "handeling";
        public const string HubAcceptRequestMethod = "accept";
        public const string HubReturnResponseMethod = "response";
        public const string HubReturnErrorMethod = "responseError";
        public const string HubAgentRequestMethod = "agentRequest";
        public const string HubAgentNotificationMethod = "agentNotification";
    }

    public class AgentHandlerRegistration
    {
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
    }

    public class AgentMethodRegistration
    {
        public string RequestMethod { get; set; }
    }
}