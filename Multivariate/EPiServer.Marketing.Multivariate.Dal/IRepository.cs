﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Marketing.Multivariate.Model;

namespace EPiServer.Marketing.Multivariate.Dal
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

        /// <summary>
        /// Deletes the object from the repository
        /// </summary>
        /// <param name="instance">Instance of the object to delete.</param>
        /// <typeparam name="T">Type of the object to delete.</typeparam>
        void Delete<T>(T instance) where T : class;

        void Delete(object id);

        /// <summary>
        /// Retrieves the entity object by Id from the repository
        /// </summary>
        /// <typeparam name="T">Type of entity to retrieve</typeparam>
        /// <param name="id">Id of the entity</param>
        /// <returns>Entity corresponding to the given id in the repository</returns>
        T GetById<T>(object id) where T : class;

        IMultivariateTest GetById(object id);

        /// <summary>
        /// Retrieves all entity objects of the given type from the repository
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to retrive</typeparam>
        /// <typeparam name="T">Type of entity to retrive</typeparam>
        /// <returns>IQueryable of all the entity object of the given type in the repository</returns>
        IQueryable<T> GetAll<T>() where T : class;

        IQueryable<IMultivariateTest> GetAll();

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
