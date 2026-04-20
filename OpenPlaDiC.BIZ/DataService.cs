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
        // Obtener consulta dinámica: el servicio se encarga de mapear parámetros y ejecutar la consulta
        Task<Response<DataTable>> GetQueryAsync(string commandText, params GlobalItem[] parameters);
        // Ejecutar consulta dinámica: el servicio se encarga de mapear parámetros y ejecutar la consulta, devolviendo el número de filas afectadas
        Task<Response<int>> ExecQueryAsync(string commandText, params GlobalItem[] parameters);
        // Ejecución de procedimientos almacenados: el servicio se encarga de mapear parámetros y ejecutar el procedimiento, devolviendo un DataTable con los resultados
        Response<DataTable> ExecProc(string procName, params GlobalItem[] parameters);
        // Inserción dinámica: el servicio se encarga de calcular el Folio y asignar auditoría
        Task<Response<GlobalEntity>> InsertDataAsync(string tableName, Guid owner, params GlobalItem[] fields);
        // Eliminación lógica o física con trazabilidad
        Task<Response<bool>> DeleteDataAsync(Guid id, Guid actor, bool remove = false);
        // Actualización dinámica: el servicio se encarga de mapear campos, asignar auditoría y ejecutar la actualización
        Task<Response<bool>> UpdateDataAsync(Guid id, Guid actor, params GlobalItem[] fields);
        // Registro de eventos: el servicio se encarga registrar el evento en la tabla de auditoría
        Task<Response> CreateLogAsync(string infoEvent, Guid actor, string logType, string procedure = "");

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

        public async Task<Response<GlobalEntity>> InsertDataAsync(string tableName, Guid owner, params GlobalItem[] fields)
        {
            try
            {
                Guid newId = Guid.NewGuid();

                // Preparar inserción dinámica
                var fieldNames = new List<string> { "Id", "Propietario", "FechaCreacion", "EsDisponible", "EsEliminado" };
                var paramNames = new List<string> { "@Id", "@Propietario", "GETDATE()", "1", "0" };

                foreach (var field in fields)
                {
                    fieldNames.Add(field.Name);
                    paramNames.Add("@" + field.Name);
                }

                string sqlInsert = $"INSERT INTO {tableName} ({string.Join(", ", fieldNames)}) VALUES ({string.Join(", ", paramNames)})";

                // Ejecutar inserción
                var allParameters = fields.ToList();
                allParameters.Add(new GlobalItem("Id", newId.ToString()));
                allParameters.Add(new GlobalItem("Propietario", owner.ToString()));

                var result = await ExecQueryAsync(sqlInsert, allParameters.ToArray());

                if (result.IsSuccess)
                {
                    // Recuperar el folio generado por la base de datos
                    string sqlGetFolio = $"SELECT Folio FROM {tableName} WHERE Id = @Id";
                    var folioResult = await GetQueryAsync(sqlGetFolio, new GlobalItem("Id", newId.ToString()));

                    string folioGenerado = folioResult.Data.Rows[0]["Folio"].ToString();

                    return new Response<GlobalEntity>
                    {
                        IsSuccess = true,
                        Data = new GlobalEntity { Id = newId, Folio = folioGenerado }
                    };
                }

                return new Response<GlobalEntity> { Message = result.Message };
            }
            catch (Exception ex)
            {
                return new Response<GlobalEntity> { IsException = true, Message = ex.Message };
            }
        }

        public async Task<Response> CreateLogAsync(string infoEvent, Guid actor, string logType, string procedure = "")
        {
            try
            {
                // El objeto Evento debe estar registrado en la tabla Registro o tener su tabla física propia
                // Asumimos una tabla llamada 'Evento' con campos: Id, Propietario, FechaCreacion, RegistroId, Accion
                string sql = @"INSERT INTO Evento (Id, Propietario, FechaCreacion, RegistroId, Accion) 
                       VALUES (@Id, @Propietario, GETDATE(), @RegistroId, @Accion)";

                var parameters = new GlobalItem[] {
                    new GlobalItem("Id", Guid.NewGuid().ToString()),
                    new GlobalItem("Referencia", actor.ToString()),
                    new GlobalItem("InfoEvento", infoEvent),
                    new GlobalItem("Tipo", logType),
                    new GlobalItem("Procedimiento", procedure)
                };

                return await ExecQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                // En auditoría, si falla el log, quizás debamos registrarlo en el EventLog del sistema operativo
                return new Response { IsSuccess = false, IsException = true, Message = ex.Message };
            }
        }

        public async Task<Response<bool>> DeleteDataAsync(Guid id, Guid actor, bool remove = false)
        {
            try
            {

                string tableName = "";

                string sql;

                sql = $"SELECT * FROM Registro WHERE Id = @Id ";
                var responseRec = await GetQueryAsync(sql, new GlobalItem("Id", id.ToString()));


                if (responseRec.IsSuccess && responseRec.Data != null && responseRec.Data.Rows.Count > 0) 
                {
                    tableName = responseRec.Data.Rows[0]["Objeto"].ToString();
                }
                else
                {
                    return new Response<bool> { IsSuccess = false, Message = "Registro no encontrado" };
                }
                


                if (remove)
                {
                    // Eliminación física definitiva
                    sql = $"DELETE FROM {tableName} WHERE Id = @Id";
                }
                else
                {
                    // Eliminación lógica (estándar OpenPlaDiC)
                    sql = $"UPDATE {tableName} SET EsEliminado = 1, EsDisponible = 0, FechaModificacion = GETDATE() WHERE Id = @Id";
                }

                var response = await ExecQueryAsync(sql, new GlobalItem("Id", id.ToString()));

                if (response.IsSuccess)
                {
                    await CreateLogAsync(id.ToString(), actor, remove ? "DELETE_FISICO" : "DELETE_LOGICO", tableName);
                    return new Response<bool> { IsSuccess = true, Data = true };
                }

                return new Response<bool> { IsSuccess = false, Message = response.Message };
            }
            catch (Exception ex)
            {
                return new Response<bool> { IsSuccess = false, IsException = true, Message = ex.Message };
            }
        }

        public async Task<Response<bool>> UpdateDataAsync(Guid id, Guid actor, params GlobalItem[] fields)
        {
            try
            {

                string tableName = "";

                string sql;

                sql = $"SELECT * FROM Registro WHERE Id = @Id ";
                var responseRec = await GetQueryAsync(sql, new GlobalItem("Id", id.ToString()));


                if (responseRec.IsSuccess && responseRec.Data != null && responseRec.Data.Rows.Count > 0)
                {
                    tableName = responseRec.Data.Rows[0]["Objeto"].ToString();
                }
                else
                {
                    return new Response<bool> { IsSuccess = false, Message = "Registro no encontrado" };
                }


                // 2. Construir la cláusula SET dinámicamente
                var setClauses = fields.Select(f => $"{f.Name} = @{f.Name}").ToList();
                setClauses.Add("FechaModificacion = GETDATE()");

                sql = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE Id = @Id";

                // 3. Preparar parámetros
                var parameters = fields.ToList();
                parameters.Add(new GlobalItem("Id", id.ToString()));

                // 4. Ejecución
                var response = await ExecQueryAsync(sql, parameters.ToArray());

                if (response.IsSuccess)
                {
                    // Registrar auditoría en Evento
                    await CreateLogAsync(id.ToString(), actor, "UPDATE", tableName);

                    return new Response<bool> { IsSuccess = true, Data = true };
                }

                return new Response<bool> { IsSuccess = false, IsException = false, Message = response.Message };
            }
            catch (Exception ex)
            {
                return new Response<bool> { IsSuccess = false, IsException = true, Message = ex.Message };
            }
        }
    }

}
