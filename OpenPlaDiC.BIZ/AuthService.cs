using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using OpenPlaDiC.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace OpenPlaDiC.BIZ
{
    public interface IAuthService
    {
        Task<Response<GlobalItem>> LoginAsync(string username, string password, string ip, string userAgent);
        Task<Response<GlobalItem>> FindByNameAsync(string username);
        Task<Response<string>> RequestPasswordResetAsync(string email, string newPass = "");
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Response<GlobalItem>> Login2Async(string username, string password)
        {
            // 1. Buscamos al usuario por su Username (Sin validar password aún)
            var user = await _context.Users
                .Include(u => u.UserProfiles)
                    .ThenInclude(up => up.Profile)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return new Response<GlobalItem> { IsSuccess = false, Code = 401, Message = "Invalid credentials" };

            // 2. Calculamos el hash usando el Salt guardado en su registro
            string hashInput = Helper.EncodePassword(password, user.PasswordSalt);

            // 3. Comparamos los hashes
            if (user.Password == hashInput)
            {
                return new Response<GlobalItem>
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = new GlobalItem
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Value = user.Username,
                        Text = user.Email,
                        Flag = user.IsMaster
                    }
                };
            }

            return new Response<GlobalItem> { IsSuccess = false, Code = 401, Message = "Invalid credentials" };
        }


// Actualizamos la interfaz
// Task<Response<GlobalItem>> LoginAsync(string username, string password, string ip, string userAgent);

    public async Task<Response<GlobalItem>> LoginAsync(string username, string password, string ip, string userAgent)
    {
        var log = new LoginLog { 
            Username = username, 
            IPAddress = ip, 
            UserAgent = userAgent, 
            LoginDate = DateTime.Now 
        };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        string hashPass = Helper.EncodePassword(password, user?.PasswordSalt ?? "");

        if (user != null && user.IsActive && user.Password == hashPass)
        {
            log.Status = "SUCCESS";
            log.UserId = user.Id;
            log.Message = "Acceso correcto";
            
            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync();

            return new Response<GlobalItem> { 
                IsSuccess = true, 
                Data = 
                
                new GlobalItem
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Value = user.Username,
                        Text = user.Email,
                        Flag = user.IsMaster
                    }
                                
            };
        }

        // Registro de fallo
        log.Status = "FAILED";
        log.Message = user == null ? "Usuario inexistente" : (!user.IsActive ? "Usuario inactivo" : "Contraseña incorrecta");
        
        _context.LoginLogs.Add(log);
        await _context.SaveChangesAsync();

        return new Response<GlobalItem> { IsSuccess = false, Message = "Credenciales inválidas", Code = 401 };
    }



        public async Task<Response<GlobalItem>> FindByNameAsync(string username)
        {
            try
            {
                var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToUpper() == username.ToUpper());

                if (user != null)
                {
                    return new Response<GlobalItem>
                    {
                        IsSuccess = true,
                        Data = new GlobalItem { Id = user.Id, Name = user.Name, Value = user.Username, Flag = user.IsMaster }
                    };
                }

                return new Response<GlobalItem> { IsSuccess = false, Message = "User not found" };
            }
            catch (System.Exception ex)
            {
                
                return new Response<GlobalItem>
                {
                    IsSuccess = false, IsException = true, Message = ex.Message
                    
                };            
            }
            
        }

        public async Task<Response<string>> RequestPasswordResetAsync(string email, string newPass = "")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                newPass = string.IsNullOrEmpty(newPass) ? Helper.CreateRandomPassword(6) : newPass;
                
                // Al actualizar, generamos un NUEVO Salt para mayor seguridad
                user.PasswordSalt = Helper.GenerateSalt();
                user.Password = Helper.EncodePassword(newPass, user.PasswordSalt);
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync(); // EF detecta los cambios y genera el UPDATE solo

                return new Response<string> { IsSuccess = true, Data = newPass };
            }

            return new Response<string> { IsSuccess = false, Message = "Email not found" };
        }
    }



    }
