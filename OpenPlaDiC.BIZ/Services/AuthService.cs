namespace OpenPlaDiC.BIZ.Services;

using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.DAL;
using OpenPlaDiC.DAL.Repositories;
using OpenPlaDiC.Entities.Models;
using OpenPlaDiC.Framework;

public interface IAuthService
{
    Task<Response<Users>> LoginAsync(string username, string password);
    Task<Response<Users>> FindByNameAsync(string username);
    Task<Response<string>> RequestPasswordResetAsync(string email, string newPass = "");
}

public class AuthService : IAuthService
{
    
    private readonly AppDbContext _appDbContext;


    public AuthService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Response<Users>> LoginAsync(string username, string password)
    {

        string hashPass = Framework.Helper.EncodePassword(username+password);

        var response = _appDbContext.Set<Users>().FirstOrDefault(x=>x.Username == username && x.Password == hashPass);

        if(response != null)
        {
            return new Response<Users>() { IsSuccess = true, Code = 200, Data = response };
        }
        else
        {
            return new Response<Users>() { IsSuccess = false, Code = 401, Message = "Credenciales inválidas" };
        }

    }

    public Task<Response<Users>> FindByNameAsync(string username)
    {

        var response = _appDbContext.Set<Users>().FirstOrDefault(x => x.Username.ToUpper() == username.ToUpper());


        if (response != null)
        {
            return Task.FromResult(new Response<Users>() { IsSuccess = true, Data = response });
        }
        else
        {
            return Task.FromResult(new Response<Users>() { IsSuccess = false, Message = "Usuario no encontrado" });
        }

    }

    public async Task<Response<string>> RequestPasswordResetAsync(string email, string newPass = "")
    {
        await Task.Delay(100);
        var response = _appDbContext.Set<Users>().FirstOrDefault(x => x.Email == email);

        if (response != null)
        {
            newPass = string.IsNullOrEmpty(newPass) ? Helper.CreateRandomPassword(4) : newPass;
            string hashPass = Helper.EncodePassword(response.Username + newPass);

            await _appDbContext.Users
                .Where(u => u.Id == response.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Password, hashPass));


            response.Password = hashPass;



            return new Response<string>() { IsSuccess = true, Data = newPass };
        }
        else
        {
            return new Response<string>() { Code = 500, Message = "Hubo un error. Intente de nuevo" };
        }

    }
}
