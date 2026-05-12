using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using OpenPlaDiC.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace OpenPlaDiC.BIZ;

public interface IDynamicViewService
{
    Task<Response<Guid>> SaveViewAsync(DynamicView view);
    Task<Response<bool>> DeleteViewAsync(Guid id);
    Task<Response<DynamicView>> GetByIdAsync(Guid id);
    Task<Response<DynamicView>> GetByNameAsync(string name);
    Task<IEnumerable<DynamicView>> GetAllAsync();
}


public class DynamicViewService : IDynamicViewService
{
    private readonly AppDbContext _context;
    private readonly string _customViewsPath;

    public DynamicViewService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        // Definimos la ruta física: /Views/Custom/
        _customViewsPath = Path.Combine(env.ContentRootPath, "Views", "Custom");
        
        if (!Directory.Exists(_customViewsPath))
            Directory.CreateDirectory(_customViewsPath);
    }

    public async Task<Response<Guid>> SaveViewAsync(DynamicView view)
    {
        try
        {
            // 1. Guardar en Base de Datos
            var existing = await _context.DynamicViews.FindAsync(view.Id);
            if (existing == null) {
                view.Id = view.Id == Guid.Empty ? Guid.NewGuid() : view.Id;
                _context.DynamicViews.Add(view);
            } else {
                _context.Entry(existing).CurrentValues.SetValues(view);
                existing.UpdatedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();

            // 2. Guardar archivo físico .cshtml
            // Usamos el Name de la vista como nombre de archivo
            string fileName = $"{view.Name}.cshtml";
            string filePath = Path.Combine(_customViewsPath, fileName);
            
            await File.WriteAllTextAsync(filePath, view.Content);

            return new Response<Guid> { IsSuccess = true, Data = view.Id };
        }
        catch (Exception ex)
        {
            return new Response<Guid> { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<Response<bool>> DeleteViewAsync(Guid id)
    {
        var view = await _context.DynamicViews.FindAsync(id);
        if (view != null)
        {
            // Eliminar archivo físico
            string filePath = Path.Combine(_customViewsPath, $"{view.Name}.cshtml");
            if (File.Exists(filePath)) File.Delete(filePath);

            _context.DynamicViews.Remove(view);
            await _context.SaveChangesAsync();
            return new Response<bool> { IsSuccess = true };
        }
        return new Response<bool> { IsSuccess = false };
    }
    
    public async Task<Response<DynamicView>> GetByIdAsync(Guid id)
    {
        try
        {
            // Buscamos la vista en la base de datos
            var view = await _context.DynamicViews.FindAsync(id);

            if (view == null)
            {
                return new Response<DynamicView> 
                { 
                    IsSuccess = false, 
                    Code = 404, 
                    Message = "Vista no encontrada." 
                };
            }

            return new Response<DynamicView> 
            { 
                IsSuccess = true, 
                Data = view, 
                Code = 200 
            };
        }
        catch (Exception ex)
        {
            return new Response<DynamicView> 
            { 
                IsSuccess = false, 
                IsException = true, 
                Message = ex.Message, 
                Code = 500 
            };
        }
    }


    public async Task<Response<DynamicView>> GetByNameAsync(string name)
    {
        try
        {
            // Buscamos la vista en la base de datos
            var view = await _context.DynamicViews.FirstAsync(x => x.Name.Equals(name));

            if (view == null)
            {
                return new Response<DynamicView> 
                { 
                    IsSuccess = false, 
                    Code = 404, 
                    Message = "Vista no encontrada." 
                };
            }

            return new Response<DynamicView> 
            { 
                IsSuccess = true, 
                Data = view, 
                Code = 200 
            };
        }
        catch (Exception ex)
        {
            return new Response<DynamicView> 
            { 
                IsSuccess = false, 
                IsException = true, 
                Message = ex.Message, 
                Code = 500 
            };
        }
    }


    public async Task<IEnumerable<DynamicView>> GetAllAsync()
    {
        // Retornamos la lista de todas las vistas registradas
        // Podrías agregar un .AsNoTracking() para mejorar el rendimiento ya que es solo lectura
        return await _context.DynamicViews
            .AsNoTracking()
            .OrderBy(v => v.Label)
            .ToListAsync();
    }

   
}
