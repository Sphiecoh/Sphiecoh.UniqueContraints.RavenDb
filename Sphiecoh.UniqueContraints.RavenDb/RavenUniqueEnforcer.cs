using Raven.Abstractions.Exceptions;
using Raven.Bundles.UniqueConstraints;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sphiecoh.UniqueContraints.RavenDb
{
    public class RavenUniqueEnforcer<T>
    {
        // All the properties that we want to be unique
        private List<LambdaExpression> _propertyExpressions = new List<LambdaExpression>();

        public void AddProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            _propertyExpressions.Add(propertyExpression);
        }
        public List<LambdaExpression> PropertyExpressions { get { return _propertyExpressions; } }
    }

    public interface IRavenUniqueInserter
    {
        void StoreUnique<T>(IDocumentSession session, T entity,RavenUniqueEnforcer<T> keyProperty);
    }

    public class RavenUniqueInserter : IRavenUniqueInserter
    {
        public void StoreUnique<T>(IDocumentSession session, T entity,RavenUniqueEnforcer<T> keyProperty)                              
        {
            if (session == null) throw new ArgumentNullException("session");
            if (keyProperty == null) throw new ArgumentNullException("keyProperty");
            if (entity == null) throw new ArgumentNullException("entity");

            var constraints = new List<UniqueConstraint>();

            keyProperty.PropertyExpressions.ForEach(x =>
            {
                var key = x.Compile().DynamicInvoke(entity).ToString();
                constraints.Add(new UniqueConstraint { PropName = key });
            });

            DoStore(session, entity, constraints);
        }

        private static void DoStore<T>(IDocumentSession session, T entity,List<UniqueConstraint> constraints)
        {
            var previousSetting = session.Advanced.UseOptimisticConcurrency;

            try
            {
                session.Advanced.UseOptimisticConcurrency = true;
                constraints.ForEach(constraint =>
                                                 {
                                                     session.Store(constraint, String.Format("UniqueConstraints/{0}/{1}", entity.GetType().Name, constraint.PropName));
                                                 });
                session.Store(entity);
                session.SaveChanges();
            }
            catch (ConcurrencyException)
            {
                // rollback changes so we can keep using the session
                session.Advanced.Evict(entity);
                constraints.ForEach(session.Advanced.Evict);
                throw;
            }
            finally
            {
                session.Advanced.UseOptimisticConcurrency = previousSetting;
            }
        }
    }
}