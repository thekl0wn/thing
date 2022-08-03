using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    
    public static class Things
    {

        static Things()
        {
            DB = new ThingDatabase();
        }

        private static ThingDatabase DB;

        #region METHODS

        public static List<Guid> GetAllIds() { return GetAllIds(Guid.Empty, Guid.Empty, Guid.Empty); }
        public static List<Guid> GetAllIds(Guid type_id, Guid status_id, Guid repository_id)
        {
            // default
            var list = new List<Guid>();

            // sql
            var sql = new StringBuilder();
            var where = new StringBuilder();
            var where_list = new List<string>();
            sql.Append("SELECT [Id] FROM [thing].[Master]");

            // build the where clause
            if (type_id != Guid.Empty)
            {
                where.Clear();
                where.Append("[TypeId] = '");
                where.Append(type_id);
                where.Append("'");
                where_list.Add(where.ToString());
            }

            if(status_id != Guid.Empty)
            {
                where.Clear();
                where.Append("[StatusId] = '");
                where.Append(status_id);
                where.Append("'");
                where_list.Add(where.ToString());
            }

            if (repository_id != Guid.Empty)
            {
                where.Clear();
                where.Append("[RepositoryId] = '");
                where.Append(repository_id);
                where.Append("'");
                where_list.Add(where.ToString());
            }

            // add the where clause
            if (where_list.Count > 0)
            {
                sql.Append(" WHERE ");
                for(int i = 0; i < where_list.Count; i++)
                {
                    if (i > 0) sql.Append(" AND ");
                    sql.Append(where_list[i]);
                }
            }

            // return the list
            return list;
        }
        

        public static Guid GetId(string type_code, string thing_code)
        {
            // try for known first
            var id = GetKnownId(type_code, thing_code);
            if (id != Guid.Empty) return id;

            // get type id
            var type_id = Types.GetId(type_code);
            if(type_id == Guid.Empty)return Guid.Empty;

            // return from type id
            return GetId(type_id, thing_code);
        }
        public static Guid GetId(Guid type_id, string thing_code)
        {
            // default
            var id = Guid.Empty;

            // formatting
            thing_code = Formatting.ToCode(thing_code);

            // sql
            var sql = new StringBuilder();
            sql.Append("SELECT M.[Id] FROM [thing].[Master] M LEFT OUTER JOIN [thing].[Property] P ON P.[ThingId] = M.[Id] WHERE M.[TypeId] = '");
            sql.Append(type_id);
            sql.Append("' AND P.[PropertyId] = '");
            sql.Append(Properties.ID_CODE);
            sql.Append("'");

            // execute as scalar
            if (!DB.Scalar(sql.ToString(), out id)) return Guid.Empty;

            // return
            return id;
        }

        public static Guid GetKnownId(string type_code, string thing_code)
        {
            // default
            var id = Guid.Empty;

            // sql
            var sql = new StringBuilder();
            sql.Append("SELECT [Id] FROM [thing].[Known] WHERE [TypeCode] = '");
            sql.Append(Formatting.ToCode(type_code));
            sql.Append("' AND [ThingCode] = '");
            sql.Append(Formatting.ToCode(thing_code));
            sql.Append("'");

            // execute as scalar
            if (!DB.Scalar(sql.ToString(), out id)) return Guid.Empty;

            // return
            return id;
        }

        private static Guid GetMasterId(Guid thing_id, string field)
        {
            // default
            var id = Guid.Empty;

            // sql
            var sql = new StringBuilder();
            sql.Append("SELECT ['");
            sql.Append(field);
            sql.Append(")] FROM [thing].[Master] WHERE [Id] = '");
            sql.Append(thing_id);
            sql.Append("'");

            // execute as scalar
            if (!DB.Scalar(sql.ToString(), out id)) return Guid.Empty;

            // return
            return id;
        }
        public static Guid GetRepositoryId(Guid thing_id) { return GetMasterId(thing_id, "RepositoryId"); }
        public static Guid GetStatusId(Guid thing_id) { return GetMasterId(thing_id, "StatusId"); }
        public static Guid GetTypeId(Guid thing_id) { return GetMasterId(thing_id, "TypeId"); }

        #endregion

        #region SUB-CLASSES

        /// <summary>
        /// Contains common formatting methods/properties
        /// </summary>
        public static class Formatting
        {

            /// <summary>
            /// Converts passed code into proper code format
            /// </summary>
            public static string ToCode(string code)
            {
                code = code.Trim().ToUpper();
                code = code.Replace("  ", " ");
                code = code.Replace(" ", "_");
                return code;
            }

        }

        /// <summary>
        /// Contains the default repositories and used to create new repositories, and
        /// obtain existing repositories.
        /// </summary>
        public static class Repositories
        {

            static Repositories()
            {
                _New = new RepositoryThing(ID_NEW);
                _System = new RepositoryThing(ID_SYSTEM);
            }

            #region ID LISTINGS

            /// <summary>
            /// Get list of all active repositories
            /// </summary>
            public static List<Guid> GetActiveIds() { return GetAllIds( Statuses.ID_ACTIVE);}

            /// <summary>
            /// Get list of all repositories of all statuses
            /// </summary>
            public static List<Guid> GetAllIds() { return GetAllIds(Guid.Empty); }

            /// <summary>
            /// Get list of all repositories of passed status
            /// </summary>
            public static List<Guid> GetAllIds(Guid status_id) { return Things.GetAllIds(Types.ID_REPOSITORY, status_id, Guid.Empty); }

            #endregion

            public static Guid GetId(string type_code)
            {
                // check if known
                var id = GetKnownId(type_code);
                if (id != Guid.Empty) return id;

                // pull from things
                return Things.GetId(Types.ID_REPOSITORY, type_code);
            }

            public static Guid GetKnownId(string type_code) { return Things.GetKnownId("REPOSITORY", type_code); }

            public static Guid ID_NEW
            {
                get
                {
                    if (_ID_NEW == Guid.Empty) _ID_NEW = Types.GetKnownId("NEW");
                    return _ID_NEW;
                }
            }
            private static Guid _ID_NEW = Guid.Empty;

            public static Guid ID_SYSTEM
            {
                get
                {
                    if (_ID_SYSTEM == Guid.Empty) _ID_SYSTEM = Types.GetKnownId("SYSTEM");
                    return _ID_SYSTEM;
                }
            }
            private static Guid _ID_SYSTEM = Guid.Empty;

            /// <summary>
            /// All newly created things are placed her until assigned
            /// </summary>
            public static IRepositoryThing New { get { return _New; } }
            internal static RepositoryThing _New { get; }

            /// <summary>
            /// Collection of system things
            /// </summary>
            public static IRepositoryThing System { get { return _System; } }
            internal static RepositoryThing _System { get; }

        }

        /// <summary>
        /// Contains methods/properties for creating/obtaining different property data
        /// </summary>
        public static class Properties
        {

            #region ID LISTINGS

            /// <summary>
            /// Get list of active properties (all repos)
            /// </summary>
            public static List<Guid> GetActiveIds() { return GetAllIds(Statuses.ID_ACTIVE, Guid.Empty); }

            /// <summary>
            /// Get list of active properties for passed repo
            /// </summary>
            public static List<Guid> GetActiveIds(Guid repository_id) { return GetAllIds(Statuses.ID_ACTIVE, repository_id); }

            /// <summary>
            /// Get list of all properties of all statuses in all repositories
            /// </summary>
            public static List<Guid> GetAllIds() { return GetAllIds(Guid.Empty, Guid.Empty); }
            
            /// <summary>
            /// Get list of all properties for the given status & repository
            /// </summary>
            public static List<Guid> GetAllIds(Guid status_id, Guid repository_id) { return Things.GetAllIds(Types.ID_PROPERTY, status_id, repository_id); }

            /// <summary>
            /// Get all properties in the new repository
            /// </summary>
            public static List<Guid> GetNewIds() { return GetAllIds(Guid.Empty, Repositories.ID_NEW); }

            /// <summary>
            /// Get all active system properties
            /// </summary>
            public static List<Guid> GetSystemIds() { return GetSystemIds(Statuses.ID_ACTIVE); }

            /// <summary>
            /// Get system properties of passed status
            /// </summary>
            public static List<Guid> GetSystemIds(Guid status_id) { return GetAllIds(status_id, Repositories.ID_SYSTEM); }

            #endregion

            public static Guid GetId(string type_code)
            {
                // check if known
                var id = GetKnownId(type_code);
                if (id != Guid.Empty) return id;

                // pull from things
                return Things.GetId(Types.ID_PROPERTY, type_code);
            }

            public static Guid GetKnownId(string type_code) { return Things.GetKnownId("PROPERTY", type_code); }

            public static Guid ID_CODE
            {
                get
                {
                    if (_ID_CODE == Guid.Empty) _ID_CODE = Types.GetKnownId("CODE");
                    return _ID_CODE;
                }
            }
            private static Guid _ID_CODE = Guid.Empty;

            public static Guid ID_DESCRIPTION
            {
                get
                {
                    if (_ID_DESCRIPTION == Guid.Empty) _ID_DESCRIPTION = Types.GetKnownId("DESCRIPTION");
                    return _ID_DESCRIPTION;
                }
            }
            private static Guid _ID_DESCRIPTION = Guid.Empty;

            public static Guid ID_NAME
            {
                get
                {
                    if (_ID_NAME == Guid.Empty) _ID_NAME = Types.GetKnownId("NAME");
                    return _ID_NAME;
                }
            }
            private static Guid _ID_NAME = Guid.Empty;

        }

        /// <summary>
        /// Contains methods/properties for creating/obtaining different status data
        /// </summary>
        public static class Statuses
        {

            #region ID LISTINGS

            /// <summary>
            /// Get list of all active statuses for all repos
            /// </summary>
            public static List<Guid> GetActiveIds() { return GetActiveIds(Guid.Empty); }

            /// <summary>
            /// Get list of all active statuses for passed repo
            /// </summary>
            public static List<Guid> GetActiveIds(Guid repository_id) { return GetAllIds(Statuses.ID_ACTIVE, repository_id); }

            /// <summary>
            /// Get list of all statuses of all statuses and repositories
            /// </summary>
            public static List<Guid> GetAllIds() { return GetAllIds(Guid.Empty, Guid.Empty); }

            /// <summary>
            /// Get list of all statuses of passed status and repository
            /// </summary>
            public static List<Guid> GetAllIds(Guid status_id, Guid repository_id) { return Things.GetAllIds(Types.ID_STATUS, status_id, repository_id); }

            #endregion

            public static Guid GetId(string type_code)
            {
                // check if known
                var id = GetKnownId(type_code);
                if (id != Guid.Empty) return id;

                // pull from things
                return Things.GetId(Types.ID_STATUS, type_code);
            }

            public static Guid GetKnownId(string type_code) { return Things.GetKnownId("STATUS", type_code); }

            public static Guid ID_ACTIVE
            {
                get
                {
                    if (_ID_ACTIVE == Guid.Empty) _ID_ACTIVE = Types.GetKnownId("ACTIVE");
                    return _ID_ACTIVE;
                }
            }
            private static Guid _ID_ACTIVE = Guid.Empty;

            public static Guid ID_DEACTIVATED
            {
                get
                {
                    if (_ID_DEACTIVATED == Guid.Empty) _ID_DEACTIVATED = Types.GetKnownId("DEACTIVATED");
                    return _ID_DEACTIVATED;
                }
            }
            private static Guid _ID_DEACTIVATED = Guid.Empty;

        }

        /// <summary>
        /// Contains methods/properties for creating/obtaining different type data
        /// </summary>
        public static class Types
        {

            #region ID LISTINGS

            /// <summary>
            /// Get list of all active types for all repos
            /// </summary>
            public static List<Guid> GetActiveIds() { return GetActiveIds(Guid.Empty); }

            /// <summary>
            /// Get list of all active types for passed repo
            /// </summary>
            public static List<Guid> GetActiveIds(Guid repository_id) { return GetAllIds(Statuses.ID_ACTIVE, repository_id); }

            /// <summary>
            /// Get list of all types of all statuses and repositories
            /// </summary>
            public static List<Guid> GetAllIds() { return GetAllIds(Guid.Empty, Guid.Empty); }

            /// <summary>
            /// Get list of all types of passed status and repository
            /// </summary>
            public static List<Guid> GetAllIds(Guid status_id, Guid repository_id) { return Things.GetAllIds(Types.ID_TYPE, status_id, repository_id); }

            #endregion

            public static Guid GetId(string type_code)
            {
                // check if known
                var id = GetKnownId(type_code);
                if (id != Guid.Empty) return id;
                
                // pull from things
                return Things.GetId(Types.ID_TYPE, type_code);
            }

            public static Guid GetKnownId(string type_code) { return Things.GetKnownId("TYPE", type_code); }

            public static Guid ID_PROPERTY
            {
                get
                {
                    if (_ID_PROPERTY == Guid.Empty) _ID_PROPERTY = Types.GetKnownId("PROPERTY");
                    return _ID_PROPERTY;
                }
            }
            private static Guid _ID_PROPERTY = Guid.Empty;

            public static Guid ID_REPOSITORY
            {
                get
                {
                    if (_ID_REPOSITORY == Guid.Empty) _ID_REPOSITORY = GetKnownId("REPOSITORY");
                    return _ID_REPOSITORY;
                }
            }
            private static Guid _ID_REPOSITORY = Guid.Empty;

            public static Guid ID_STATUS
            {
                get
                {
                    if (_ID_STATUS == Guid.Empty) _ID_STATUS = GetKnownId("STATUS");
                    return _ID_STATUS;
                }
            }
            private static Guid _ID_STATUS = Guid.Empty;

            public static Guid ID_TYPE
            {
                get
                {
                    if (_ID_TYPE == Guid.Empty) _ID_TYPE = Types.GetKnownId("TYPE");
                    return _ID_TYPE;
                }
            }
            private static Guid _ID_TYPE = Guid.Empty;

        }

        #endregion

    }

}
