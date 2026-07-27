using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Framework;
using OpenPlaDiC.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OpenPlaDiC.DAL
{
    public partial class AppDbContext : DbContext
    {
        private readonly string _connectionString;


// Tablas fijas del Kernel mapeadas como objetos
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; } 

        public DbSet<Entity> Entities { get; set; }
        public DbSet<AccessControl> AccessControls { get; set; }
        public DbSet<EntityProperty> EntityProperties { get; set; }  
        public DbSet<DataType> DataTypes { get; set; }
        public DbSet<DynamicView> DynamicViews { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<SystemParameter> SystemParameters { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo de tablas con nombres reservados o específicos

            modelBuilder.Entity<LoginLog>().ToTable("LoginLog");

            modelBuilder.Entity<User>().ToTable("User");

            modelBuilder.Entity<Profile>().ToTable("Profile");
            
            modelBuilder.Entity<UserProfile>().ToTable("UserProfile");

            modelBuilder.Entity<Entity>().ToTable("Entity");
            modelBuilder.Entity<AccessControl>().ToTable("AccessControl");
            modelBuilder.Entity<EntityProperty>().ToTable("EntityProperty");
            modelBuilder.Entity<DataType>().ToTable("DataType");
            modelBuilder.Entity<DynamicView>().ToTable("DynamicView");
            modelBuilder.Entity<Record>().ToTable("Record");

            // Mapeo explícito de SystemParameter (debe coincidir con el nombre en SQL)
            modelBuilder.Entity<SystemParameter>().ToTable("SystemParameter");

            // También mapeamos la columna 'Key' por ser palabra reservada
            modelBuilder.Entity<SystemParameter>()
                .Property(p => p.Key)
                .HasColumnName("Key");


            // Configuración de la relación con EntityProperty
            modelBuilder.Entity<EntityProperty>()
            .HasOne<DataType>()
            .WithMany(dt => dt.EntityProperties)
            .HasForeignKey(ep => ep.DataTypeId);

            modelBuilder.Entity<EntityProperty>()
            .HasOne(ep => ep.Entity)
            .WithMany(e => e.Properties)
            .HasForeignKey(ep => ep.EntityId);

            // Configuración de llave compuesta para UserProfile
            modelBuilder.Entity<UserProfile>()
                .HasKey(up => new { up.UserId, up.ProfileId });


            modelBuilder.Entity<Record>()
            .HasOne(r => r.Entity)          // Record tiene UNA Entity
            .WithMany()                     // Entity NO necesita tener una lista de Records (opcional)
            .HasForeignKey(r => r.EntityId) // La FK es EntityId
            .OnDelete(DeleteBehavior.Restrict); // Evita borrados accidentales del padre


            // Asegúrate de que el mapeo a la tabla física esté correcto
            modelBuilder.Entity<UserProfile>().ToTable("UserProfile");
            
            // Llave compuesta
            modelBuilder.Entity<UserProfile>()
                .HasKey(up => new { up.UserId, up.ProfileId });


            modelBuilder.Entity<User>(entity =>
            {
                // Indica que el valor se genera al insertar y no debe enviarse en el INSERT
                entity.Property(e => e.Folio)
                    .ValueGeneratedOnAdd(); 

                // Opcional: Si quieres que sea estrictamente de solo lectura tras la creación
                entity.Property(e => e.Folio)
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            });                


        }

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

        public Response<DataTable> ExecProc(string procName, params GlobalItem[] parameters)
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

        public async Task<Response<DataTable>> ExecProcAsync(string procName, params GlobalItem[] parameters)
        {

            var response = new Response<DataTable>();
            try
            {

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(procName, connection))
                    {
                        AddParameters(command, parameters);
                        command.CommandType = CommandType.StoredProcedure;

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

                    //paramName = item.Name;
                    //command.Parameters.AddWithValue(paramName, string.IsNullOrEmpty(item.Value) ? DBNull.Value : item.Value);


                    if (item.Value is System.Text.Json.JsonElement jsonElement)
                    {
                        item.Value = jsonElement.ValueKind switch
                        {
                            System.Text.Json.JsonValueKind.String => jsonElement.GetString(),
                            System.Text.Json.JsonValueKind.Number => jsonElement.TryGetInt64(out var l) ? l : jsonElement.GetDouble(),
                            System.Text.Json.JsonValueKind.True => true,
                            System.Text.Json.JsonValueKind.False => false,
                            _ => jsonElement.GetRawText()
                        };
                    }




                    command.Parameters.AddWithValue(paramName, item.Value);
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
