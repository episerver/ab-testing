using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.Migrations
{
    internal static class DatabaseVersion
    {
        internal const long RequiredDbVersion = 201705222044071;
        internal const string TableToCheckFor = "tblABTest";
        internal const string Schema = "dbo";
        internal const string ContextKey = "Testing.Migrations.Configuration";
    }
}
