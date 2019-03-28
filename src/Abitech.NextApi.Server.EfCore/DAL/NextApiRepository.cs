﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.Entity;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Basic implementation of entity repository
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    /// <typeparam name="TDbContext">DbContext type</typeparam>
    public abstract class NextApiRepository<T, TKey, TDbContext> : INextApiRepository<T, TKey>
        where T : class
        where TDbContext : class, INextApiDbContext
    {
        private readonly TDbContext _context;
        private readonly DbSet<T> _dbset;
        private bool _isIEntity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        protected NextApiRepository(TDbContext dbContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbset = _context.Set<T>();
            _isIEntity = typeof(IEntity<TKey>).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Adds entity to dbset
        /// </summary>
        /// <param name="entity">entity instance</param>
        /// <returns></returns>
        public virtual async Task AddAsync(T entity)
        {
            await _dbset.AddAsync(entity);
        }

        /// <summary>
        /// Updates entity by instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        public virtual void Update(T entity)
        {
            _dbset.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Deletes item by entity instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        public virtual void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        /// <summary>
        /// Deletes items by condition
        /// </summary>
        /// <param name="where">delete condition</param>
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            var objects = _dbset.Where(where).AsEnumerable();
            foreach (var obj in objects)
                _dbset.Remove(obj);
        }

        /// <summary>
        /// Returns entity by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>entity</returns>
        public virtual async Task<T> GetByIdAsync(TKey id)
        {
            return await _dbset.FirstOrDefaultAsync(KeyPredicate(id));
        }

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> GetAll()
        {
            return _dbset.AsQueryable();
        }

        /// <summary>
        /// Given a entity and id, return true if entity's key property equals id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Expression<Func<T, bool>> KeyPredicate(TKey id)
        {
            if (!_isIEntity)
            {
                throw new NotSupportedException(
                    "Default implementation of KeyPredicate method supports only entities that implements IEntity<TKey>." +
                    "Override KeyPredicate for your entity type.");
            }

            return entity => (entity as IEntity<TKey>).Id.Equals(id);
        }

        /// <summary>
        /// Returns entity using where expression
        /// </summary>
        /// <param name="where">Filter expression</param>
        /// <returns>Filtered entity with includes</returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> where)
        {
            return await _dbset.FirstOrDefaultAsync<T>(where);
        }
    }
}