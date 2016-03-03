using System.Collections.Generic;
using System.Xml.Linq;
using WebApiDavExtension.Configuration;

namespace WebApiDavExtension.CalDav
{
    public class Principal : CalendarCollection
    {
        public Principal(string href, string calendarHref)
        {
            HRef = href;
            CalendarHomeSet = new HRef(calendarHref);

            PrincipalUrl = new HRef(href);
            CurrentUserPrincipal = new HRef(href);
            CalendarUserAddressSet = new HRef(href);
            CalendarProxyReadFor = new HRef(href);
            CalendarProxyWriteFor = new HRef(href);

            SupportedReportSet = new List<ReportType>()
            {
                new ReportType { ReportName = new XElement("expand-property") },
                new ReportType { ReportName = new XElement("principal-property-search") },
                new ReportType { ReportName = new XElement("principal-search-property-set") },
            };
        }

        [PropFind("displayname", Namespace = "DAV:")]
        public string DisplayName { get; set; }

        [PropFind("email-address", Namespace = "http://medocheck.com")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Identifies the URL of any WebDAV collections that containcalendar collections owned by the associated principal resource.
        /// </summary>
        [PropFind("calendar-home-set", Namespace = "urn:ietf:params:xml:ns:caldav", IsComplex = true)]
        public HRef CalendarHomeSet { get; set; }

        /// <summary>
        /// A principal may have many URLs, but there must be one "principal URL" that clients can use to uniquely identify a principal.
        /// This protected property contains the URL that MUST be used to identify this principal in an ACL request.
        /// </summary>
        [PropFind("principal-URL", Namespace = "DAV:", IsComplex = true)]
        public HRef PrincipalUrl { get; set; }

        [PropFind("principal-collection-set", Namespace = "DAV:", IsComplex = true)]
        public HRef PrincipalCollectionSet => new HRef("/WebDavPrototype/api/Principals");

        /// <summary>
        /// This property of a group principal identifies the principals that are direct members of this group.Since a group may be a 
        /// member of another group, a group may also have indirect members (i.e., the members of its direct members).
        /// </summary>
        [PropFind("group-member-set", Namespace = "DAV:")]
        public List<HRef> GroupMembers { get; } = new List<HRef>();

        /// <summary>
        /// This protected property identifies the groups in which the principal is directly a member.Note that a server may allow a 
        /// group to be a member of another group, in which case the DAV:group-membership of those other groups would need to be queried 
        /// in order to determine the groups in which the principal is indirectly a member.
        /// </summary>
        [PropFind("group-member-set", Namespace = "DAV:")]
        public List<HRef> GroupMemberships { get; } = new List<HRef>();

        /// <summary>
        /// Identify the calendar addresses of the associated principal resource.
          /// </summary>
        [PropFind("calendar-user-address-set", Namespace = "urn:ietf:params:xml:ns:caldav", IsComplex = true)]
        public HRef CalendarUserAddressSet { get; set; }

        /// <summary>
        ///  Lists principals for whom the current principal is a read-only proxy for.
        /// </summary>
        [PropFind("calendar-proxy-read-for", Namespace = "CS:http://calendarserver.org/ns/", IsComplex = true)]
        public HRef CalendarProxyReadFor { get; set; }

        /// <summary>
        /// Lists principals for whom the current principal is a read-write proxy for.
        /// </summary>
        [PropFind("calendar-proxy-write-for", Namespace = "CS:http://calendarserver.org/ns/", IsComplex = true)]
        public HRef CalendarProxyWriteFor { get; set; }


    }
}
