using System;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;

namespace OpenPlaDiC.BIZ;

public interface ISystemParameterService
{
    Task<string> GetValueAsync(string key);
    Task<Response<bool>> UpdateValueAsync(string key, string value, Guid userId);
    Task<IEnumerable<SystemParameter>> GetAllParametersAsync();

}

public class SystemParameterService : ISystemParameterService
{
    private readonly AppDbContext _context;

    private static Dictionary<string, string> _cache = new Dictionary<string, string>();


    public SystemParameterService(AppDbContext context) => _context = context;

    public async Task<string> GetValueAsync(string key)
    {

        // 1. Intentar leer de caché
        if (_cache.ContainsKey(key)) return _cache[key];

        // 2. Si no está, ir a DB
        var param = await _context.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == key);
        
        var value = param?.Value ?? string.Empty;

        // 3. Guardar en caché para la próxima vez
        if (param != null) _cache[key] = value;

        return value;

    }

    public async Task<Response<bool>> UpdateValueAsync(string key, string value, Guid userId)
    {

        var param = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Key == key);
        if (param == null) return new Response<bool> { IsSuccess = false, Message = "No existe" };

        param.Value = value;
        param.UpdatedAt = DateTime.Now;
        param.UpdatedById = userId;

        await _context.SaveChangesAsync();

        // RECARGA: Limpiamos o actualizamos la caché inmediatamente
        _cache[key] = value; 

        return new Response<bool> { IsSuccess = true, Data = true };
    

    }

    public async Task<IEnumerable<SystemParameter>> GetAllParametersAsync()
    {
        return await _context.SystemParameters
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Key)
            .ToListAsync();
    }

            // Método para forzar recarga masiva
    public void ReloadAll() => _cache.Clear();


}