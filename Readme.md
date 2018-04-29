# log4net.Ext.Json

[TOC]

## Enable JSON logging in log4net

Extend log4net with simple configuration options to create JSON log entries. Pass semantic information to downstream utilities, such as nxlog, LogStash, GrayLogs2 and similar.
Ideally, this will be integrated in log4net: <https://issues.apache.org/jira/browse/LOG4NET-419>
Events can be uniquely stamped and a keep alive appender can assure that your logging path is functional at all times.

### Features

- JSON logging
- Simple set up
- Easy configuration
- Seamless integration
- Uniquely stamped events
- Keep-alive appender to keep the log rolling even with minimal activity

## First steps

- Install the latest [nuget package of log4net.Ext.Json](https://www.nuget.org/packages/log4net.Ext.Json).
- [Configure log4net](https://logging.apache.org/log4net/release/manual/configuration.html)
- Use the `log4net.Layout.SerializedLayout, log4net.Ext.Json` as your appender's layout
- Log as you normally would

### Installation

You only need to reference the log4net.Ext.Json package. The log4net package will be installed as a dependency.

```bash
dotnet add package log4net.Ext.Json
```

### Configuration basics

Log4net has a nice page explaining [initial configuration](http://logging.apache.org/log4net/release/manual/configuration.html). I normally have this in my AssemblyInfo.cs or Program.cs:

```c#
// configure the log4net framework
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
```

log4net.config would then have the loggers and appenders, refer to [log4net examples](http://logging.apache.org/log4net/release/config-examples.html) for guidance. Please note that '...' here means you need to complete the configuration of the appender.

```xml
<log4net>
    <root>
        <level value='DEBUG'/>
        <appender-ref ref='MyAppender'/>
    </root>
    <appender name="MyAppender" ...>
    </appender>
</log4net>
```

I have a per class static field to hold the logger:

```c#
using log4net;
class MyClass {
	static private ILog log = LogManager.GetLogger(typeof(MyClass));
}
```

And then I log semantically come ca:

```c#
log.Info(new { A = 1, B = new { X = "Y" } });
```

### JSON stuff

To get a first shot with JSON, just change the layout type in your chosen appender (that supports layouts):

```xml
<appender...>
    <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
    </layout> 
</appender>
```

That will produce a JSON log, but the logged object message will not be JSON like. To do that, use the following:

```xml
<appender...>
    <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
        <decorator type='log4net.Layout.Decorators.StandardTypesDecorator, log4net.Ext.Json' />
        <default /> <!--explicit default members-->
        <remove value='message' /> <!--remove the default preformatted message member-->
        <member value='message:messageobject' /> <!--add raw message-->
    </layout> 
</appender>
```

If the log should go down to [nxlog](http://nxlog-ce.sourceforge.net/), try this instead:

```xml
<appender...>
    <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
        <decorator type='log4net.Layout.Decorators.StandardTypesFlatDecorator, log4net.Ext.Json' />
        <default value='nxlog' /> <!--explicit default members-->
        <remove value='Message' /> <!--remove the default preformatted message member-->
        <member value='Message:messageobject' /> <!--add raw message-->
    </layout> 
</appender>
```

Decorators modify (decorate) the logged JSON object.

## Newtonsoft: Json.NET

The log4net.Ext.Json project includes a home brew JSON serializer. This allows us to use the RendererMap, but it is quite a simple implementation. You may prefer the popular Json.NET serializer for various reasons. It is available in a separate package log4net.Ext.Json.Net not to bloat the project up with dependencies.

### Installation

You only need to reference the log4net.Ext.Json.Net package. The log4net.Ext.Json package will be installed as a dependency.

```bash
dotnet add package log4net.Ext.Json.Net
```

### Configuration

To enable Json.NET, change the SerializedLayout renderer:

```xml
<appender name="ConsoleAppenderJson" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.SerializedLayout, log4net.Ext.Json">
        <renderer type='log4net.ObjectRenderer.JsonDotNetRenderer, log4net.Ext.Json.Net'>
            <DateFormatHandling value="IsoDateFormat" />
            <NullValueHandling value="Ignore" />
            <Formatting value="Indented" />
        </renderer>
        <default />
        <remove value="message" />
        <member value="message:messageobject" />
    </layout>
</appender>
```
You can set basic `JsonSerializerSettings` [properties](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm) in the config file.

## Modify the object graph

To some extent, you can make the json object graph take a shape you like. This can be achieved using arrangements. 

### Members

The most common arrangement is a member arrangement:

```xml
<member value="messageobject" />
```

It will add a member to the object graph. There are many standard member names, such as `message`, `logger`, `level`as you already know from log4net. The `messageobject` value is the raw logged object as oposed to `message` which is already serialized by log4net. This is useful if you prefer to have the logged object be part of the event object graph.

You can change the name of a member with a `name:member` notation:

```xml
<member value="message:messageobject" />
```

Like that, the message object will be called just message:

```json
{"message":{"semantic":"log"}}
```

We have some sensible defaults such as `<default />`or `<default value="nxlog" />`. You can use that as a starting point and then modify the arrangement with `<remove value="membername" />` and `<member value="membername:membervalue"/>` .

We can take advantage of the log4net `PatternLayout` to format some more specific members. This is done with the `name|pattern notation`:

```xml
<member value="Day|It is %date{dddd} today" />
```

You can use that to add some static content easily:

```
<member value="Note|This is static content" />
```

We can also call a single pattern with an option. This is done with `name%pattern:option` notation:

```xml
<member value="Month%date:MMM" />
```

### More

There's much more to it. Check the `SerializedLayout` class for more examples.

## Stamps

When you roll your events into some central store, you'll want to have some unique key to avoid duplicates. This can be done using stamps that are generated by loggers (as opposed to appenders).

This is a serious production requirement. No messing. Should you ever need to replay your events to your log storage of choice for reprocessing, you can rely on a unique stamp produced by a modified logger. It is implemented at a logger level because if it was at the appender instead, there would be duplication defeating the requirement of uniqueness. To stamp your events, simply add a logger factory to your log4net config (the topmost element):

```xml
<log4net>                    
  <loggerFactory type='log4net.Util.Stamps.StampingLoggerFactory, log4net.Ext.Json'>
  </loggerFactory>
  ...
</log4net>
```

Stamps can be configured to your liking:

```xml
<log4net>                    
  <loggerFactory type='log4net.Util.Stamps.StampingLoggerFactory, log4net.Ext.Json'>
            ...
    <stamp type='log4net.Util.Stamps.ValueStamp, log4net.Ext.Json'>
      <name>stamp</name>
      <value>CustomValue</value>
    </stamp>
  </loggerFactory>
  ...
</log4net>
```

Explore the namespace log4net.Util.Stamps or add your own stamp implementing the IStamp interface.

To make the stamp show in JSON log, add a Stamp member like this to the serialized layout:

```xml
<appender ...>
  <layout type="log4net.Layout.SerializedLayout, log4net.Ext.Json">
    <default /> <!--explicit default members-->
    <member value="Stamp:stamp" />
  </layout>
</appender>
```

## Keep alive!

Keep-alive appender will come to rescue if your components produce nearly no logs during their production life time. How do you check that your app is alive and well? How do you force rolling file appenders to roll the files over? Easy: log every minute an "alive" entry! This keep alive appender allows you to do just that without modifying your app codes. It will pulse your connected appenders.

To get it going, add this to your log4net config:

```xml
<logger name="log4net.Appender.KeepAliveAppender" additivity="false">
  <appender-ref ref="KeepAliveAppender"/>
</logger>    
<appender name="KeepAliveAppender" type="log4net.Appender.KeepAliveAppender, log4net.Ext.Json" additivity="false">
  <keepaliveinterval value="60000" />
  <appender-ref ref="YourAppender"/>
</appender>
```

Note the "YourAppender" part. That's the name of the appender you want to pulse. You can add multiple appender references, too.

## Fare well

I hope you'll make the world a better place with this tool at hand, nothing less :D

Good luck and happy logging.