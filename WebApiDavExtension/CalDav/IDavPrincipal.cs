using System.Collections.Generic;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
    public interface IDavPrincipal : IDavCollectionResource
    {
        [PropFind("displayname", Namespace = "DAV:")]
        string DisplayName { get; set; }

        [PropFind("email-address", Namespace = "http://medocheck.com")]
        string EmailAddress { get; set; }

        /// <summary>
        /// Identifies the URL of any WebDAV collections that containcalendar collections owned by the associated principal resource.
        /// </summary>
        [PropFind("calendar-home-set", Namespace = "urn:ietf:params:xml:ns:caldav", IsComplex = true)]
        HRef CalendarHomeSet { get; set; }

        /// <summary>
        /// A principal may have many URLs, but there must be one "principal URL" that clients can use to uniquely identify a principal.
        /// This protected property contains the URL that MUST be used to identify this principal in an ACL request.
        /// </summary>
        [PropFind("principal-URL", Namespace = "DAV:", IsComplex = true)]
        HRef PrincipalUrl { get; set; }

        [PropFind("principal-collection-set", Namespace = "DAV:", IsComplex = true)]
        HRef PrincipalCollectionSet { get; }

        /// <summary>
        /// This property of a group principal identifies the principals that are direct members of this group.Since a group may be a 
        /// member of another group, a group may also have indirect members (i.e., the members of its direct members).
        /// </summary>
        [PropFind("group-member-set", Namespace = "DAV:")]
        List<HRef> GroupMembers { get; }

        /// <summary>
        /// This protected property identifies the groups in which the principal is directly a member.Note that a server may allow a 
        /// group to be a member of another group, in which case the DAV:group-membership of those other groups would need to be queried 
        /// in order to determine the groups in which the principal is indirectly a member.
        /// </summary>
        [PropFind("group-member-set", Namespace = "DAV:")]
        List<HRef> GroupMemberships { get; }

        /// <summary>
        /// Identify the calendar addresses of the associated principal resource.
        /// </summary>
        [PropFind("calendar-user-address-set", Namespace = "urn:ietf:params:xml:ns:caldav", IsComplex = true)]
        HRef CalendarUserAddressSet { get; set; }

        /// <summary>
        ///  Lists principals for whom the current principal is a read-only proxy for.
        /// </summary>
        [PropFind("calendar-proxy-read-for", Namespace = "CS:http://calendarserver.org/ns/", IsComplex = true)]
        HRef CalendarProxyReadFor { get; set; }

        /// <summary>
        /// Lists principals for whom the current principal is a read-write proxy for.
        /// </summary>
        [PropFind("calendar-proxy-write-for", Namespace = "CS:http://calendarserver.org/ns/", IsComplex = true)]
        HRef CalendarProxyWriteFor { get; set; }
    }
}
