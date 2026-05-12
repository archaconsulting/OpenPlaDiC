using System;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.BIZ;

public interface ISearchService
{
    Task<Response<List<Record>>> GlobalSearchAsync(string term);
}

public class SearchService : ISearchService
{
    private readonly AppDbContext _context;

    public SearchService(AppDbContext context) => _context = context;

    public async Task<Response<List<Record>>> GlobalSearchAsync(string term)
    {
        // Una sola consulta a la tabla maestra que ahora tiene todo lo necesario
        var results = await _context.Records
            .Include(r => r.Entity)
            .Where(r => r.Folio.Contains(term) || r.SearchContent.Contains(term))
            .OrderByDescending(r => r.CreatedAt)
            .Take(30)
            .ToListAsync();

        return new Response<List<Record>> 
        { 
            IsSuccess = true, 
            Data = results 
        };
    }
}