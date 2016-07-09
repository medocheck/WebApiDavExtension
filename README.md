# .Net WebApi Dav Extension

*this extension is in an early development state*
*only CalDav is already implemented*

An extension for the .Net WebApi to communicate with Dav (CalDav) clients.

**tested with**
* Thunderbird Lightning
* Android CalDav-sync
* Os X iCalendar
* IOS Calendar

## Get Started

1. Add WebApiDavExtension to your WebApi Project
2. Add config section to your Web.config

  ``` XML
  <section name="calDavConfiguration" type="WebApiCalDavExtension.Configuration.CalDavConfigurationSectionHandler, WebApiCalDavExtension" />
  ```

3. Add CalDav Configuration to your Web.config

  ```XML
  <calDavConfiguration davHeader="1, 2, access-control, calendar-access, access-control, calendarserver-principal-property-search" xmlns:d="DAV:" xmlns:cal="urn:ietf:params:xml:ns:caldav">
  		<supported-calendar-component-set namespace="urn:ietf:params:xml:ns:caldav">
  			<comp name="VEVENT" namespace="urn:ietf:params:xml:ns:caldav"></comp>
  		</supported-calendar-component-set>
  		<supported-report-set namespace="DAV:">
  			<supported-report>
  				<report>
  					<calendar-multiget />
  				</report>
  			</supported-report>
  			<supported-report>
  				<report>
  					<calendar-query />
  				</report>
  			</supported-report>
  			<supported-report>
  				<report>
  					<sync-collection />
  				</report>
  			</supported-report>
  		</supported-report-set>
      <global-privileges>
        <current-user-privilege-set namespace="DAV:">
          <privilege>
            <read-free-busy namespace="urn:ietf:params:xml:ns:caldav" />
          </privilege>
          <privilege>
            <read />
          </privilege>
          <privilege>
            <read-acl />
          </privilege>
          <privilege>
            <read-current-user-privilege-set />
          </privilege>
          <privilege>
            <write />
          </privilege>
          <privilege>
            <write-acl />
          </privilege>
          <privilege>
            <write-properties />
          </privilege>
          <privilege>
            <write-content />
          </privilege>
          <privilege>
            <bind />
          </privilege>
          <privilege>
            <unbind />
          </privilege>
          <privilege>
            <unlock />
          </privilege>
        </current-user-privilege-set>
      </global-privileges>
  		<allowed-http-methods>
  			<method>OPTIONS</method>
  			<method>GET</method>
  			<method>POST</method>
  			<method>PUT</method>
  			<method>DELETE</method>
  			<method>COPY</method>
  			<method>MOVE</method>
  			<method>PROPFIND</method>
  			<method>PROPPATCH</method>
  			<method>LOCK</method>
  			<method>UNLOCK</method>
  			<method>REPORT</method>
  		</allowed-http-methods>
  	</calDavConfiguration>
  	```

2. Add Authorisation Filters to your WebApiConfig.cs

  ```C#
  config.Filters.Add(new BasicAuthorizationFilterAttribute(false));
  config.Filters.Add(new DigestAuthorizationFilterAttribute());
  ```

3. Add Routes to your WebApiConfig.cs

  ```C#
  config.Routes.MapHttpRoute(
    name: "WebDavApi",
    routeTemplate: "api/Calendar/{*path}",
    defaults: new { controller = "Calendar" }
  );
  
  config.Routes.MapHttpRoute(
    name: "PrincipalRoute",
    routeTemplate: "api/Principals/{*path}",
    defaults: new { controller = "Principal" }
  );
  ```

4. Add Value Provider and log4net to your WebApiConfig

  ```C#
  config.Services.Add(typeof(ValueProviderFactory), new CalDavValueProviderFactory());
  log4net.Config.XmlConfigurator.Configure();
  ```

5. Create a Controller and derive it from CalDavController to handle your CalDav requests
6. if you want to use Authorization, add the authorization filter attributes to your class definition

  ```C#
  [BasicAuthorizationFilter] // Enable Basic authentication for this controller.
  [DigestAuthorizationFilter]
  [Authorize] // Require authenticated requests.
  public class CalendarController : CalDavController
  {...}
  ```
  
7. Create Model Classes

  * For a calendar model implement the ICalendarCollection interface
  * For en event model implement the ICalendarResource interface
  * For a principal model implement the IDavPrincipal interface
  
  Add a PropFind Attribute to Properties that should be returned by WebDav requests. The Propfind Attribute contains the name and the namespace of the requested WebDav Property. 

  By using the PropFind Attribute to mark WebDav Properties you can extend the Extension or create youre own Properties.
