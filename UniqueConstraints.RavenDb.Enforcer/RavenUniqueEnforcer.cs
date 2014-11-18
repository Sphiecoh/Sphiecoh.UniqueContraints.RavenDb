using Raven.Abstractions.Exceptions;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UniqueConstraints.RavenDb.Enforcer
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
        void StoreUnique<T>(IDocumentSession session, T entity, RavenUniqueEnforcer<T> keyProperty);
    }

    public class RavenUniqueInserter : IRavenUniqueInserter
    {
        public void StoreUnique<T>(IDocumentSession session, T entity, RavenUniqueEnforcer<T> keyProperty)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (keyProperty == null) throw new ArgumentNullException("keyProperty");
            if (entity == null) throw new ArgumentNullException("entity");

            var constraints = new List<UniqueConstraint>();

            keyProperty.PropertyExpressions.ForEach(x =>
            {
                var value = x.Compile().DynamicInvoke(entity).ToString();
                var member = (MemberExpression)x.Body;
                constraints.Add(new UniqueConstraint { UniqueProperty = member.Member.Name, UniquePropertyValue = value });
            });

            DoStore(session, entity, constraints);
        }

        private static void DoStore<T>(IDocumentSession session, T entity, List<UniqueConstraint> constraints)
        {
            var previousSetting = session.Advanced.UseOptimisticConcurrency;

            try
            {
                session.Advanced.UseOptimisticConcurrency = true;
                constraints.ForEach(constraint =>
                                                 {
                                                     session.Store(constraint, String.Format("UniqueConstraints/{0}/{1}/{2}", entity.GetType().Name, constraint.UniqueProperty, constraint.UniquePropertyValue));
                                                 });
                session.Store(entity);
                session.SaveChanges();
            }
            catch (ConcurrencyException exception)
            {
                // rollback changes so we can keep using the session
                session.Advanced.Evict(entity);
                constraints.ForEach(session.Advanced.Evict);
                string[] strArray = exception.Message.Split(new[] { '\'', '/' });
                throw new UniqueConstraintViolationException(string.Format("Violation of unique constraint.There is already a '{0}' document with a property '{1}' containing value '{2}' .", strArray[2], strArray[3], strArray[4]));
            }
            finally
            {
                session.Advanced.UseOptimisticConcurrency = previousSetting;
            }
        }
    }
}