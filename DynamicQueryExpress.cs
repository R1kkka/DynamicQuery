using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynamicQueryModel.Model;

namespace DynamicQueryModel
{
    public static class DynamicQueryExpress
    {
        public static Expression<Func<T, bool>> GetDynamicQueryExpression<T>(this IEnumerable<QueryModel> list)
        {
            Expression Bep = null;

            var methud = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            ParameterExpression p = Expression.Parameter(typeof(T), "s");

            /*   lamda参考    
            *   s => s.DischargeChute > 10;
            *   
            *   ParameterExpression  s
            *   MemberExpression     s.DischargeChute
            *   ConstantExpression   10 
            * 
            */

            foreach (var item in list)
            {
                Expression Bep2 = null;
                foreach (var i in item.QueryData)
                {
                    if (i.Value != null)
                    {
                        //Type
                        PropertyInfo pi = typeof(T).GetProperty(i.Key);
                        TypeConverter converter = TypeDescriptor.GetConverter(pi.PropertyType);

                        //key
                        MemberExpression left = Expression.Property(p, pi);

                        //Value
                        ConstantExpression right = Expression.Constant(converter.ConvertFrom(i.Value), pi.PropertyType);

                        Expression express = i.FilterAction switch
                        {
                            // =
                            InsideAction.Equals => Expression.Equal(left, right),
                            // !=
                            InsideAction.NotEquals => Expression.NotEqual(left, right),
                            // < 
                            InsideAction.LessThan => Expression.LessThan(left, right),
                            // <=
                            InsideAction.LessAndEqualThan => Expression.LessThanOrEqual(left, right),
                            // >
                            InsideAction.GreaterThan => Expression.GreaterThan(left, right),
                            // >=
                            InsideAction.GreaterAndEqualThan => Expression.GreaterThanOrEqual(left, right),
                            // like
                            InsideAction.NotContains => Expression.Not(left.Contains(right)),
                            // not like
                            InsideAction.Contains => left.Contains(right),

                            _ => throw new NotImplementedException(),
                        };

                        if (Bep2 == null)
                        {
                            Bep2 = express;
                        }
                        else
                        {
                            Bep2 = i.InsideLogic switch
                            {
                                InsideLogic.And => Expression.AndAlso(Bep2, express),
                                InsideLogic.Or => Expression.OrElse(Bep2, express)
                            };
                        }

                    }
                    //between
                    else if (i.Value == null && i.LeftValue != null && i.RightValue != null)
                    {

                        //Type
                        PropertyInfo pi = typeof(T).GetProperty(i.Key);
                        TypeConverter converter = TypeDescriptor.GetConverter(pi.PropertyType);

                        //key
                        MemberExpression left1 = Expression.Property(p, pi);
                        MemberExpression left2 = Expression.Property(p, pi);

                        //Value
                        ConstantExpression right1 = Expression.Constant(converter.ConvertFrom(i.LeftValue), pi.PropertyType);
                        ConstantExpression right2 = Expression.Constant(converter.ConvertFrom(i.RightValue), pi.PropertyType);

                        Expression express1 = Expression.GreaterThanOrEqual(left1, right1);
                        Expression express2 = Expression.LessThanOrEqual(left2, right2);

                        var Bep3 = Expression.AndAlso(express1, express2);

                        if (Bep2 == null)
                        {
                            Bep2 = Bep3;
                        }
                        else
                        {
                            Bep2 = i.InsideLogic switch
                            {
                                InsideLogic.And => Expression.AndAlso(Bep2, Bep3),
                                InsideLogic.Or => Expression.OrElse(Bep2, Bep3)
                            };
                        }

                    }
                }

                if (Bep == null)
                {
                    Bep = Bep2;
                }
                else
                {
                    Bep = item.OutsideLogic switch
                    {
                        OutsideLogic.And => Expression.AndAlso(Bep, Bep2),
                        OutsideLogic.Or => Expression.OrElse(Bep, Bep2)
                    };
                }
                Console.WriteLine(Bep);

            }


            return Expression.Lambda<Func<T, bool>>(Bep, p);
        }


        public static Expression Contains(this Expression left, Expression right)
        {

            var method = typeof(string).GetMethod("Contains", new Type[1] { typeof(string) })!;
            return Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null)), Expression.Call(left, method, right));
        }
    }
}
