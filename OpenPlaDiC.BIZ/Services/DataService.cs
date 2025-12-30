using OpenPlaDiC.DAL;
using OpenPlaDiC.DAL.Repositories;
using OpenPlaDiC.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPlaDiC.BIZ.Services
{
    public interface IDataService
    {
        Response<DataTable> GetQuery(string commandText, params GlobalItem[] parameters);
        Response<int> ExecQuery(string commandText, params GlobalItem[] parameters);
        Response<DataTable> ExecProc(string procName, params GlobalItem[] parameters);
    }

    public class DataService : IDataService
    {

        private readonly DataAccessRepository _daRepository;    

        public DataService(DataAccessRepository appDbContext)
        {
            _daRepository = appDbContext;
        }

        public Response<DataTable> ExecProc(string procName, params GlobalItem[] parameters)
        {
            return _daRepository.ExecProc(procName, parameters);
        }

        public Response<int> ExecQuery(string commandText, params GlobalItem[] parameters)
        {
            return _daRepository.ExecQuery(commandText, parameters);
        }

        public Response<DataTable> GetQuery(string commandText, params GlobalItem[] parameters)
        {
            return _daRepository.GetQuery(commandText, parameters);
        }
    }
}
