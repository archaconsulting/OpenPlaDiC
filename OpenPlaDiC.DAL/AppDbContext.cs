using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OpenPlaDiC.DAL
{
    public partial class AppDbContext : DbContext
    {
        private readonly string _connectionString;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public async Task<Response<DataTable>> GetQueryAsync(string commandText, params GlobalItem[] parameters)
        {
            var response = new Response<DataTable>();
            try
            {

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        AddParameters(command, parameters);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(reader);
                            return new Response<DataTable>
                            {
                                IsSuccess = true,
                                Data = dataTable,
                                Code = 200
                            };
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                SetErrorResponse(response, ex);
            }

            return response;
        }

        public async Task<Response<int>> ExecQueryAsync(string commandText, params GlobalItem[] parameters)
        {
            var response = new Response<int>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand(commandText, connection);
                AddParameters(command, parameters);

                connection.Open();
                response.Data = await command.ExecuteNonQueryAsync();
                response.IsSuccess = true;
                response.Code = 200;
            }
            catch (Exception ex)
            {
                SetErrorResponse(response, ex);
            }
            return response;
        }

        public Response<DataTable> ExecProc(string procName, OpenPlaDiC.Framework.GlobalItem[] parameters1, params GlobalItem[] parameters)
        {
            var response = new Response<DataTable>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand(procName, connection);
                command.CommandType = CommandType.StoredProcedure;
                AddParameters(command, parameters);

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                response.IsSuccess = true;
                response.Data = dataTable;
                response.Code = 200;
            }
            catch (Exception ex)
            {
                SetErrorResponse(response, ex);
            }
            return response;
        }

        /// <summary>
        /// Mapea los GlobalItem a parámetros de SQL usando Name y Value.
        /// </summary>
        private void AddParameters(SqlCommand command, GlobalItem[] parameters)
        {
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    // Asegura que el nombre comience con @
                    string paramName = item.Name.StartsWith("@") ? item.Name : "@" + item.Name;
                    command.Parameters.AddWithValue(paramName, string.IsNullOrEmpty(item.Value) ? DBNull.Value : item.Value);
                }
            }
        }

        /// <summary>
        /// Estandariza la respuesta en caso de error o excepción.
        /// </summary>
        private void SetErrorResponse(Response response, Exception ex)
        {
            response.IsSuccess = false;
            response.IsException = true;
            response.Message = ex.Message;
            response.Code = 500; // Código genérico de error interno
        }

    }
}
