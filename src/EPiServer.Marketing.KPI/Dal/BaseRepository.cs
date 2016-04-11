using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    [ExcludeFromCodeCoverage]
    public class BaseRepository : IRepository
    {
        #region Constants and Tables
        #endregion

        #region Constructors

        /// <summary>
        /// Build a new base repository with an EF Db context
        /// (recommended to be injected via Spring.NET)
        /// </summary>
        /// <param name="dbContext">EF Db Context to make queries from</param>
        public BaseRepository(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }
        #endregion

        #region Public Properties
        #endregion

        #region Public Member Functions


        /// <summary>
        /// Persists all changes made in DatabaseContext to the database
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public int SaveChanges()
        {
            //call save changes with no retries
            return SaveChanges(0);
        }

        /// <summary>
        /// Persists all changes made in DatabaseContext to the database
        /// </summary>
        /// <param name="retryCount">Number of times to retry save operation</param>
        /// <returns>Number of rows affected</returns>
        public int SaveChanges(int retryCount)
        {
            var records = 0;
      
            if (DatabaseContext != null)
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        IsolationLevel = IsolationLevel.ReadCommitted
                    }))
                {
                    records = DatabaseContext.SaveChanges();
                    scope.Complete();
                }
            }
            
            return records;
        }

        public IDalKpi GetById(object id)
        {
            return DatabaseContext.Set<DalKpi>().Find(id);
        }

        public IQueryable<IDalKpi> GetAll()
        {
            return DatabaseContext.Set<DalKpi>().AsQueryable();
        } 

        public void DeleteKpi(object id)
        {
            var test = DatabaseContext.Set<DalKpi>().Find(id);
            DatabaseContext.Set<DalKpi>().Remove(test);
        }

        /// <summary>
        /// Get a repository object by id from the ORM
        /// </summary>
        /// <typeparam name="T">Type of object to query for</typeparam>
        /// <param name="id">Id of the object to query for</param>
        /// <returns>Object of specified type with matching id. Returns null if there is no match
        /// in the repository.</returns>
        public T GetById<T>(object id) where T : class
        {
            return DatabaseContext.Set<T>().Find(id);
        }

        /// <summary>
        /// Marks the object as modified to the ORM so it will be saved when SaveChanges is called. Use for detached entities.
        /// </summary>
        /// <param name="instance">Instance of the objec to save</param>
        public void Save(object instance)
        {
            DatabaseContext.Entry(instance).State = EntityState.Modified;
        }

        /// <summary>
        /// Get all repository objects of the given type from the ORM.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve</typeparam>
        /// <returns>IQueryable of all objects matching the given type from the ORM.</returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            return DatabaseContext.Set<T>().AsQueryable<T>();
        }

        /// <summary>
        /// Get all repository objects of the given type from the ORM as a list.
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve</typeparam>
        /// <returns>IList of all objects matching the given type from the ORM.</returns>
        public IList<T> GetAllList<T>() where T : class
        {
            return GetAll<T>().ToList<T>();
        }

        /// <summary>
        /// Deletes the given object from the ORM.
        /// </summary>
        /// <param name="instance">Instance of the object to remove</param>
        public void Delete<T>(T instance) where T : class
        {
            DatabaseContext.Set<T>().Remove(instance as T);
        }

        /// <summary>
        /// Add the given object to the database context.
        /// </summary>
        /// <param name="instance"></param>
        public void Add(object instance)
        {
            if (instance != null)
            {
                DatabaseContext.Set(instance.GetType()).Add(instance);
            }
        }

        /// <summary>
        /// Add a detached object to the database context.
        /// </summary>
        /// <param name="instance"></param>
        public void AddDetached(object instance)
        {
            if (instance != null)
            {
                DatabaseContext.Set(instance.GetType()).Attach(instance);
                DatabaseContext.Entry(instance).State = EntityState.Added;
            }
        }

        /// <summary>
        /// Add a detached object to the database context.
        /// </summary>
        /// <param name="instance"></param>
        public void UpdateDetached(object instance)
        {
            if (instance != null)
            {
                DatabaseContext.Set(instance.GetType()).Attach(instance);
                DatabaseContext.Entry(instance).State = EntityState.Modified;
            }
        }


        #region IDisposable
        // Taken from: Implementing Finalize and Dispose to Clean Up
        // Unmanaged Resources
        // http://msdn.microsoft.com/en-us/library/b1yfkh5e(v=VS.100).aspx

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the repository by disposing of the current transaction and the database context after saving any changes.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    if (DatabaseContext != null)
                    {
                        DatabaseContext.Dispose();
                        DatabaseContext = null;
                    }
                }

                _disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~BaseRepository()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #endregion
        #endregion

        #region Private Member Variables
        private bool _disposed;
        #endregion

        #region Internal Members
        public DatabaseContext DatabaseContext { get; private set; }

        #endregion
    }
}
