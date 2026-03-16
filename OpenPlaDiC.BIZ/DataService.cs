using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace OpenPlaDiC.BIZ
{

    public interface IDataService
    {
        Task<Response<DataTable>> GetQueryAsync(string commandText, params GlobalItem[] parameters);
        Task<Response<int>> ExecQueryAsync(string commandText, params GlobalItem[] parameters);
        Response<DataTable> ExecProc(string procName, params GlobalItem[] parameters);

    }
    public class DataService : IDataService
    {

        private readonly AppDbContext _appDbContext;

        public DataService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public Response<DataTable> ExecProc(string procName, params GlobalItem[] parameters)
        {
            return _appDbContext.ExecProc(procName, parameters);
        }

        public async Task<Response<int>> ExecQueryAsync(string commandText, params GlobalItem[] parameters)
        {
            return await _appDbContext.ExecQueryAsync(commandText, parameters);
        }

        public async Task<Response<DataTable>> GetQueryAsync(string commandText, params GlobalItem[] parameters)
        {
            return await _appDbContext.GetQueryAsync(commandText, parameters);
        }

        public Response<T> GetEntity<T>(Guid Id) where T : class
        {

            try
            {

                var obj = _appDbContext.Set<T>().Find(Id);


                return new Response<T>() { IsSuccess = obj != null, Data = obj };



            }
            catch (Exception ex)
            {
                return new Response<T> { IsException = true, InnerException = (ex.InnerException != null ? ex.InnerException.Message : "NIE") };
            }

        }

        public Response DeleteEntity<T>(Expression<Func<T, bool>> predicate) where T : class
        {

            try
            {
                T obj;

                if (predicate != null)
                {

                    try
                    {
                        var x = _appDbContext.Set<T>().Where(predicate);

                        _appDbContext.Set<T>().RemoveRange(x);
                        _appDbContext.SaveChanges();
                        return new Response() { IsSuccess = true };
                    }
                    catch (Exception ex)
                    {

                        return new Response<T> { IsException = true, InnerException = (ex.InnerException != null ? ex.InnerException.Message : "NIE") };
                    }

                }
                else
                {
                    return new Response<T> { Message = "Debe especificar una condición" };
                }





            }
            catch (Exception ex)
            {
                return new Response<T> { IsException = true, InnerException = (ex.InnerException != null ? ex.InnerException.Message : "NIE") };
            }


        }

        public Response<T> GetEntity<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                string msg = "";
                T? obj;


                if (predicate != null)
                {
                    obj = _appDbContext.Set<T>().FirstOrDefault(predicate);
                }
                else
                {
                    obj = null;
                    msg = "No filter";
                }

                return new Response<T>() { IsSuccess = obj != null, Data = obj, Message = msg };
            }
            catch (Exception ex)
            {
                return new Response<T> { IsException = true, InnerException = (ex.InnerException != null ? ex.InnerException.Message : "NIE") };
            }
        }

        public Response<List<T>> GetEntityList<T>(Expression<Func<T, bool>>? predicate = null) where T : class
        {
            try
            {
                List<T> list;



                if (predicate != null)
                {
                    list = _appDbContext.Set<T>().Where(predicate).ToList();
                }
                else
                {
                    list = _appDbContext.Set<T>().ToList();
                }




                return new Response<List<T>>() { IsSuccess = list != null, Data = list };
            }
            catch (Exception ex)
            {
                return new Response<List<T>> { IsException = true, InnerException = (ex.InnerException != null ? ex.InnerException.Message : "NIE") };
            }
        }

        public Response UpdateEntity(Object obj)
        {


            try
            {
                _appDbContext.Entry(obj).State = EntityState.Modified;
                int rows = _appDbContext.SaveChanges();

                return new Response() { IsSuccess = true, Message = rows.ToString() };
            }
            catch (Exception ex)
            {
                return new Response { IsException = true, InnerException = (ex.InnerException != null ? ex.InnerException.Message : "NIE") };
            }



        }

        public Response InsertEntity(Object obj)
        {


            try
            {


                _appDbContext.Add(obj);
                _appDbContext.SaveChanges();

                return new Response() { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new Response { IsException = true, Message = ex.Message, InnerException = ex.InnerException != null ? ex.InnerException.Message : "" };
            }



        }


    }

}
