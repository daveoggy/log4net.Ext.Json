---
layout: page
title: Intro
---

Welcome to log4net.Ext.Json pages! Check your [first steps] to start exploring the world of JSON logging. Consider [unique event stamps] or adding a [keep-alive appender] to keep your logs alive. To figure what to do with Arrangements, look at [Arrangement tests](https://sourceforge.net/p/log4net-json/code/HEAD/tree/trunk/log4net.Ext.Json.Tests/Layout/Arrangements/). Check the [news] for latest gossips.

This log4net extension allows your .net applications to produce JSON formatted logs through the log4net framework. It is currently implemented and run as a separate DLL and project, but it could very well become a feature of log4net itself in the future.

Downloads - The DLL is built against various log4net released/trunk versions in different .net frameworks. To safely identify the released log4net dll you use, make an md5sum of the binary and compare:

md5sum hash of log4net.dll        | build to use
--------------------------------- | --------------------------
b89cb7f3f1a1e2807e708f5435deb13d  | 1.2.10 oldkey net.2.0
42e02f10d65701b1ff92a4937bb24ed7  | 1.2.11 oldkey net.2.0
95b1db78784fa5968fa0c0615815a292  | 1.2.11 newkey net.2.0
996047633a94d54149c0968185673ab9  | 1.2.11 newkey net.3.5
4eb9b506b3454921782a60d40690ed37  | 1.2.13 newkey net.2.0
f69c36bbf1220bdd2a114f4abc6b0fc1  | 1.2.13 newkey net.3.5
f365257deaf2b5319beb47b84ac6cc25  | 1.2.13 newkey net.3.5.cp
31e73af0734f4328879c1d96cdc4658c  | 1.2.13 newkey net.4.0
trunk is built afresh on release. | 1.3 trunk net.4.0
if it's not on the list           | use one of the above or ask for a special build

The solution is unit-tested and bench-marked (bench-mark GUI included in source) against regular log4net, though it's still under development and it may show some quirks. Your feedback is most welcome and needed at this stage. To involve the etheric guardians,  [ask log4net.ext.json on StackOverflow](http://stackoverflow.com/questions/ask?title=log4net.ext.json&tags=log4net+json) and [send a note to Robajz](https://sourceforge.net/u/robajz/profile/send_message) to attend to it. See [what others have said on SO](http://stackoverflow.com/search?q=log4net+json). If diligence is your virtue, report bug and feature tickets on SF.

