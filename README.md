# pypn
Simple Pipeline Processing framework. 

## Overview
A pipeline consists of a series of layered stages, each stage providing an encapsulation of custom logic. 
You run commands on a pipeline and when you do so, each command is run individually 
through the pipeline's layered stages. A command is only run on a pipeline's stage if that stage recognises the command and its payload.

## Example Application
A pipeline framework can be used in many scenarios where you need to run a series of commands through a series of layered functionality. 
As such, you could view a pipeline as a factory assembly line - as an item passes through the assembly line, each check point (stage) will
perform some action on the item.

As another example, consider how you'd use a pipeline framework when building an ORM or CRUD-based solution.

|Stages  | Function             | Description
|--------| ---------------------| ------------
|Stage 1 | Logging              | Log all queries/requests/responses from database
|Stage 2 | Event Source Publish | If the command is an update/insert/delete command, publish an event to notify subscribers of data changes
|Stage 2 | Level 1 Session Cache| Cache data fetched from database into a local memory cache to minimize unecessary database calls (similar to what ORMs like NHibernate would do)
|Stage 3 | Level 2 Session Cache| Distributed cache using something like NCache, memcache, Redis, etc...
|Stage 4 | ORM DAL              | Core ORM code responsible for querying and updating data

## What makes this Pipeline framework unique
This pipeline framework supports the concept of sessions, Run-commands and PostRun-commands. Essentially, session commands can be used to begin (start), rollback (abort) or commit (end) transactions. Each stage implementation can provide its own interpretation of starting/aborting or ending a transaction.
When you run a command, a RunCommand is executed against each layered Stage from top to bottom. Once the last stage is reached, each stage is also given the opportunity to run a PostRunCommand in reverse order, from bottom to top. 
Stages within a pipeline can be stateful, and as such, complex stage implementations can also enlist behaviour as part of Run/PostRun commands to be finalised/scoped within the context of the aforementioned sessions. 

In addition, this framework makes a distinction between pipeline configuration versus a pipeline for executing commands. A pipeline is configured with a series of stages and command definitions. Essentially, this framework provides you with the ability to define adaptors in order to allow you to consume 3rd party code as pipeline stages, and to define and adapt commands and their payload to a format that can be consumed by heterogenous stage implementations. 
Once a pipeline configuration is in place, you can create one or more pipeline instances that can be used as the target for running commands and session commands.
