# AgentR
Remote Handler Agents for [MediatR](https://github.com/jbogard/MediatR) using SignalR Core

## MediatR + SignalR = AgentR

Q: Why would I need this? 

A: You want to handle mediator requests in a separate process/container/machine in order to:
 - Scale
 - Use different operating systems/frameworks.
 - Handle requests in different locations, including behind corporate firewalls.   

#FAQ

 Q: Is this ready for production?
 
 A: TLDR; No, but it is being developed and have great plans to test out the scenarios mentioned above. 

 Q: What is the current state?
 
 A: The sample runs and works, proving the technical feasibility. 
