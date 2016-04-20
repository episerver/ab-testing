using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.Dal
{
    /// <summary>
    /// Interface for basic repository functionality
    /// </summary>
    public interface IRepository : IDisposable
    {
        #region Member Properties
        #endregion

        #region Member Functions
        /// <summary>
        /// Persists modified objects to the repository immediately
        /// </summary>
        /// <returns>Count of objects changed</returns>
        int SaveChanges();

        /// <summary>
        /// Persists modified objects to the repository immediately
        /// </summary>
        /// <param name="retryCount">Number of times to retry the save</param>
        /// <returns>Count of objects changed</returns>
        int SaveChanges(int retryCount);

        /// <summary>
        /// Mark object as modified to be persisted when request is made.
        /// </summary>
        /// <param name="instance">Object to persist</param>
        void Save(object instance);

        /// <summary>
        /// Add the object to the repository
        /// </summary>
        /// <param name="instance">Instance of the object to be added.</param>
        void Add(object instance);

        void DeleteKpi(object id);

        /// <summary>
        /// Retrieves the entity object by Id from the repository
        /// </summary>
        /// <typeparam name="T">Type of entity to retrieve</typeparam>
        /// <param name="id">Id of the entity</param>
        /// <returns>Entity corresponding to the given id in the repository</returns>
        T GetById<T>(object id) where T : class;

        IDalKpi GetById(object id);

        /// <summary>
        /// Retrieves all entity objects of the given type from the repository
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to retrive</typeparam>
        /// <typeparam name="T">Type of entity to retrive</typeparam>
        /// <returns>IQueryable of all the entity object of the given type in the repository</returns>
        IQueryable<T> GetAll<T>() where T : class;

        IQueryable<IDalKpi> GetAll();

        /// <summary>
        /// Retrieves all entity objects of the given type from the repository as a list
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to retrive</typeparam>
        /// <typeparam name="T">Type of entity to retrive</typeparam>
        /// <returns>IList of all the entity object of the given type in the repository</returns>
        IList<T> GetAllList<T>() where T : class;

        /// <summary>
        /// Add a detached object to the repository
        /// </summary>
        /// <param name="instance">instance of the object to attach and add</param>
        void AddDetached(object instance);

        /// <summary>
        /// Update a detached object in the repository
        /// </summary>
        /// <param name="instance">instance of the object to attach and update</param>
        void UpdateDetached(object instance);

        #endregion

        #region Events
        #endregion

    }
}
