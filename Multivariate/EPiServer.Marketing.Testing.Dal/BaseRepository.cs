using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Transactions;
using EPiServer.Marketing.Testing.Dal.Entity;

namespace EPiServer.Marketing.Testing.Dal
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
            int records = 0;
            bool retrySave = false;
            // save off retry count to potentially retry if there is an exception
            _retryCount = retryCount;
            try
            {
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
            }
            catch (EntityCommandExecutionException commandException)
            {
                var exception = String.Format(CultureInfo.InvariantCulture,
                                              "Database save failed with command exception error.  Exception: {0}",
                                              commandException.ToString());
                retrySave = false;
            }
            catch (EntityException entityException)
            {
                //TraceErr("Database save failed with entity exception error. Exception: {0}", entityException.ToString());
                //Wait half a second before moving on - these exceptions include connectivity issues
                Thread.Sleep(500);
                retrySave = true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    // mark entity as unchanged so we can retry if there are errors
                    if (validationErrors.Entry.State != EntityState.Added)
                    {
                        validationErrors.Entry.State = EntityState.Unchanged;
                    }

                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        //DataProcessingLog.TraceErr("Database save failed with validation error.  Property: {0} Error: {1}",validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                retrySave = true;
            }
            catch (DbUpdateConcurrencyException concurrencyException)
            {
                var exception = String.Format(CultureInfo.InvariantCulture,
                                              "Database save failed with concurrency exception. Exception: {0}",
                                              concurrencyException.ToString());

                var entries = concurrencyException.Entries;
                foreach (var entry in entries)
                {
                    //Refresh the object from the context, save again in retry
                    exception += String.Format(CultureInfo.InvariantCulture,
                                               "Refreshing entity from the context due to concurrency exception {0}.",
                                               entry.Entity.GetType().ToString());

                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
               // DataProcessingLog.TraceErr("{0}", exception);
                retrySave = true;
            }
            catch (OptimisticConcurrencyException concurrencyException)
            {
                var exception = String.Format(CultureInfo.InvariantCulture,
                                              "Database save failed with optimistic concurrency exception, retrying entire set of events. Exception: {0}",
                                              concurrencyException.ToString());
                //DataProcessingLog.TraceErr("{0}", exception);
                throw new Exception("Database save failed with optimistic concurrency exception, retrying entire set of events");
            }
            catch (DbUpdateException updateException)
            {
                var exception = String.Format(CultureInfo.InvariantCulture,
                                              "Database save failed with db update exception error.  Exception: {0}",
                                              updateException.ToString());

                //possible exceptions - primary key constraints violation - somebody else is trying to insert the same record at the same time
                var entries = updateException.Entries;
                foreach (var entry in entries)
                {
                    entry.State = EntityState.Unchanged;
                    exception += String.Format(CultureInfo.InvariantCulture,
                                               "-- Unable to update entity {0}.",
                                               entry.Entity.GetType().ToString());
                }

                var innerUpdateException = updateException.InnerException as UpdateException;
                if (innerUpdateException != null)
                {
                    var sqlException = innerUpdateException.InnerException as SqlException;
                    //We are now categorically dealing with a primary key violation (2627 is always the type)
                    const int primaryKeySqlError = 2627;
                    if (sqlException != null && sqlException.Number == primaryKeySqlError)
                    {
                        throw new Exception("Duplicate primary key detected - reprocessing.");
                    }
                }

                var concurrencyException = updateException.InnerException as OptimisticConcurrencyException;
                if (concurrencyException != null)
                {
                    throw new Exception("OptimisticConcurrencyException detected within the update exception handler.  Reprocessing set of events.");
                }

                retrySave = true;
            }
            finally
            {
                if (_retryCount > 0 && retrySave)
                {
                    _retryCount--;
                    SaveChanges(_retryCount);
                }
            }
            return records;
        }

        public IABTest GetById(object id)
        {
            return DatabaseContext.Set<ABTest>().Find(id);
        }

        public IQueryable<IABTest> GetAll()
        {
            return DatabaseContext.Set<ABTest>().AsQueryable();
        } 

        public void DeleteTest(object id)
        {
            var test = DatabaseContext.Set<ABTest>().Find(id);
            DatabaseContext.Set<ABTest>().Remove(test);
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
        private int _retryCount = 0;
        #endregion

        #region Internal Members
        public DatabaseContext DatabaseContext { get; private set; }

        #endregion
    }
}
