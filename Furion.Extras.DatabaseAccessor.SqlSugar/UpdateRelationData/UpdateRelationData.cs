using SqlSugar;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Extras.DatabaseAccessor.SqlSugar.RelationDataUpdate
{
    /// <summary>
    /// 更新关联数据表中的数据，适用于关系表C中存储多条表A主键与表C主键对应关系
    /// </summary>
    public static class UpdateRelationData
    {


        /// <summary>
        ///更新关联关系表中的记录
        /// </summary>    
        /// <typeparam name="T1">关联关系表实体，实体必须有id属性且具有主键特性，id必须是小写</typeparam>
        /// <param name="t_rep">数据库对象</param>
        /// <param name="dataObj">关联关系更新集合值</param>
        /// <param name="oldObj">关联关系历史集合值</param>
        /// <returns></returns>
        public static async Task UpdatebyID_Relation_Table<T1>(ISqlSugarRepository<T1> t_rep, List<T1> dataObj, List<T1> oldObj)
        //   where T : class, new()
        where T1 : class, new()
        {

            try
            {
                t_rep.Ado.BeginTran();
                // await t_rep.Context.Updateable(updateObj).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
                if (dataObj != null && dataObj.Any() && oldObj != null && oldObj.Any())
                {

                    //直接填充更新
                    if (dataObj.Count == oldObj.Count)
                    {
                        await UpdateEqualCountEntity(t_rep, dataObj, oldObj);
                    }
                    //添加   相同条数部分更新 多出来的添加
                    else if (dataObj.Count > oldObj.Count)//0 1 2  //0 1 2 3 4 5
                    {
                        //相同条数集合
                        List<T1> _equalCountList = new List<T1>();
                        //多出来的集合
                        List<T1> _addCountList = new List<T1>();

                        for (int i = 0; i < dataObj.Count; i++)
                        {
                            //多出来的起始索引   oldObj.Count
                            if (i < oldObj.Count)
                            {
                                _equalCountList.Add(dataObj[i]);
                            }
                            else
                            {
                                _addCountList.Add(dataObj[i]);
                            }
                        }
                        await UpdateEqualCountEntity(t_rep, _equalCountList, oldObj);
                        await t_rep.Context.Insertable(_addCountList).ExecuteCommandAsync();


                    }
                    //删除  相同条数部分更新  多出来的删除
                    else if (dataObj.Count < oldObj.Count)//0 1 2 3//0 1
                    {
                        //相同条数集合
                        List<T1> _equalCountList = new List<T1>();
                        //相同条数旧的记录集合
                        List<T1> _oldCountList = new List<T1>();

                        //删除的集合
                        List<T1> _delCountList = new List<T1>();
                        for (int i = 0; i < oldObj.Count; i++)
                        {
                            //删除的起始索引大于dataObj.Count
                            if (i < dataObj.Count)
                            {
                                _equalCountList.Add(dataObj[i]);
                                _oldCountList.Add(oldObj[i]);
                            }
                            else
                            {
                                _delCountList.Add(oldObj[i]);
                            }
                        }

                        await UpdateEqualCountEntity(t_rep, _equalCountList, _oldCountList);
                        await t_rep.Context.Deleteable(_delCountList).ExecuteCommandAsync();

                    }

                }
                t_rep.Ado.CommitTran();
            }
            catch (System.Exception)
            {
                t_rep.Ado.RollbackTran();
                throw;
            }


        }








        /// 更新相同条数的数据
        /// </summary>
        /// <typeparam name="T1">关联表实体</typeparam>
        /// <param name="t_rep">数据库资源</param>
        /// <param name="dataObj">更新的对象</param>
        /// <param name="oldObj">已有的对象</param>
        /// <returns></returns>
        private static async Task UpdateEqualCountEntity<T1>(ISqlSugarRepository<T1> t_rep, List<T1> dataObj, List<T1> oldObj)
            // where T : class, new()
            where T1 : class, new()
        {
            List<T1> updateDataList = new List<T1>();
            for (int i = 0; i < oldObj.Count; i++)
            {
                dynamic data_ex = new ExpandoObject();
                dynamic old_ex = new ExpandoObject();
                old_ex = oldObj[i];

                data_ex = dataObj[i];
                data_ex.id = old_ex.id;
                //此处必须使用T中带特性的主键进行更新，因为表达式中不允许使用动态类型
                T1 t1 = new T1();
                t1 = data_ex;
                updateDataList.Add(t1);
            }
            await t_rep.Context.Updateable(updateDataList).ExecuteCommandAsync();
        }

    }
}
