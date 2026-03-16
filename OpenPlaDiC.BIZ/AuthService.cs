using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.BIZ
{
    public interface IAuthService
    {
        Task<Response<GlobalItem>> LoginAsync(string username, string password);
        Task<Response<GlobalItem>> FindByNameAsync(string username);
        Task<Response<string>> RequestPasswordResetAsync(string email, string newPass = "");
    }


    public class AuthService : IAuthService
    {

        private readonly AppDbContext _appDbContext;

        public AuthService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Response<GlobalItem>> FindByNameAsync(string username)
        {

            var response = await _appDbContext.GetQueryAsync($"select * from usuario where upper(uname) = '{username}' ");


            if (response.IsSuccess && response.Data != null && response.Data.Rows.Count > 0 )
            {
                return new Response<GlobalItem>()
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = new GlobalItem()
                    {
                        Id = Guid.Parse(response.Data.Rows[0]["Id"].ToString()!),
                        Name = response.Data.Rows[0]["Nombre"].ToString()!,
                        Value = response.Data.Rows[0]["Uname"].ToString()!
                    }
                };
            }
            else
            {
                return new Response<GlobalItem>() { IsSuccess = false, Code = 401, Message = "Usuario no encontrado" };
            }


        }

        public async Task<Response<GlobalItem>> LoginAsync(string username, string password)
        {

            string hashPass = Framework.Helper.EncodePassword(password+username);

            var response = await _appDbContext.GetQueryAsync($"select * from usuario where uname = '{username}' and passwd = '{hashPass}' ");
                
            
            if (response.IsSuccess && response.Data != null && response.Data.Rows.Count > 0)
            {
                return new Response<GlobalItem>()
                {
                    IsSuccess = true,
                    Code = 200,
                    Data = new GlobalItem()
                    {
                        Id = Guid.Parse(response.Data.Rows[0]["Id"].ToString()!),
                        Name = response.Data.Rows[0]["Nombre"].ToString()!,
                        Value = response.Data.Rows[0]["Uname"].ToString()!,
                        Text = response.Data.Rows[0]["Correo"].ToString()!,
                    }
                };
            }
            else
            {
                return new Response<GlobalItem>() { IsSuccess = false, Code = 401, Message = "Credenciales inválidas" };
            }

        }

        public async Task<Response<string>> RequestPasswordResetAsync(string email, string newPass = "")
        {

            await Task.Delay(100);

            var response = await _appDbContext.GetQueryAsync($"select * from usuario where correo = '{email}' ");


            if (response.IsSuccess && response.Data != null && response.Data.Rows.Count > 0)
            {
                string uname = response.Data.Rows[0]["Uname"].ToString()!;

                newPass = string.IsNullOrEmpty(newPass) ? Helper.CreateRandomPassword(4) : newPass;
                string hashPass = Helper.EncodePassword(newPass+ uname);

                await _appDbContext.ExecQueryAsync($"update usuario set passwd = '{hashPass}' where Id = '{response.Data.Rows[0]["Id"].ToString()}' ");



                return new Response<string>() { IsSuccess = true, Data = newPass };
            }
            else
            {
                return new Response<string>() { Code = 500, Message = "Hubo un error. Intente de nuevo" };
            }


        }
    }


    }
