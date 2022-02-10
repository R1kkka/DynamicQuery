using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DynamicQueryModel.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace DynamicQueryModel
{
    public partial class DynamicQuery<TItem> where TItem : class, new()
    {
        private static readonly string InitField = typeof(TItem).GetProperties().FirstOrDefault().Name;
        [Inject] private IJSRuntime jsRuntime { get; set; }
        [Parameter] public Action OnClickCallback { get; set; }

        [CascadingParameter] private List<QueryModel> QueryModelList { get; set; }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <typeparam name="T">枚举类</typeparam>
        /// <param name="i">枚举值</param>
        /// <returns></returns>
        private FieldInfo? GetFieldInfo<T>(object i) => typeof(T).GetField(Enum.GetName(typeof(T), i));

        /// <summary>
        /// 获取枚举的描述
        /// </summary>
        /// <typeparam name="T">枚举类</typeparam>
        /// <param name="i">枚举值</param>
        /// <returns></returns>
        private String GetDescription<T>(object i) =>
            (GetFieldInfo<T>(i).GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as
                DescriptionAttribute).Description;

        private static string GetDisplayName(PropertyInfo key) => key.GetCustomAttribute<DisplayAttribute>()?.Name;

        /// <summary>
        /// 判断类型是否是 T  
        /// </summary>
        /// <typeparam name="T">需要判断的类型</typeparam>
        /// <param name="j"></param>
        /// <returns></returns>
        private static bool PropertyTypeIsGenericType<T>(string j) =>
            typeof(TItem).GetProperty(j).PropertyType.IsGenericType
                ? typeof(TItem).GetProperty(j).PropertyType.GetGenericArguments()[0] == typeof(T)
                : typeof(TItem).GetProperty(j).PropertyType == typeof(T);

        /// <summary>
        /// 添加主查询(外部)逻辑 -- 内部逻辑对应子查询中的 AND OR
        /// </summary>
        private void AddOutsideLogic()
        {
            var data = new QueryModel()
            {
                OutsideLogic = OutsideLogic.And,
                QueryData = new List<QueryData>()
                { new QueryData()
                {
                    Key = InitField
                }
                },
                Index = QueryModelList.Count
            };
            QueryModelList.Add(data);
        }

        private async void DeleteOutsideLogic(QueryModel i)
        {
            if (QueryModelList.Count == 1 && QueryModelList[0].QueryData.Count == 1)
            {
                await jsRuntime.InvokeVoidAsync("AlertMessage", "查询条件不能为空");
            }
            if (QueryModelList.Count > 1)
            {
                QueryModelList.Remove(i);
            }
        }
        /// <summary>
        /// 添加内部逻辑 -- 子查询逻辑
        /// </summary>
        /// <param name="i"></param>
        private void AddInsideLogic(QueryModel i)
        {
            i.QueryData.Add(new QueryData()
            {
                Key = InitField
            });
        }
        /// <summary>
        /// 删去内部逻辑
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private async void DeleteInsideLogic(QueryModel i, QueryData j)
        {
            if (QueryModelList.Count > 1 || i.QueryData.Count > 1)
                i.QueryData.Remove(j);

            if (i.QueryData.Count < 1 && QueryModelList.Count > 1)
            {
                QueryModelList.Remove(i);
            }
        }
        /// <summary>
        /// 值
        /// </summary>
        /// <param name="e"></param>
        /// <param name="j"></param>
        private void OnValueChanged(ChangeEventArgs e, QueryData j)
        {
            j.Value = e.Value.ToString();
            j.LeftValue = null;
            j.RightValue = null;
        }
        /// <summary>
        /// between 左值
        /// </summary>
        /// <param name="e"></param>
        /// <param name="j"></param>
        private void OnLeftValueChanged(ChangeEventArgs e, QueryData j)
        {
            j.Value = null;
            j.LeftValue = e.Value.ToString();
        }
        /// <summary>
        /// between 右值
        /// </summary>
        /// <param name="e"></param>
        /// <param name="j"></param>
        private void OnRightValueChanged(ChangeEventArgs e, QueryData j)
        {
            j.RightValue = e.Value.ToString();
        }

        private async void Onclick()
        {
            if (OnClickCallback != null)
            {
                await Task.Run(() => OnClickCallback.Invoke());
            }
            else
            {
                await jsRuntime.InvokeVoidAsync("AlertMessage", JsonConvert.SerializeObject(QueryModelList));
            }
        }

        private void Reset()
        {
            QueryModelList = new List<QueryModel>()
            {
                new QueryModel()
                {
                    OutsideLogic = OutsideLogic.And,
                    QueryData = new List<QueryData>()
                    {
                        new QueryData()
                        {
                            Key = InitField
                        }
                    },
                    Index = 0
                }
            };
        }

    }
}